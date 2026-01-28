using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeBustupPhysicsService
{
    private readonly IUnreal unreal;
    private readonly CostumeManager manager;
    private DataTable<FBustupParamTable>? bustupTable;
    private readonly List<Costume> pendingCostumes = [];

    public CostumeBustupPhysicsService(IDataTables dt, IUnreal unreal, CostumeManager manager)
    {
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBustupParamTable>("truthnuke", this.BustupTableLoaded);
        this.manager.OnCostumeChanged += this.RefreshBustupPhysicsForCostume;
    }

    private void BustupTableLoaded(DataTable<FBustupParamTable> table)
    {

        this.bustupTable = table;

        // Ensure currently equipped costumes are re-applied when the table reloads.
        this.pendingCostumes.AddRange(this.manager.GetCurrentCostumes());

        this.ApplyPendingCostumes();
    }

    private void RefreshBustupPhysicsForCostume(Costume costume)
    {
        if (this.bustupTable == null)
        {
            Log.Debug("Bustup table not yet loaded; queueing physics refresh.");
            this.pendingCostumes.Add(costume);
            return;
        }

        this.ApplyBustupPhysics(costume);
    }

    private void ApplyPendingCostumes()
    {
        if (this.bustupTable == null || this.pendingCostumes.Count == 0)
        {
            return;
        }

        foreach (var costume in this.pendingCostumes.ToArray())
        {
            this.ApplyBustupPhysics(costume);
        }

        this.pendingCostumes.Clear();
    }

    private void ApplyBustupPhysics(Costume costume)
    {
        var table = this.bustupTable;
        if (table == null)
        {
            Log.Debug("Bustup table not yet loaded; skipping physics application.");
            return;
        }

        var bustupRowName = $"PC{(int)costume.Character}";
        var bustupRow = table.Rows.FirstOrDefault(x => x.Name == bustupRowName);
        if (bustupRow == null)
        {
            Log.Debug($"Bustup row not found for {bustupRowName}.");
            return;
        }

        var bustupRowPtr = bustupRow.Self;

        var rawBattlePhysics = costume.Config.BattlePhysics;
        var physicsEnabled = rawBattlePhysics ?? false;
        Log.Information($"Battle physics config for {bustupRowName}: raw={rawBattlePhysics?.ToString() ?? "null"}, applied={physicsEnabled}.");
        bustupRowPtr->Pose = this.unreal.FString(physicsEnabled ? "True" : "False");
        Log.Debug($"Set battle physics pose for {bustupRowName} to {physicsEnabled}.");
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
