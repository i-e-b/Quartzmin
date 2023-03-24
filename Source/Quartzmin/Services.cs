#nullable enable
using System;
using HandlebarsDotNet;
using Quartz;
using Quartzmin.Helpers;

namespace Quartzmin;

public class Services
{
    internal const string ContextKey = "quartzmin.services";

    public QuartzminOptions Options { get; set; }

    public ViewEngine ViewEngine { get; set; }

    public IHandlebars Handlebars { get; set; }

    public TypeHandlerService TypeHandlers { get; set; }

    public IScheduler Scheduler { get; set; }

    internal Cache Cache { get; private set; }

    private Services(IHandlebars handlebars, QuartzminOptions options)
    {
        Handlebars = handlebars;
        Scheduler = options.Scheduler ?? throw new Exception($"Invalid options in {nameof(Services)}.{nameof(Create)}. No {nameof(options.Scheduler)} provided.");
        Options = options;
        
        ViewEngine = new ViewEngine(this);
        TypeHandlers = new TypeHandlerService(this);
        Cache = new Cache(this);
    }

    public static Services Create(QuartzminOptions options)
    {
        var handlebars = HandlebarsDotNet.Handlebars.Create(new HandlebarsConfiguration
        {
            FileSystem = ViewFileSystemFactory.Create(options),
            ThrowOnUnresolvedBindingExpression = true,
        });
        if (handlebars is null) throw new Exception($"Failed to construct 'Handlebars' in {nameof(Services)}.{nameof(Create)}");

        var services = new Services(handlebars, options);

        HandlebarsHelpers.Register(services);

        return services;
    }
}