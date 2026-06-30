using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal sealed unsafe class CostumeAlloutSequencerService
{
    private const string TableName = "DT_BtlAlloutSequencer";

    private readonly IUnreal unreal;
    private readonly CostumeManager manager;

    private readonly List<DataTable<FBtlAlloutSequencer>> tables = new();
    private readonly Dictionary<DataTable<FBtlAlloutSequencer>, Dictionary<string, FBtlAlloutSequencer>> vanilla = new();

    private int loadCount;

    public CostumeAlloutSequencerService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    {
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBtlAlloutSequencer>(TableName, this.OnTableLoaded);
        hooks.OnCostumeChanged += _ => this.ApplyAll();
    }

    private void OnTableLoaded(DataTable<FBtlAlloutSequencer> loaded)
    {
        if (!this.tables.Contains(loaded))
        {
            this.tables.Add(loaded);
        }

        var snapshot = new Dictionary<string, FBtlAlloutSequencer>();
        foreach (var row in loaded.Rows)
        {
            snapshot[row.Name] = *row.Self;
        }
        this.vanilla[loaded] = snapshot;

        Log.Information(
            $"Allout sequencer table loaded #{++this.loadCount}: {snapshot.Count} rows " +
            $"[{string.Join(", ", loaded.Rows.Select(r => r.Name))}]");

        this.ApplyAll();
    }

    private void ApplyAll()
    {
        foreach (var table in this.tables)
        {
            this.RestoreVanilla(table);

            foreach (var costume in this.manager.GetCurrentCostumes())
            {
                this.Apply(table, costume);
            }
        }
    }

    private void RestoreVanilla(DataTable<FBtlAlloutSequencer> table)
    {
        if (!this.vanilla.TryGetValue(table, out var snapshot))
        {
            return;
        }

        foreach (var row in table.Rows)
        {
            if (snapshot.TryGetValue(row.Name, out var original))
            {
                *row.Self = original;
                row.Self->AlloutA.baseObj.baseObj.WeakPtr = new();
                row.Self->AlloutB.baseObj.baseObj.WeakPtr = new();
            }
        }
    }

    private void Apply(DataTable<FBtlAlloutSequencer> table, Costume costume)
    {
        var aPath = costume.Config.Allout.AoalsA;
        var bPath = costume.Config.Allout.AoalsB;
        if (string.IsNullOrEmpty(aPath) && string.IsNullOrEmpty(bPath))
        {
            return;
        }

        var rowName = $"pc_{(int)costume.Character}";
        var row = table.Rows.FirstOrDefault(x => x.Name.Equals(rowName, StringComparison.OrdinalIgnoreCase));
        if (row == null)
        {
            return;
        }

        var ptr = row.Self;

        if (!string.IsNullOrEmpty(aPath))
        {
            this.SetSequence(&ptr->AlloutA, AssetUtils.GetUnrealAssetPath(aPath));
            Log.Debug($"Allout sequencer [{rowName}] A set to {aPath} ({costume.Character} || {costume.Name}).");
        }

        if (!string.IsNullOrEmpty(bPath))
        {
            this.SetSequence(&ptr->AlloutB, AssetUtils.GetUnrealAssetPath(bPath));
            Log.Debug($"Allout sequencer [{rowName}] B set to {bPath} ({costume.Character} || {costume.Name}).");
        }
    }

    private void SetSequence(TSoftObjectPtr<ULevelSequence>* seq, string unrealPath)
    {
        seq->baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(unrealPath);
        seq->baseObj.baseObj.WeakPtr = new();
    }
}
