using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace ThankYouLetter.Services;

public sealed class LazyService<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T
> : Lazy<T>
    where T : class
{
    public LazyService(IServiceProvider serviceProvider)
        : base(serviceProvider.GetRequiredService<T>) { }
}
