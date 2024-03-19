using P3R.CostumeFramework.Configuration;
using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Diagnostics;
using System.Drawing;
using Unreal.AtlusScript.Interfaces;
using Unreal.ObjectsEmitter.Interfaces;

namespace P3R.CostumeFramework;

public class Mod : ModBase
{
    public const string NAME = "P3R.CostumeFramework";

    private readonly IModLoader modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger log;
    private readonly IMod owner;

    private Config config;
    private readonly IModConfig modConfig;

    private readonly CostumeService costumes;
    private readonly CostumeRegistry costumeRegistry;
    private readonly CostumeDescService costumeDesc;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks!;
        this.log = context.Logger;
        this.owner = context.Owner;
        this.config = context.Configuration;
        this.modConfig = context.ModConfig;

#if DEBUG
        Debugger.Launch();
#endif

        Log.Initialize(NAME, this.log, Color.White);
        Log.LogLevel = this.config.LogLevel;

        this.modLoader.GetController<IStartupScanner>().TryGetTarget(out var scanner);
        this.modLoader.GetController<IUObjects>().TryGetTarget(out var uobjects);
        this.modLoader.GetController<IUnreal>().TryGetTarget(out var unreal);
        this.modLoader.GetController<IAtlusAssets>().TryGetTarget(out var atlusAssets);

        this.costumeRegistry = new(this.config.CostumeFilter);
        this.costumeDesc = new(atlusAssets!);
        this.costumes = new(uobjects!, unreal!, this.costumeRegistry, this.costumeDesc);

        ScanHooks.Initialize(scanner!, this.hooks);
        this.ApplyConfig();

        this.modLoader.ModLoaded += this.OnModLoaded;
    }

    private void OnModLoaded(IModV1 mod, IModConfigV1 config)
    {
        if (!config.ModDependencies.Contains(this.modConfig.ModId))
        {
            return;
        }

        var modDir = this.modLoader.GetDirectoryForModId(config.ModId);
        this.costumeRegistry.RegisterMod(config.ModId, modDir);
    }

    private void ApplyConfig()
    {
        Log.LogLevel = this.config.LogLevel;
        this.costumes.SetRandomizeCostumes(this.config.RandomizeCostumes);
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        config = configuration;
        log.WriteLine($"[{modConfig.ModId}] Config Updated: Applying");
        this.ApplyConfig();
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}