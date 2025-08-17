using System;
using Avalonia.Controls;
using ThankYouLetter.ViewModels;

namespace ThankYouLetter.Views;

public abstract class UserControl<TViewModel> : UserControl, IView<TViewModel>
    where TViewModel : ViewModel
{
    public new TViewModel DataContext
    {
        get =>
            base.DataContext as TViewModel
            ?? throw new InvalidCastException(
                $"DataContext is null or not of the expected type '{typeof(TViewModel).FullName}'."
            );
        set => base.DataContext = value;
    }

    public TViewModel ViewModel => DataContext;
}
