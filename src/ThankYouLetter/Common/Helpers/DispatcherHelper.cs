using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace ThankYouLetter.Common.Helpers;

public static class DispatcherHelper
{
    public static void RunOnMainThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Invoke(action);
    }

    public static async Task RunOnMainThread(Func<Task> action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            await action();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }

    public static T RunOnMainThread<T>(Func<T> func) =>
        Dispatcher.UIThread.CheckAccess() ? func() : Dispatcher.UIThread.Invoke(func);

    public static async Task<T> RunOnMainThread<T>(Func<Task<T>> func)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return await func();
        }

        return await Dispatcher.UIThread.InvokeAsync(func);
    }

    public static void PostOnMainThread(Action func)
    {
        Dispatcher.UIThread.Post(func);
    }
}
