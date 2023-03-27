using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Quartzmin.Helpers;

/// <summary>
/// An action result which formats the given object as JSON.
/// </summary>
public class JsonXResult : ActionResult, IStatusCodeActionResult
{
    /// <summary>
    /// Creates a new <see cref="JsonResult"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to format as JSON.</param>
    public JsonXResult(object value)
    {
        ContentType = "application/json";
        Value = value;
    }

    /// <summary>
    /// Gets or sets the MediaTypeHeaderValue representing the Content-Type header of the response.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the value to be formatted.
    /// </summary>
    public object Value { get; set; }

    /// <inheritdoc />
    public override async Task ExecuteResultAsync(ActionContext context)
    {
        if (context?.HttpContext is null) throw new ArgumentNullException(nameof(context));

        var response = context.HttpContext.Response;

        await System.Text.Json.JsonSerializer.SerializeAsync(response!.Body!, Value);

        await response.Body.FlushAsync()!;
    }
}