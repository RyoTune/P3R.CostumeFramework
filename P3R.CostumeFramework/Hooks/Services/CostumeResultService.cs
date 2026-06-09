using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

/// <summary>
/// Overrides the battle result level sequence (DT_BtlResultLS) per costume.
/// Rows are named pc_{id} where id matches the Character enum value
/// (pc_1 = Player, pc_2 = Yukari, ... pc_11 = Metis, pc_12 = AigisReal).
/// pc_11/pc_12 actually live in a separate table layered over this one, so
/// they may simply not be present here; missing rows are skipped gracefully.
/// </summary>
internal sealed unsafe class CostumeResultService
{
    private const string TableName = "DT_BtlResultLS";

    private readonly IUnreal unreal;
    private readonly CostumeManager manager;

    private DataTable<FBtlResultSequence>? table;

    // Snapshot of the vanilla rows so we can restore before re-applying.
    private readonly Dictionary<string, FBtlResultSequence> vanilla = new();

    public CostumeResultService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    {
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBtlResultSequence>(TableName, this.OnTableLoaded);
        hooks.OnCostumeChanged += _ => this.ApplyAll();
    }

    private void OnTableLoaded(DataTable<FBtlResultSequence> loaded)
    {
        this.table = loaded;

        this.vanilla.Clear();
        foreach (var row in loaded.Rows)
        {
            this.vanilla[row.Name] = *row.Self;
        }

        Log.Information($"Result LS table loaded: snapshotted {this.vanilla.Count} vanilla rows.");
        this.ApplyAll();
    }

    private void ApplyAll()
    {
        if (this.table == null)
        {
            return;
        }

        this.RestoreVanilla();

        foreach (var costume in this.manager.GetCurrentCostumes())
        {
            this.Apply(costume);
        }
    }

    private void RestoreVanilla()
    {
        foreach (var row in this.table!.Rows)
        {
            if (this.vanilla.TryGetValue(row.Name, out var original))
            {
                *row.Self = original;
                // Reset the weak ptr so the engine re-resolves the soft object reference.
                row.Self->Sequencer.baseObj.baseObj.WeakPtr = new();
            }
        }
    }

    private void Apply(Costume costume)
    {
        var lsPath = costume.Config.Allout.LsPath;
        if (string.IsNullOrEmpty(lsPath))
        {
            return;
        }

        var rowName = $"pc_{(int)costume.Character}";
        var row = this.table!.Rows.FirstOrDefault(x => x.Name == rowName);
        if (row == null)
        {
            // Expected for pc_11/pc_12 if their rows live in the other table.
            Log.Debug($"Result LS row not found: {rowName} (from {costume.Character} || {costume.Name}).");
            return;
        }

        var unrealPath = AssetUtils.GetUnrealAssetPath(lsPath);
        var ptr = row.Self;
        ptr->Sequencer.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(unrealPath);
        ptr->Sequencer.baseObj.baseObj.WeakPtr = new();

        Log.Debug($"Result LS row {rowName} overridden by {costume.Character} || {costume.Name} -> {unrealPath}.");
    }
}
