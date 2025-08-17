using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Hosting.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Avalonia.Hosting;

/// <summary>
/// Provides extension methods for configuring Avalonia applications with the generic host.
/// </summary>
public static class AvaloniaHostingExtensions
{
    /// <summary>
    /// Adds Avalonia main window to the host's service collection,
    /// and a <see cref="AppBuilder"/> to create the Avalonia application.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configure">The application builder, also used by the previewer.</param>
    /// <returns>The updated host application builder.</returns>
    public static IHostApplicationBuilder AddAvaloniaHosting<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp
    >(this IHostApplicationBuilder builder, Action<IServiceProvider, AppBuilder> configure)
        where TApp : Application
    {
        builder
            .Services.AddSingleton<TApp>()
            .AddSingleton<Application>(sp => sp.GetRequiredService<TApp>())
            .AddSingleton(sp =>
            {
                var appBuilder = AppBuilder.Configure(sp.GetRequiredService<TApp>);
                configure(sp, appBuilder);
                return appBuilder;
            })
            .AddSingleton<IClassicDesktopStyleApplicationLifetime>(_ =>
                (IClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime
                ?? throw new InvalidOperationException("Avalonia application lifetime is not set.")
            )
            .AddSingleton<AvaloniaThread>()
            .AddHostedService<AvaloniaHostedService>();
        return builder;
    }

    /// <summary>
    /// Adds Avalonia main window to the host's service collection,
    /// and a <see cref="AppBuilder"/> to create the Avalonia application.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configure">The application builder, also used by the previewer.</param>
    /// <returns>The updated host application builder.</returns>
    public static IHostApplicationBuilder AddAvaloniaHosting<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp
    >(this IHostApplicationBuilder builder, Action<AppBuilder> configure)
        where TApp : Application =>
        builder.AddAvaloniaHosting<TApp>((_, appBuilder) => configure(appBuilder));
}
