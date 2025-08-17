using System;
using System.IO;
using System.Text.Json.Serialization;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThankYouLetter.Configuration;

[INotifyPropertyChanged]
public sealed partial class AppConfig : SettingsBase
{
    private static readonly string ConfigPath = Path.Combine(
        AppContext.BaseDirectory,
        "config.json"
    );

    public AppConfig()
        : base(ConfigPath, AppConfigurationJsonContext.Default) { }

    [JsonSerializable(typeof(AppConfig))]
    private sealed partial class AppConfigurationJsonContext : JsonSerializerContext;
}
