using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ThankYouLetter.ViewModels.Windows;
using ThankYouLetter.Views.Windows;

namespace ThankYouLetter;

public sealed class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    // ReSharper disable once ArrangeModifiersOrder
    public static new App Current =>
        (App?)Application.Current
        ?? throw new InvalidOperationException("Application is not yet initialized");

    // ReSharper disable once ArrangeModifiersOrder
    public static new IClassicDesktopStyleApplicationLifetime ApplicationLifetime =>
        (IClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime
        ?? throw new InvalidOperationException("Application is not yet initialized");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        Dispatcher.UIThread.UnhandledException += DispatcherUIThreadOnUnhandledException;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var desktop = ApplicationLifetime;

        DisableAvaloniaDataAnnotationValidation();

        desktop.ShutdownRequested += OnShutdownRequested;
        desktop.Exit += OnExit;

        var services = new ServiceCollection();
        services.AddSingleton(desktop);
        services.ConfigureThankYouLetter();
        ConfigureServices(services);

        services.AddSingleton<ViewLocator>();

        Services = services.BuildServiceProvider();

        var viewLocator = Services.GetRequiredService<ViewLocator>();
        DataTemplates.Add(viewLocator);

        var window = viewLocator.Build(Services.GetRequiredService<RootViewModel>()) as Window;
        desktop.MainWindow = window;

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services) { }

    private static void TaskSchedulerOnUnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs e
    )
    {
        try
        {
            ErrorView.ShowException(e.Exception);
            // if (TryIgnoreException(e.Exception, out string errorMessage))
            // {
            //     LoggerHelper.Warning(errorMessage);
            //     LoggerHelper.Info(e.Exception.ToString());
            // }
            // else
            // {
            //     LoggerHelper.Error(e.Exception);
            //     ErrorView.ShowException(e.Exception);
            //
            //     foreach (var item in e.Exception.InnerExceptions ?? Enumerable.Empty<Exception>())
            //     {
            //         LoggerHelper.Error(
            //             string.Format(
            //                 "异常类型：{0}{1}来自：{2}{3}异常内容：{4}",
            //                 item.GetType(),
            //                 Environment.NewLine,
            //                 item.Source,
            //                 Environment.NewLine,
            //                 item.Message
            //             )
            //         );
            //     }
            // }

            e.SetObserved();
        }
        catch (Exception ex)
        {
            // LoggerHelper.Error("处理未观察任务异常时发生错误: " + ex.ToString());
            e.SetObserved();
        }
    }

    private static void CurrentDomainOnUnhandledException(
        object sender,
        UnhandledExceptionEventArgs e
    )
    {
        try
        {
            // if (
            //     e.ExceptionObject is Exception ex
            //     && TryIgnoreException(ex, out string errorMessage)
            // )
            // {
            //     LoggerHelper.Warning(errorMessage);
            //     LoggerHelper.Error(ex.ToString());
            //     return;
            // }

            var sbEx = new StringBuilder();
            if (e.IsTerminating)
                sbEx.Append("非UI线程发生致命错误");
            else
                sbEx.Append("非UI线程异常：");

            if (e.ExceptionObject is Exception ex2)
            {
                ErrorView.ShowException(ex2);
                sbEx.Append(ex2.Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            // LoggerHelper.Error(sbEx.ToString());
        }
        catch (Exception ex)
        {
            // LoggerHelper.Error("处理非UI线程异常时发生错误: " + ex.ToString());
        }
    }

    private static void DispatcherUIThreadOnUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e
    )
    {
        try
        {
            // if (TryIgnoreException(e.Exception, out string errorMessage))
            // {
            //     LoggerHelper.Warning(errorMessage);
            //     LoggerHelper.Error(e.Exception.ToString());
            //     e.Handled = true;
            //     return;
            // }

            e.Handled = true;
            // LoggerHelper.Error(e.Exception);
            ErrorView.ShowException(e.Exception);
        }
        catch (Exception ex)
        {
            // LoggerHelper.Error("处理UI线程异常时发生错误: " + ex.ToString());
            ErrorView.ShowException(ex, true);
        }
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
