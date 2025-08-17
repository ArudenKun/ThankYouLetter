using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using ServiceScan.SourceGenerator;
using ThankYouLetter.ViewModels;
using ThankYouLetter.Views;

namespace ThankYouLetter;

public sealed partial class ViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Func<Control>> _viewFactory = new();

    public ViewLocator(IServiceProvider serviceProvider)
    {
        AddViews(this, serviceProvider);
    }

    public TView Build<TView>(ViewModel viewModel)
        where TView : Control => (TView)Build(viewModel);

    public Control Build(object? data)
    {
        if (data is not ViewModel viewModel)
        {
            return CreateText("ViewModel is null or does not inherit from ViewModel");
        }

        var viewModelType = viewModel.GetType();
        if (!_viewFactory.TryGetValue(viewModelType, out var factory))
        {
            return CreateText($"Could not find view for {viewModelType.FullName}");
        }

        var view = factory();
        view.DataContext = viewModel;
        BindEvents(view, viewModel);
        return view;
    }

    public bool Match(object? data) => data is ViewModel;

    private static void BindEvents(Control control, ViewModel viewModel)
    {
        control.Loaded += Loaded;
        control.Unloaded += Unloaded;
        return;

        void Loaded(object? sender, RoutedEventArgs e) => viewModel.OnLoaded();

        void Unloaded(object? sender, RoutedEventArgs e)
        {
            viewModel.OnUnloaded();
            control.Loaded -= Loaded;
            control.Unloaded -= Unloaded;
        }
    }

    private static TextBlock CreateText(string text) => new() { Text = text };

    [GenerateServiceRegistrations(
        AssignableTo = typeof(IView<>),
        CustomHandler = nameof(AddViewsHandler)
    )]
    private static partial void AddViews(ViewLocator viewLocator, IServiceProvider serviceProvider);

    private static void AddViewsHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
        TViewModel
    >(ViewLocator viewLocator, IServiceProvider serviceProvider)
        where TView : Control, IView<TViewModel>
        where TViewModel : ViewModel =>
        viewLocator._viewFactory.TryAdd(
            typeof(TViewModel),
            serviceProvider.GetRequiredService<TView>
        );
}
