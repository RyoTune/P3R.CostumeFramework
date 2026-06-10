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

    private DataTable<FBtlResultSequence>? table;

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
        Log.Information($"Result LS rows: {string.Join(", ", loaded.Rows.Select(r => r.Name))}");
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
                row.Self->Sequencer.baseObj.baseObj.WeakPtr = new();
            }
        }
    }

    private void Apply(Costume costume)
    {
        var lsPath = costume.Config.Result.LsPath;
        if (string.IsNullOrEmpty(lsPath))
        {
            return;
        }

        var rowName = $"pc_{(int)costume.Character}";
        var row = this.table!.Rows.FirstOrDefault(x => x.Name.Equals(rowName, StringComparison.OrdinalIgnoreCase));
        if (row == null)
        {
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
