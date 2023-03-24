#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Quartzmin;

public class ViewEngine
{
    private readonly Services _services;
    private readonly Dictionary<string, Func<object, string>> _compiledViews = new(StringComparer.OrdinalIgnoreCase);

    public bool UseCache { get; set; }

    public ViewEngine(Services services)
    {
        _services = services;
        UseCache = string.IsNullOrEmpty(services.Options.ViewsRootDirectory!);
    }

    private Func<object, string> GetRenderDelegate(string templatePath)
    {
        if (!UseCache) return _services.Handlebars.CompileView(templatePath)!;
        
        lock (_compiledViews)
        {
            if (!_compiledViews.ContainsKey(templatePath) || _compiledViews[templatePath] is null)
            {
                _compiledViews[templatePath] = _services.Handlebars.CompileView(templatePath);
            }

            return _compiledViews[templatePath]!;
        }

    }

    public string Render(string templatePath, object model)
    {
        return GetRenderDelegate(templatePath)(model) ?? "Fault in core render engine (render)";
    }

    public string Encode(object value)
    {
        return _services.Handlebars.Configuration?.TextEncoder?.Encode(string.Format(CultureInfo.InvariantCulture, "{0}", value)) ?? "Fault in core render engine (encode)";
    }

    public string ErrorPage(Exception ex)
    {
        return Render("Error.hbs", new
        {
            ex.GetBaseException().GetType()?.FullName,
            Exception = ex,
            BaseException = ex.GetBaseException(),
            Dump = ex.ToString()
        });
    }
}