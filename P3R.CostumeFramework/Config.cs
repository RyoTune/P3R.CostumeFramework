using P3R.CostumeFramework.Template.Configuration;
using System.ComponentModel;

namespace P3R.CostumeFramework.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [DisplayName("Extra Costumes")]
    [Description("Adds existing game files as new costumes.\nMay or may not cause visual bugs or crashes.")]
    [DefaultValue(false)]
    public bool ExtraCostumes { get; set; } = false;

    [DisplayName("Randomize Costumes")]
    [DefaultValue(false)]
    public bool RandomizeCostumes { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}