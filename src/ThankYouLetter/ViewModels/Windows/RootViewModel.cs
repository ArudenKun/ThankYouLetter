using System;
using CommunityToolkit.Mvvm.Input;

namespace ThankYouLetter.ViewModels.Windows;

public partial class RootViewModel : ViewModel
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    [RelayCommand]
    private void ShowError()
    {
        throw new NotImplementedException("Test");
    }
}
