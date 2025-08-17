using System;
using System.Text;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using ThankYouLetter.Common.Helpers;
using ThankYouLetter.Dependency;
using ThankYouLetter.ViewModels.Windows;
using ThankYouLetter.Views.Windows;

namespace ThankYouLetter.Services;

public sealed class ExceptionService : ISingletonDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ViewLocator _viewLocator;

    public ExceptionService(IServiceProvider serviceProvider, ViewLocator viewLocator)
    {
        _serviceProvider = serviceProvider;
        _viewLocator = viewLocator;
    }

    public void ShowWindow(Exception? exception, bool shouldExit = false)
    {
        var errorStr = new StringBuilder();
        while (exception != null)
        {
            errorStr.Append(exception.Message);
            if (exception.InnerException != null)
            {
                errorStr.AppendLine();
                exception = exception.InnerException;
            }
            else
                break;
        }

        var viewModel = _serviceProvider.GetRequiredService<ErrorViewModel>();
        viewModel.Exception = exception;
        viewModel.ExceptionMessage = errorStr.ToString();
        viewModel.ExceptionDetails = exception?.ToString();
        viewModel.ShouldExit = shouldExit;

        var view = _viewLocator.Build<ErrorView>(viewModel);
        DispatcherHelper.RunOnMainThread(() => view.ShowDialog(App.RootView)).SafeFireAndForget();
    }
}
