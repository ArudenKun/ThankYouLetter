using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using ThankYouLetter.Common.Attributes;
using ThankYouLetter.ViewModels.Windows;
using ThankYouLetter.Views.Windows;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS0169 // Field is never used

namespace ThankYouLetter.Common;

[LazyStatic]
public static partial class Instances
{
    private static readonly ConcurrentDictionary<Type, Lazy<object>> ServiceCache = new();

    /// <summary>
    /// 解析服务（自动缓存 + 循环依赖检测）
    /// </summary>
    private static T Resolve<T>()
        where T : notnull
    {
        var serviceType = typeof(T);
        var lazy = ServiceCache.GetOrAdd(
            serviceType,
            _ => new Lazy<object>(
                () =>
                {
                    try
                    {
                        return App.Services.GetRequiredService<T>();
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to resolve service {typeof(T).Name}. Possible causes: 1. Service not registered; 2. Circular dependency detected; 3. Thread contention during initialization.",
                            ex
                        );
                    }
                },
                LazyThreadSafetyMode.ExecutionAndPublication
            )
        );
        return (T)lazy.Value;
    }

    private static RootView _rootView;
    private static RootViewModel _rootViewModel;
}
