using P3R.CostumeFramework.Costumes;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks;

internal unsafe class CostumeNameHook
{
    private readonly IUnreal unreal;
    private readonly CostumeRegistry registry;
    private UItemNameListTable* nameTable;

    public CostumeNameHook(IUObjects uobjects, IUnreal unreal, CostumeRegistry registry)
    {
        this.unreal = unreal;
        this.registry = registry;

        uobjects.FindObject("DatItemCostumeNameDataAsset", obj =>
        {
            this.nameTable = (UItemNameListTable*)obj.Self;
            Log.Debug($"DatItemCostumeNameDataAsset loaded with {this.nameTable->Data.Num} entries.");
            // Write names now after its actually loaded since we have to include file again. Maybe I can switch it to uetoolkit as opposed to the actual file but im lazy. 
            this.RefreshNames();
        });
    }
    public void RefreshNames()
    {
        if (this.nameTable == null)
        {
            Log.Debug("RefreshNames: name table not yet loaded; skipping.");
            return;
        }

        var written = 0;
        for (int i = 0; i < this.nameTable->Data.Num; i++)
        {
            if (i == 0) continue;

            var costume = this.registry.Costumes.FirstOrDefault(x => x.CostumeItemId == i);
            if (costume == null) continue;

            if (costume.Config.DisplayName != null)
            {
                this.nameTable->Data.AllocatorInstance[i] = this.unreal.FString(costume.Config.DisplayName);
                Log.Debug($"Set name for Costume Item ID: {i} || Name: {costume.Config.DisplayName}");
                written++;
            }
            else if (costume.Name != null)
            {
                this.nameTable->Data.AllocatorInstance[i] = this.unreal.FString(costume.Name);
                Log.Debug($"Set name for Costume Item ID: {i} || Name: {costume.Name}");
                written++;
            }
        }

        Log.Information($"RefreshNames: wrote {written} costume names into the name table.");
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x40)]
public unsafe struct UItemNameListTable
{
    //[FieldOffset(0x0000)] public UAppDataAsset baseObj;
    [FieldOffset(0x0030)] public TArray<FString> Data;
}