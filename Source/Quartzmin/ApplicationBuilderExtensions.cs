#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Quartzmin;

public static class ApplicationBuilderExtensions
{
    public static void UseQuartzmin(this IApplicationBuilder app, QuartzminOptions options, Action<Services>? configure = null, bool addRoutes = true)
    {
        options = options ?? throw new ArgumentNullException(nameof(options));

        app.UseFileServer(options);

        var services = Services.Create(options);
        configure?.Invoke(services);

        app.Use(async (context, next) =>
        {
            if (context?.Items is null) throw new Exception("Cannot build application: context is null");
            context.Items[typeof(Services)] = services;
            await (next?.Invoke() ?? Task.CompletedTask);
        });
            
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                if (context?.Response is null) return;
                var ex = context.Features?.Get<IExceptionHandlerFeature>()?.Error ?? new Exception("Unable to determine fault");
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(services.ViewEngine.ErrorPage(ex))!;
            });
        });
        
        if (addRoutes)
        {
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: nameof(Quartzmin),
                    template: "{controller=Scheduler}/{action=Index}");
            });
        }
    }

    private static void UseFileServer(this IApplicationBuilder app, QuartzminOptions options)
    {
        IFileProvider fs;
        if (string.IsNullOrEmpty(options.ContentRootDirectory!))
            fs = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Content");
        else
            fs = new PhysicalFileProvider(options.ContentRootDirectory!);

        var fsOptions = new FileServerOptions
        {
            RequestPath = new PathString("/Content"),
            EnableDefaultFiles = false,
            EnableDirectoryBrowsing = false,
            FileProvider = fs
        };

        app.UseFileServer(fsOptions);
    }

    public static void AddQuartzmin(this IServiceCollection services)
    {
        services.AddMvcCore()
            ?.AddApplicationPart(Assembly.GetExecutingAssembly());
            //?.AddJsonFormatters(); // (?) This is causing "Could not load type 'Microsoft.Extensions.DependencyInjection.MvcJsonMvcCoreBuilderExtensions' from assembly 'Microsoft.AspNetCore.Mvc.Formatters.Json"
            // See: https://github.com/aspnet/Announcements/issues/325
            //      https://github.com/dotnet/aspnetcore/issues/3612
    }

}