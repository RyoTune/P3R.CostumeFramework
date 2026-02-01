using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeVisualTableService
{
    private readonly IUnreal unreal;
    private readonly CostumeManager manager;
    private DataTable<FBustupParamTable>? visualTable;
    private readonly List<Costume> pendingCostumes = [];

    public CostumeVisualTableService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    {
        this.unreal = unreal;
        this.manager = manager;

        // Hook into the "VisualTable" data table loading
        dt.FindDataTable<FBustupParamTable>("VisualTable", this.VisualTableLoaded);

        hooks.OnCostumeChanged += this.RefreshVisualTableForCostume;
    }

    private void VisualTableLoaded(DataTable<FBustupParamTable> table)
    {
        this.visualTable = table;

        this.pendingCostumes.AddRange(this.manager.GetCurrentCostumes());

        this.ApplyPendingCostumes();
    }

    private void RefreshVisualTableForCostume(Costume costume)
    {
        if (this.visualTable == null)
        {
            Log.Debug("Visual Table not yet loaded; queueing refresh.");
            this.pendingCostumes.Add(costume);
            return;
        }

        this.ApplyVisualTable(costume);
    }

    private void ApplyPendingCostumes()
    {
        if (this.visualTable == null || this.pendingCostumes.Count == 0)
        {
            return;
        }

        foreach (var costume in this.pendingCostumes.ToArray())
        {
            this.ApplyVisualTable(costume);
        }

        this.pendingCostumes.Clear();
    }

    private void ApplyVisualTable(Costume costume)
    {
        var table = this.visualTable;
        if (table == null)
        {
            Log.Debug("Visual Table not yet loaded; skipping application.");
            return;
        }

        var rowName = $"PC{(int)costume.Character}";
        var row = table.Rows.FirstOrDefault(x => x.Name == rowName);
        if (row == null)
        {
            Log.Debug($"Visual Table row not found for {rowName}.");
            return;
        }

        var rowPtr = row.Self;

        string visualTablePath;
        if (costume.Config.Animation.VisualTable != null)
        {
            visualTablePath = AssetUtils.GetUnrealAssetPath(costume.Config.Animation.VisualTable);
        }
        else
        {
            var charId = AssetUtils.GetCharIdStringShort(costume.Character);
            visualTablePath = $"/Game/Xrd777/Battle/Players/Pc{charId}/DT_BtlPc{charId}CharacterVisual.DT_BtlPc{charId}CharacterVisual";
        }

        Log.Debug($"Set visual table for {rowName} to {visualTablePath}.");
        rowPtr->Pose = this.unreal.FString(visualTablePath);
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