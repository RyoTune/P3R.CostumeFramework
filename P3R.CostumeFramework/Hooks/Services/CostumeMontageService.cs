using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using P3R.CostumeFramework.Hooks.Services;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeMontageService
{
    private readonly IUnreal unreal;
    private readonly CostumeManager manager;
    private DataTable<FBustupParamTable>? montageTable;
    private readonly List<Costume> pendingCostumes = [];

    public CostumeMontageService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    {
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBustupParamTable>("Anim_Montage", this.MontageTableLoaded);

        hooks.OnCostumeChanged += this.RefreshMontageForCostume;
    }

    private void MontageTableLoaded(DataTable<FBustupParamTable> table)
    {
        this.montageTable = table;

        // Ensure currently equipped costumes are re-applied when the table reloads.
        this.pendingCostumes.AddRange(this.manager.GetCurrentCostumes());

        this.ApplyPendingCostumes();
    }

    private void RefreshMontageForCostume(Costume costume)
    {
        if (this.montageTable == null)
        {
            Log.Debug("Montage table not yet loaded; queueing refresh.");
            this.pendingCostumes.Add(costume);
            return;
        }

        this.ApplyMontage(costume);
    }

    private void ApplyPendingCostumes()
    {
        if (this.montageTable == null || this.pendingCostumes.Count == 0)
        {
            return;
        }

        foreach (var costume in this.pendingCostumes.ToArray())
        {
            this.ApplyMontage(costume);
        }

        this.pendingCostumes.Clear();
    }

    private void ApplyMontage(Costume costume)
    {
        var table = this.montageTable;
        if (table == null)
        {
            Log.Debug("Montage table not yet loaded; skipping application.");
            return;
        }

        var rowName = $"PC{(int)costume.Character}";
        var row = table.Rows.FirstOrDefault(x => x.Name == rowName);
        if (row == null)
        {
            Log.Debug($"Montage row not found for {rowName}.");
            return;
        }

        var rowPtr = row.Self;
        
        string montagePath;
        if (costume.Config.MontagePath != null)
        {
             montagePath = AssetUtils.GetUnrealAssetPath(costume.Config.MontagePath);
        }
        else
        {
             // Default path
             var charId = AssetUtils.GetCharIdStringShort(costume.Character);
             montagePath = $"/Game/Xrd777/Battle/Players/Pc{charId}/AM_BtlPc{charId}.AM_BtlPc{charId}";
        }

        Log.Debug($"Set montage for {rowName} to {montagePath}.");
        rowPtr->Pose = this.unreal.FString(montagePath);
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