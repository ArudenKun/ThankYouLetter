using System;

namespace ThankYouLetter.Dependency;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : Attribute;
