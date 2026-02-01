using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeCritCameraService
{
    private readonly IUnreal unreal;
    private readonly CostumeManager manager;
    private DataTable<FBustupParamTable>? critTable;
    private readonly List<Costume> pendingCostumes = [];

    public CostumeCritCameraService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    {
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBustupParamTable>("crit", this.CritTableLoaded);

        hooks.OnCostumeChanged += this.RefreshCameraForCostume;
    }

    private void CritTableLoaded(DataTable<FBustupParamTable> table)
    {
        this.critTable = table;

        this.pendingCostumes.AddRange(this.manager.GetCurrentCostumes());

        this.ApplyPendingCostumes();
    }

    private void RefreshCameraForCostume(Costume costume)
    {
        if (this.critTable == null)
        {
            Log.Debug("Crit Camera table not yet loaded; queueing refresh.");
            this.pendingCostumes.Add(costume);
            return;
        }

        this.ApplyCamera(costume);
    }

    private void ApplyPendingCostumes()
    {
        if (this.critTable == null || this.pendingCostumes.Count == 0)
        {
            return;
        }

        foreach (var costume in this.pendingCostumes.ToArray())
        {
            this.ApplyCamera(costume);
        }

        this.pendingCostumes.Clear();
    }

    private void ApplyCamera(Costume costume)
    {
        var table = this.critTable;
        if (table == null)
        {
            Log.Debug("Crit Camera table not yet loaded; skipping application.");
            return;
        }

        var rowName = $"PC{(int)costume.Character}";
        var row = table.Rows.FirstOrDefault(x => x.Name == rowName);
        if (row == null)
        {
            Log.Debug($"Crit Camera row not found for {rowName}.");
            return;
        }

        var rowPtr = row.Self;

        string cameraPath;
        if (costume.Config.Animation.CritCamera != null)
        {
            cameraPath = AssetUtils.GetUnrealAssetPath(costume.Config.Animation.CritCamera);
        }
        else
        {
            var charId = AssetUtils.GetCharIdStringShort(costume.Character);
            cameraPath = $"/Game/Xrd777/Battle/Critical/LS_Btl_Critical_Pc{charId}.LS_Btl_Critical_Pc{charId}";
        }

        Log.Debug($"Set crit camera for {rowName} to {cameraPath}.");
        rowPtr->Pose = this.unreal.FString(cameraPath);
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