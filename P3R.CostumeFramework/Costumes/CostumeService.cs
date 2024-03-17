using P3R.CostumeFramework.Hooks;
using Unreal.ObjectsEmitter.Interfaces;

namespace P3R.CostumeFramework.Costumes;

internal unsafe class CostumeService
{
    private readonly CostumeHooks costumeHooks;
    private readonly ItemCountHook itemCountHook;
    private readonly CostumeNameHook costumeNameHook;

    public CostumeService(IUObjects uobjects, IUnreal unreal, CostumeRegistry registry)
    {
        this.costumeHooks = new(uobjects, unreal, registry);
        this.itemCountHook = new();
        this.costumeNameHook = new(uobjects, registry);
    }

    public void SetRandomizeCostumes(bool randomize)
        => this.costumeHooks.SetRandomizeCostumes(randomize);
}