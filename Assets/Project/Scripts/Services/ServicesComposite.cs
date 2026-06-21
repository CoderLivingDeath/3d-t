using _3dt.Infrastructure;
using Reflex.Core;
using UnityEngine;

public static class ServicesComposite
{
    public static void RegisterGameplayServices(ContainerBuilder c) { }

    public static void RegisterProjectServices(ContainerBuilder c)
    {
        c.RegisterType(
            typeof(ResourceAssetsRepository),
            new[] { typeof(IReadOnlyRepository<UnityEngine.Object>) },
            Reflex.Enums.Lifetime.Singleton,
            Reflex.Enums.Resolution.Lazy
        );
        c.RegisterType(
            typeof(ResourceConfigRepository),
            new[] { typeof(IReadOnlyRepository<Config>) },
            Reflex.Enums.Lifetime.Singleton,
            Reflex.Enums.Resolution.Lazy
        );
        c.RegisterType(
            typeof(ResourceConfigService),
            new[] { typeof(IConfigService) },
            Reflex.Enums.Lifetime.Singleton,
            Reflex.Enums.Resolution.Lazy
        );
        c.RegisterType(
            typeof(ResourcesAssetsServiceService),
            new[] { typeof(IAssetsService) },
            Reflex.Enums.Lifetime.Singleton,
            Reflex.Enums.Resolution.Lazy
        );
        c.RegisterType(
            typeof(InputService),
            new[] { typeof(IInputService) },
            Reflex.Enums.Lifetime.Singleton,
            Reflex.Enums.Resolution.Lazy
        );
    }
}
