using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;
internal abstract unsafe class PoseTableServiceBase
{
    protected readonly IUnreal Unreal;
    protected readonly CostumeManager Manager;

    private readonly string label;
    private readonly List<Costume> pendingCostumes = [];
    private DataTable<FBustupParamTable>? table;

    protected PoseTableServiceBase(
        IDataTables dt,
        IUnreal unreal,
        CostumeManager manager,
        CostumeHooks hooks,
        string tableName,
        string label)
    {
        this.Unreal = unreal;
        this.Manager = manager;
        this.label = label;

        dt.FindDataTable<FBustupParamTable>(tableName, this.OnTableLoaded);
        hooks.OnCostumeChanged += this.RefreshForCostume;
    }
    protected abstract string? GetPoseValue(Costume costume, string rowName);
    protected virtual bool ShouldApply(Costume costume) => true;
    protected virtual void OnApplied(Costume costume, FBustupParamTable* row, string rowName) { }

    private void OnTableLoaded(DataTable<FBustupParamTable> loadedTable)
    {
        this.table = loadedTable;

        this.pendingCostumes.AddRange(this.Manager.GetCurrentCostumes());
        this.ApplyPendingCostumes();
    }

    private void RefreshForCostume(Costume costume)
    {
        if (this.table == null)
        {
            Log.Debug($"{this.label} table not yet loaded; queueing refresh.");
            if (!this.pendingCostumes.Contains(costume))
            {
                this.pendingCostumes.Add(costume);
            }
            return;
        }

        this.Apply(costume);
    }

    private void ApplyPendingCostumes()
    {
        if (this.table == null || this.pendingCostumes.Count == 0)
        {
            return;
        }

        foreach (var costume in this.pendingCostumes.ToArray())
        {
            this.Apply(costume);
        }

        this.pendingCostumes.Clear();
    }

    private void Apply(Costume costume)
    {
        if (this.table == null)
        {
            return;
        }

        if (!this.ShouldApply(costume))
        {
            return;
        }

        var rowName = $"PC{(int)costume.Character}";
        var row = this.table.Rows.FirstOrDefault(x => x.Name == rowName);
        if (row == null)
        {
            Log.Debug($"{this.label} row not found for {rowName}.");
            return;
        }

        var rowPtr = row.Self;

        var poseValue = this.GetPoseValue(costume, rowName);
        if (poseValue != null)
        {
            rowPtr->Pose = this.Unreal.FString(poseValue);
            Log.Debug($"Set {this.label} for {rowName} to {poseValue}.");
        }

        this.OnApplied(costume, rowPtr, rowName);
    }
}
