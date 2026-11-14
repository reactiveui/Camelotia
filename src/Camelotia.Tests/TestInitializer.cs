using System.Runtime.CompilerServices;
using Akavache.Reactive;
using Akavache.Reactive.SystemTextJson;
using ReactiveUI.Reactive.Builder;

namespace Camelotia.Tests;

internal static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        RxAppBuilder.CreateReactiveUIBuilder().WithCoreServices().BuildApp();
        Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(static builder => builder.WithInMemoryDefaults(), "CamelotiaTests");
    }
}
