using System;
using System.IO;
using System.Text.Json.Serialization;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThankYouLetter.Configuration;

[INotifyPropertyChanged]
public sealed partial class GlobalConfig : SettingsBase
{
    private static readonly string ConfigPath = Path.Combine(
        AppContext.BaseDirectory,
        "global.json"
    );

    public GlobalConfig()
        : base(ConfigPath, GlobalConfigurationJsonContext.Default) { }

    [JsonSerializable(typeof(GlobalConfig))]
    private partial class GlobalConfigurationJsonContext : JsonSerializerContext;
}
