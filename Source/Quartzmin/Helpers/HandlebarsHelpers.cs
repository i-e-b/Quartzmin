#nullable enable
using HandlebarsDotNet;
using Quartzmin.Models;
using Quartzmin.TypeHandlers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using static Quartzmin.Controllers.PageControllerBase;

namespace Quartzmin.Helpers;

internal class HandlebarsHelpers
{
    private readonly Services _services;

    public HandlebarsHelpers(Services services)
    {
        _services = services;
    }

    public static void Register(Services services)
    {
        new HandlebarsHelpers(services).RegisterInternal();
    }

    private void RegisterInternal()
    {
        var h = _services.Handlebars;

        h.RegisterHelper("Upper", (o, _, a) => o.Write(a[0]?.ToString()?.ToUpper() ?? ""));
        h.RegisterHelper("Lower", (o, _, a) => o.Write(a[0]?.ToString()?.ToLower() ?? ""));
        h.RegisterHelper("LocalTimeZoneInfoId", (o, _, _) => o.Write(TimeZoneInfo.Local.Id));
        h.RegisterHelper("SystemTimeZonesJson", (o, c, _) => Json(o, c, TimeZoneInfo.GetSystemTimeZones().ToDictionary()));
        h.RegisterHelper("DefaultDateFormat", (o, _, _) => o.Write(DateTimeSettings.DefaultDateFormat));
        h.RegisterHelper("DefaultTimeFormat", (o, _, _) => o.Write(DateTimeSettings.DefaultTimeFormat));
        h.RegisterHelper("DoLayout", (_, c, _) => c.Layout());
        h.RegisterHelper("SerializeTypeHandler", (o, c, a) => o.WriteSafeString((a[0] as Services)?.TypeHandlers.Serialize((TypeHandlerBase)c) ?? ""));
        h.RegisterHelper("Disabled", (o, _, a) =>
        {
            if (IsTrue(a[0])) o.Write("disabled");
        });
        h.RegisterHelper("Checked", (o, _, a) =>
        {
            if (IsTrue(a[0])) o.Write("checked");
        });
        h.RegisterHelper("nvl", (o, _, a) => o.Write(a[a[0] is null ? 1 : 0] ?? ""));
        h.RegisterHelper("not", (o, _, a) => o.Write(IsTrue(a[0]) ? "False" : "True"));

        h.RegisterHelper(nameof(BaseUrl), (o, _, _) => o.WriteSafeString(BaseUrl));
        h.RegisterHelper(nameof(MenuItemActionLink), MenuItemActionLink);
        h.RegisterHelper(nameof(RenderJobDataMapValue), RenderJobDataMapValue);
        h.RegisterHelper(nameof(ViewBag), ViewBag);
        h.RegisterHelper(nameof(ActionUrl), ActionUrl);
        h.RegisterHelper(nameof(Json), Json);
        h.RegisterHelper(nameof(Selected), Selected);
        h.RegisterHelper(nameof(isType), isType);
        h.RegisterHelper(nameof(eachPair), eachPair);
        h.RegisterHelper(nameof(eachItems), eachItems);
        h.RegisterHelper(nameof(ToBase64), ToBase64);
        h.RegisterHelper(nameof(footer), footer);
        h.RegisterHelper(nameof(QuartzminVersion), QuartzminVersion);
        h.RegisterHelper(nameof(Logo), Logo);
        h.RegisterHelper(nameof(ProductName), ProductName);
    }

    private static bool IsTrue(object? value) => value?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    private string HtmlEncode(object? value)
    {
        if (value is null) return "";
        return _services.ViewEngine.Encode(value);
    }

    private string UrlEncode(string? value) => HttpUtility.UrlEncode(value ?? "");

    private string BaseUrl
    {
        get
        {
            var url = _services.Options.VirtualPathRoot;
            if (!url.EndsWith("/"))
                url += "/";
            return url;
        }
    }

    private string AddQueryString(string uri, IEnumerable<KeyValuePair<string, object>>? queryString)
    {
        if (queryString is null) return uri;

        var anchorIndex = uri.IndexOf('#');
        var uriToBeAppended = uri;
        var anchorText = "";
        // If there is an anchor, then the query string must be inserted before its first occurence.
        if (anchorIndex != -1)
        {
            anchorText = uri.Substring(anchorIndex);
            uriToBeAppended = uri.Substring(0, anchorIndex);
        }

        var queryIndex = uriToBeAppended.IndexOf('?');
        var hasQuery = queryIndex != -1;

        var sb = new StringBuilder();
        sb.Append(uriToBeAppended);

        foreach (var parameter in queryString)
        {
            sb.Append(hasQuery ? '&' : '?');
            sb.Append(UrlEncode(parameter.Key));
            sb.Append('=');
            if (parameter.Value is not null)
            {
                sb.Append(UrlEncode(string.Format(CultureInfo.InvariantCulture, "{0}", parameter.Value)));
            }

            hasQuery = true;
        }

        sb.Append(anchorText);
        return sb.ToString();
    }

    private void ViewBag(TextWriter output, dynamic context, params object[] arguments)
    {
        var dict = (IDictionary<string, object>)arguments[0];
        var viewBag = (IDictionary<string, object>)context.ViewBag;

        foreach (var pair in dict)
        {
            viewBag[pair.Key] = pair.Value;
        }
    }

