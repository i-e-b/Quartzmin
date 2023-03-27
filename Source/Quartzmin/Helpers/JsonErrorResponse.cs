using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Quartzmin.Helpers;

public class JsonErrorResponseAttribute : ActionFilterAttribute
{
    //private static readonly JsonSerializerSettings _serializerSettings = new();

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context?.Exception != null)
        {
            context.Result = new JsonXResult(new { ExceptionMessage = context.Exception.Message }) { StatusCode = 400 };
            context.ExceptionHandled = true;
        }
    }
}