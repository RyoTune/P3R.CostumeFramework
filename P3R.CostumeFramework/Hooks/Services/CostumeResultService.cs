using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal sealed unsafe class CostumeResultService
{
    private const string TableName = "DT_BtlResultLS";

    private readonly IUnreal unreal;
    private readonly CostumeManager manager;

    private readonly List<DataTable<FBtlResultSequence>> tables = new();
    private readonly Dictionary<DataTable<FBtlResultSequence>, Dictionary<string, FBtlResultSequence>> vanilla = new();

    private int loadCount;

    public CostumeResultService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    {
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBtlResultSequence>(TableName, this.OnTableLoaded);
        hooks.OnCostumeChanged += _ => this.ApplyAll();
    }

    private void OnTableLoaded(DataTable<FBtlResultSequence> loaded)
    {
        if (!this.tables.Contains(loaded))
        {
            this.tables.Add(loaded);
        }

        var snapshot = new Dictionary<string, FBtlResultSequence>();
        foreach (var row in loaded.Rows)
        {
            snapshot[row.Name] = *row.Self;
        }
        this.vanilla[loaded] = snapshot;

        Log.Information(
            $"Result LS table loaded #{++this.loadCount}: {snapshot.Count} rows " +
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

    private void RestoreVanilla(DataTable<FBtlResultSequence> table)
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
                row.Self->Sequencer.baseObj.baseObj.WeakPtr = new();
            }
        }
    }

    private void Apply(DataTable<FBtlResultSequence> table, Costume costume)
    {
        var lsPath = costume.Config.Result.LsPath;
        if (string.IsNullOrEmpty(lsPath))
        {
            return;
        }

        var rowName = $"pc_{(int)costume.Character}";
        var row = table.Rows.FirstOrDefault(x => x.Name.Equals(rowName, StringComparison.OrdinalIgnoreCase));
        if (row == null)
        {
            return;
        }

        var unrealPath = AssetUtils.GetUnrealAssetPath(lsPath);
        var ptr = row.Self;
        ptr->Sequencer.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(unrealPath);
        ptr->Sequencer.baseObj.baseObj.WeakPtr = new();

        Log.Debug($"Result LS [{rowName}] set to {unrealPath} ({costume.Character} || {costume.Name}).");
    }
}