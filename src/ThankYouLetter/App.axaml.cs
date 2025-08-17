using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using ThankYouLetter.Common.Helpers;
using ThankYouLetter.Services;
using ThankYouLetter.ViewModels.Windows;
using ThankYouLetter.Views.Windows;

namespace ThankYouLetter;

public sealed class App : Application
{
    /// <summary>
    /// Mutex to prevent multiple instances of the application from running.
    /// </summary>
    private static readonly Mutex AppMutex = new(
        true,
        $"Mutex_{Environment.UserDomainName}_{Environment.UserName}_{typeof(App).FullName}_{{10BD95D3-492E-4A2E-A734-2002B6EE798B}}"
    );

    public static IServiceProvider Services { get; private set; } = null!;

    // ReSharper disable once ArrangeModifiersOrder
    public static new App Current =>
        (App?)Application.Current
        ?? throw new InvalidOperationException("Application is not yet initialized");

    // ReSharper disable once ArrangeModifiersOrder
    public static new IClassicDesktopStyleApplicationLifetime ApplicationLifetime =>
        (IClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime
        ?? throw new InvalidOperationException("Application is not yet initialized");

    public static Window RootView =>
        ApplicationLifetime.MainWindow
        ?? throw new InvalidOperationException("Application is not yet initialized");

    public override void Initialize()
    {
        LogHelper.Initialize();
        AvaloniaXamlLoader.Load(this);

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            HandleUnhandledException((Exception)e.ExceptionObject, "App");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            e.SetObserved();
            HandleUnhandledException(e.Exception, "Task");
        };
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            e.Handled = true;
            HandleUnhandledException(e.Exception, "UI");
        };
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var desktop = ApplicationLifetime;

        DisableAvaloniaDataAnnotationValidation();

        desktop.ShutdownRequested += OnShutdownRequested;
        desktop.Exit += OnExit;

        var services = new ServiceCollection();
        services.AddSingleton(desktop);
        services.AddViews();
        services.AddViewModels();
        services.AddSingleton<ViewLocator>();

        services.AddSingleton<TopLevel>(sp => sp.GetRequiredService<RootView>());
        services.AddSingleton<IClipboard>(sp => sp.GetRequiredService<TopLevel>().Clipboard!);
        services.AddSingleton<IStorageProvider>(sp =>
            sp.GetRequiredService<TopLevel>().StorageProvider
        );

        services.AddServices();
        services.AddTransient(typeof(Lazy<>), typeof(LazyService<>));

        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();

        services.AddLogging(b => b.ClearProviders().AddSerilog(dispose: true));

        Services = services.BuildServiceProvider();

        var viewLocator = Services.GetRequiredService<ViewLocator>();
        DataTemplates.Add(viewLocator);

        var window = viewLocator.Build(Services.GetRequiredService<RootViewModel>()) as Window;
        desktop.MainWindow = window;

        base.OnFrameworkInitializationCompleted();
    }

    public static void HandleUnhandledException(Exception ex, string category)
    {
        var loggerFactory = Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(category);
        var exceptionService = Services.GetRequiredService<ExceptionService>();
        exceptionService.ShowWindow(ex);
        logger.LogError(ex, "Unhandled Exception");
    }

    private static void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e) { }

    private static void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "<Pending>"
    )]
    private static void DisableAvaloniaDataAnnotationValidation()
    {
        foreach (
            var plugin in BindingPlugins
                .DataValidators.OfType<DataAnnotationsValidationPlugin>()
                .ToArray()
        )
            BindingPlugins.DataValidators.Remove(plugin);
    }
}
