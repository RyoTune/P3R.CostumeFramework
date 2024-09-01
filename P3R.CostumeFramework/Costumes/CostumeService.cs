using P3R.CostumeFramework.Configuration;
using P3R.CostumeFramework.Hooks;
using P3R.CostumeFramework.Hooks.Services;
using Unreal.ObjectsEmitter.Interfaces;

namespace P3R.CostumeFramework.Costumes;

internal unsafe class CostumeService
{
    private readonly CostumeHooks costumeHooks;
    private readonly ItemCountHook itemCountHook;
    private readonly CostumeNameHook costumeNameHook;
    private readonly ItemEquip itemEquip;
    private readonly CostumeAlloutService alloutService;

    public CostumeService(
        IUObjects uobjects,
        IUnreal unreal,
        IDataTables dt,
        CostumeRegistry registry,
        CostumeOverridesRegistry overrides,
        CostumeDescService costumeDesc,
        CostumeMusicService costumeMusic)
    {
        this.itemEquip = new(registry);
        this.alloutService = new(dt, unreal, this.itemEquip);
        this.costumeHooks = new(uobjects, unreal, registry, overrides, costumeDesc, costumeMusic, this.itemEquip);
        this.itemCountHook = new(registry);
        this.costumeNameHook = new(uobjects, registry);
    }

    public void SetConfig(Config config)
    {
        this.costumeHooks.SetRandomizeCostumes(config.RandomizeCostumes);
        this.costumeHooks.SetOverworldCostumes(config.OverworldCostumes);
    }

    public void SetUseFemc(bool useFemc) => this.costumeHooks.SetUseFemc(useFemc);
}
