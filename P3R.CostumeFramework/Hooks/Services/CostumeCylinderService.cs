using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeCylinderService
{
    private readonly IUnreal unreal;
    private readonly CostumeManager manager;
    private DataTable<FBustupParamTable>? cylinderTable;
    private readonly List<Costume> pendingCostumes = [];

    public CostumeCylinderService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    {
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBustupParamTable>("CylinderTable", this.CylinderTableLoaded);

        hooks.OnCostumeChanged += this.RefreshCylinderForCostume;
    }

    private void CylinderTableLoaded(DataTable<FBustupParamTable> table)
    {
        this.cylinderTable = table;

        this.pendingCostumes.AddRange(this.manager.GetCurrentCostumes());

        this.ApplyPendingCostumes();
    }

    private void RefreshCylinderForCostume(Costume costume)
    {
        if (this.cylinderTable == null)
        {
            Log.Debug("Cylinder table not yet loaded; queueing refresh.");
            this.pendingCostumes.Add(costume);
            return;
        }

        this.ApplyCylinder(costume);
    }

    private void ApplyPendingCostumes()
    {
        if (this.cylinderTable == null || this.pendingCostumes.Count == 0)
        {
            return;
        }

        foreach (var costume in this.pendingCostumes.ToArray())
        {
            this.ApplyCylinder(costume);
        }

        this.pendingCostumes.Clear();
    }

    private void ApplyCylinder(Costume costume)
    {
        var table = this.cylinderTable;
        if (table == null)
        {
            Log.Debug("Cylinder table not yet loaded; skipping application.");
            return;
        }

        var rowName = $"PC{(int)costume.Character}";
        var row = table.Rows.FirstOrDefault(x => x.Name == rowName);
        if (row == null)
        {
            Log.Debug($"Cylinder row not found for {rowName}.");
            return;
        }

        var rowPtr = row.Self;

        string cylinderPath;
        if (costume.Config.Animation.CylinderTable != null)
        {
            cylinderPath = AssetUtils.GetUnrealAssetPath(costume.Config.Animation.CylinderTable);
        }
        else
        {
            var charId = AssetUtils.GetCharIdStringShort(costume.Character);
            cylinderPath = $"/Game/Xrd777/Battle/Players/Pc{charId}/DT_BtlPc{charId}Cylinder.DT_BtlPc{charId}Cylinder";
        }

        Log.Debug($"Set cylinder table for {rowName} to {cylinderPath}.");
        rowPtr->Pose = this.unreal.FString(cylinderPath);
    }

    [StructLayout(LayoutKind.Explicit, Pack = 8, Size = 0x28)]
    private struct FBustupParamTable
    {
        [FieldOffset(0x0)] public FTableRowBase Super;
        [FieldOffset(0x8)] public ushort CharaID;
        [FieldOffset(0xA)] public ushort FaceID;
        [FieldOffset(0xC)] public ushort ClothID;
        [FieldOffset(0x10)] public FString Pose;
        [FieldOffset(0x20)] public bool EyeAnim;
        [FieldOffset(0x21)] public bool MouthAnim;
        [FieldOffset(0x22)] public byte InBetween;
    }
}