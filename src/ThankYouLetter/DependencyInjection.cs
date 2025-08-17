using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceScan.SourceGenerator;
using ThankYouLetter.Dependency;
using ThankYouLetter.Services;
using ThankYouLetter.ViewModels;
using ThankYouLetter.Views;

namespace ThankYouLetter;

public static partial class DependencyInjection
{
    public const string LogOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";

    public static IServiceCollection ConfigureThankYouLetter(this IServiceCollection services)
    {
        services.AddServices();
        services.AddViews();
        services.AddViewModels();
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyService<>));
        return services;
    }

    [GenerateServiceRegistrations(
        AssignableTo = typeof(ISingletonDependency),
        AsSelf = true,
        AsImplementedInterfaces = true,
        Lifetime = ServiceLifetime.Singleton
    )]
    [GenerateServiceRegistrations(
        AssignableTo = typeof(ITransientDependency),
        AsSelf = true,
        AsImplementedInterfaces = true,
        Lifetime = ServiceLifetime.Transient
    )]
    public static partial IServiceCollection AddServices(this IServiceCollection services);

    [GenerateServiceRegistrations(
        AssignableTo = typeof(IView<>),
        AsSelf = true,
        AttributeFilter = typeof(SingletonAttribute),
        Lifetime = ServiceLifetime.Singleton
    )]
    [GenerateServiceRegistrations(
        AssignableTo = typeof(IView<>),
        AsSelf = true,
        ExcludeByAttribute = typeof(SingletonAttribute),
        Lifetime = ServiceLifetime.Transient
    )]
    public static partial IServiceCollection AddViews(this IServiceCollection services);

    [GenerateServiceRegistrations(
        AssignableTo = typeof(ViewModel),
        CustomHandler = nameof(AddViewModelsHandler)
    )]
    public static partial IServiceCollection AddViewModels(this IServiceCollection services);

    private static void AddViewModelsHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel
    >(IServiceCollection service)
        where TViewModel : ViewModel
    {
        var viewModelType = typeof(TViewModel);
        var isSingleton = viewModelType.GetCustomAttribute<SingletonAttribute>() is not null;
        var lifetime = isSingleton ? ServiceLifetime.Singleton : ServiceLifetime.Transient;
        var sd = ServiceDescriptor.Describe(typeof(TViewModel), typeof(TViewModel), lifetime);
        var baseClassSds = EnumerateBaseTypes<ViewModel>(viewModelType)
            .Select(baseType =>
                ServiceDescriptor.Describe(
                    baseType,
                    sp => sp.GetRequiredService<TViewModel>(),
                    lifetime
                )
            );
        service.Add(sd);
        service.Add(baseClassSds);
    }

    private static IEnumerable<Type> EnumerateBaseTypes<TRoot>(Type t)
    {
        ArgumentNullException.ThrowIfNull(t);

        var baseType = t.BaseType;
        while (baseType is not null && baseType != typeof(object))
        {
            yield return baseType;
            if (baseType == typeof(TRoot))
            {
                yield break;
            }

            baseType = baseType.BaseType;
        }
    }
}
