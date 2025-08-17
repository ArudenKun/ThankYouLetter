using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace ThankYouLetter.ViewModels;

public abstract class ViewModel : ObservableRecipient, IDisposable
{
    private bool _isDisposed;

    public ISukiToastManager ToastManager { get; } =
        App.Services.GetRequiredService<Lazy<ISukiToastManager>>().Value;

    public ISukiDialogManager DialogManager { get; } =
        App.Services.GetRequiredService<Lazy<ISukiDialogManager>>().Value;

    public virtual void OnLoaded() { }

    public virtual void OnUnloaded() { }

    /// <summary>
    /// Dispatches the specified action on the UI thread synchronously.
    /// </summary>
    /// <param name="action">The action to be dispatched.</param>
    protected static void Dispatch(Action action) => Dispatcher.UIThread.Invoke(action);

    /// <summary>
    /// Dispatches the specified action on the UI thread synchronously.
    /// </summary>
    /// <param name="action">The action to be dispatched.</param>
    protected static TResult Dispatch<TResult>(Func<TResult> action) =>
        Dispatcher.UIThread.Invoke(action);

    /// <summary>
    /// Dispatches the specified action on the UI thread.
    /// </summary>
    /// <param name="action">The action to be dispatched.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected static async Task DispatchAsync(Action action) =>
        await Dispatcher.UIThread.InvokeAsync(action);

    /// <summary>
    /// Dispatches the specified action on the UI thread asynchronously.
    /// </summary>
    /// <param name="action">The action to be dispatched.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected static async Task<TResult> DispatchAsync<TResult>(Func<TResult> action) =>
        await Dispatcher.UIThread.InvokeAsync(action);

    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    ~ViewModel() => Dispose(false);

    /// <inheritdoc cref="Dispose"/>>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (!disposing)
            return;

        _isDisposed = true;
    }

    /// <inheritdoc />>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
