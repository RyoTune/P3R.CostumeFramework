using P3R.CostumeFramework.Hooks;
using Unreal.ObjectsEmitter.Interfaces;

namespace P3R.CostumeFramework.Costumes;

internal unsafe class CostumeService
{
    private readonly CostumeHooks costumeHooks;
    private readonly ItemCountHook itemCountHook;
    private readonly CostumeNameHook costumeNameHook;
    private readonly ItemEquipHooks itemEquipHooks;

    public CostumeService(
        IUObjects uobjects,
        IUnreal unreal,
        CostumeRegistry registry,
        CostumeOverridesRegistry overrides,
        CostumeDescService costumeDesc,
        CostumeMusicService costumeMusic)
    {
        this.itemEquipHooks = new();
        this.costumeHooks = new(uobjects, unreal, registry, overrides, costumeDesc, costumeMusic, this.itemEquipHooks);
        this.itemCountHook = new(registry);
        this.costumeNameHook = new(uobjects, registry);
    }

    public void SetRandomizeCostumes(bool randomize) => this.costumeHooks.SetRandomizeCostumes(randomize);

    public void SetUseFemc(bool useFemc) => this.costumeHooks.SetUseFemc(useFemc);
}