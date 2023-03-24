#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Quartzmin.Models;
using Quartz;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Quartzmin.Controllers;

public abstract partial class PageControllerBase : ControllerBase
{
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver(), // PascalCase as default
    };

    private HttpRequest GetRequest() => Request ?? throw new Exception($"Invalid controller instance: no '{nameof(Request)}' instance");

    protected Services Services => (Services)(GetRequest().HttpContext?.Items?[typeof(Services)] ?? throw new Exception("Invalid controller instance: no 'Services' instance in 'HttpContext.Items'"));
    protected string GetRouteData(string key) => RouteData?.Values?[key]?.ToString() ?? throw new Exception($"Invalid RouteData in {nameof(PageControllerBase)}");
    protected static IActionResult Json(object content) => new JsonResult(content, _serializerSettings);

    protected static IActionResult NotModified() => new StatusCodeResult(304);

    protected IEnumerable<string>? GetHeader(string key)
    {
        var values = Request?.Headers?[key];
        return string.IsNullOrEmpty(values!) ? (IEnumerable<string>?)null : values;
    }
}

public abstract partial class PageControllerBase
{
    protected IScheduler Scheduler => Services.Scheduler ?? throw new Exception($"Internal error: No scheduler in {nameof(PageControllerBase)}");

    protected dynamic ViewBag { get; } = new ExpandoObject();

    internal class Page
    {
        private readonly PageControllerBase _controller;

        public string ControllerName => _controller.GetRouteData("controller");

        public string ActionName => _controller.GetRouteData("action");

        public Services Services => _controller.Services;

        public object ViewBag => _controller.ViewBag;

        public object Model { get; set; }

        public Page(PageControllerBase controller, object? model = null)
        {
            _controller = controller;
            Model = model ?? new {};
        }
    }

    protected IActionResult View(object? model)
    {
        return View(GetRouteData("action"), model);
    }

    protected IActionResult View(string viewName, object? model)
    {
        var engine = Services.ViewEngine;
        if (engine is null) return StatusCode(500)!;
        var content = engine.Render($"{GetRouteData("controller")}/{viewName}.hbs", new Page(this, model));
        return Html(content);
    }

    protected static IActionResult Html(string? html)
    {
        return new ContentResult
        {
            Content = html,
            ContentType = "text/html",
        };
    }

    protected string? GetETag()
    {
        var values = GetHeader("If-None-Match");

        var first = values?.FirstOrDefault();
        if (first is null) return null;
        
        return new System.Net.Http.Headers.EntityTagHeaderValue(first).Tag;
    }

    public IActionResult TextFile(string content, string contentType, DateTime lastModified, string etag)
    {
        Response!.Headers!.Add("Last-Modified", lastModified.ToUniversalTime().ToString("R"));
        Response.Headers.Add("ETag", etag);
        return new ContentResult
        {
            Content = content,
            ContentType = contentType,
        };
    }

    protected JobDataMapItem JobDataMapItemTemplate => new()
    {
        SelectedType = Services.Options!.DefaultSelectedType,
        SupportedTypes = Services.Options.StandardTypes!.Order(),
    };
}