    private void MenuItemActionLink(TextWriter output, dynamic context, params object[] arguments)
    {
        var dict = arguments[0] as IDictionary<string, object> ?? new Dictionary<string, object> { ["controller"] = arguments[0] };

        var classes = "item";
        if (dict["controller"]?.Equals(context.ControllerName) == true)
            classes += " active";

        var url = BaseUrl + dict["controller"];
        var title = HtmlEncode(dict.GetValue("title", dict["controller"]));

        output.WriteSafeString($@"<a href=""{url}"" class=""{classes}"">{title}</a>");
    }

    private void ActionUrl(TextWriter output, dynamic context, params object[] arguments)
    {
        if (arguments.Length < 1 || arguments.Length > 3)
            throw new ArgumentOutOfRangeException(nameof(arguments));

        IDictionary<string, object>? routeValues = null;
        string? controller = null;
        var action = (arguments[0] as Page)?.ActionName ?? (string)arguments[0];

        if (arguments.Length >= 2) // [actionName, controllerName/routeValues ]
        {
            if (arguments[1] is IDictionary<string, object> r)
                routeValues = r;
            else if (arguments[1] is string s)
                controller = s;
            else if (arguments[1] is Page v)
                controller = v.ControllerName;
            else
                throw new Exception("ActionUrl: Invalid parameter 1");
        }

        if (arguments.Length == 3) // [actionName, controllerName, routeValues]
            routeValues = (IDictionary<string, object>)arguments[2];

        if (controller == null)
            controller = context.ControllerName;

        var url = BaseUrl + controller;

        if (!string.IsNullOrEmpty(action))
            url += "/" + action;

        output.WriteSafeString(AddQueryString(url, routeValues));
    }

    private void Selected(TextWriter output, dynamic context, params object?[] arguments)
    {
        string? selected;
        if (arguments.Length >= 2)
            selected = arguments[1]?.ToString();
        else
            selected = context["selected"].ToString();

        if (selected is null)
        {
            if (arguments[0] is null)
            {
                output.Write("selected");
            }
        }
        else if ((arguments[0] as string)?.Equals(selected, StringComparison.InvariantCultureIgnoreCase) == true)
        {
            output.Write("selected");
        }
    }

    private void Json(TextWriter output, dynamic context, params object[] arguments)
    {
        output.WriteSafeString(Newtonsoft.Json.JsonConvert.SerializeObject(arguments[0]));
    }

    private void RenderJobDataMapValue(TextWriter output, dynamic context, params object[] arguments)
    {
        var item = (JobDataMapItem)arguments[1];
        output.WriteSafeString(item.SelectedType?.RenderView((Services)arguments[0], item.Value!) ?? "");
    }

    private void isType(TextWriter writer, HelperOptions options, dynamic context, params object?[] arguments)
    {
        Type[] expectedType;

        if (arguments.Length < 2) throw new ArgumentException("Invalid type");
        var strType = arguments[1] as string;

        switch (strType)
        {
            case "IEnumerable<string>":
                expectedType = new[] { typeof(IEnumerable<string>) };
                break;
            case "IEnumerable<KeyValuePair<string, string>>":
                expectedType = new[] { typeof(IEnumerable<KeyValuePair<string, string>>) };
                break;
            default:
                throw new ArgumentException("Invalid type: " + strType);
        }

        var t = arguments[0]?.GetType();

        if (t is not null && expectedType.Any(x => x.IsAssignableFrom(t)))
            options.Template?.Invoke(writer, (object)context);
        else
            options.Inverse?.Invoke(writer, (object)context);
    }

    private void eachPair(TextWriter writer, HelperOptions options, dynamic context, params object[] arguments)
    {
        void OutputElements<T>()
        {
            if (arguments[0] is not IEnumerable<T> pairs) return;
            foreach (var item in pairs)
            {
                if (item is null) continue;
                options.Template?.Invoke(writer, item);
            }
        }

        OutputElements<KeyValuePair<string, string>>();
        OutputElements<KeyValuePair<string, object>>();
    }

    private void eachItems(TextWriter writer, HelperOptions options, dynamic context, params object[] arguments)
    {
        eachPair(writer, options, context, ((dynamic)arguments[0]).GetItems());
    }

    private void ToBase64(TextWriter output, dynamic context, params object?[] arguments)
    {
        if (arguments.Length > 0 && arguments[0] is byte[] bytes)
            output.Write(Convert.ToBase64String(bytes));
    }

    private void footer(TextWriter writer, HelperOptions options, dynamic context, params object[] arguments)
    {
        IDictionary<string, object> viewBag = context.ViewBag;

        if (viewBag.TryGetValue("ShowFooter", out var show) && (bool)show)
        {
            options.Template?.Invoke(writer, (object)context);
        }
    }

    private void QuartzminVersion(TextWriter output, dynamic context, params object[] arguments)
    {
        var v = GetType().Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();
        if (v?.InformationalVersion is not null) output.Write(v.InformationalVersion);
    }

    private void Logo(TextWriter output, dynamic context, params object[] arguments)
    {
        output.Write(_services.Options.Logo);
    }

    private void ProductName(TextWriter output, dynamic context, params object[] arguments)
    {
        output.Write(_services.Options.ProductName);
    }
}