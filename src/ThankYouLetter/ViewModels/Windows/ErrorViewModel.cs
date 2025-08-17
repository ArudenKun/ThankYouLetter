using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThankYouLetter.Common.Helpers;

namespace ThankYouLetter.ViewModels.Windows;

public sealed partial class ErrorViewModel : ViewModel
{
    private readonly IClipboard _clipboard;

    public ErrorViewModel(IClipboard clipboard)
    {
        _clipboard = clipboard;
    }

    [ObservableProperty]
    public partial bool ShouldExit { get; set; } = false;

    [ObservableProperty]
    public partial Exception? Exception { get; set; }

    [ObservableProperty]
    public partial string ExceptionMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ExceptionDetails { get; set; }

    [RelayCommand]
    private async Task CopyToClipboardAsync(RoutedEventArgs e)
    {
        var text = $"{ExceptionMessage}\n\n{ExceptionDetails}";
        DispatcherHelper.PostOnMainThread(() => _clipboard.SetTextAsync(text));

        if (e.Source is Control control)
        {
            DispatcherHelper.PostOnMainThread(() => ToolTip.SetTip(control, "Copied to clipboard"));
            DispatcherHelper.PostOnMainThread(() => ToolTip.SetIsOpen(control, true));
            await Task.Delay(1000);
            DispatcherHelper.PostOnMainThread(() => ToolTip.SetIsOpen(control, false));
            DispatcherHelper.PostOnMainThread(() => ToolTip.SetTip(control, "Copy to clipboard"));
        }
    }

    public override void OnUnloaded()
    {
        if (ShouldExit)
        {
            App.ApplicationLifetime.TryShutdown();
        }
    }
}
