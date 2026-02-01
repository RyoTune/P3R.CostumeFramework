using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeWeaponService
{
    private readonly IUnreal unreal;
    private readonly CostumeManager manager;

    private DataTable<FBustupParamTable>? boolTable;
    private DataTable<FBustupParamTable>? pathTable;

    private readonly List<Costume> pendingCostumes = [];

    public CostumeWeaponService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
    { 
        this.unreal = unreal;
        this.manager = manager;

        dt.FindDataTable<FBustupParamTable>("WeaponBool", table =>
        {
            this.boolTable = table;
            this.CheckPendingCostumes();
        });

        dt.FindDataTable<FBustupParamTable>("WeaponPath", table =>
        {
            this.pathTable = table;
            this.CheckPendingCostumes();
        });

        hooks.OnCostumeChanged += this.RefreshWeaponForCostume;
    }

    private void CheckPendingCostumes()
    {
        if (this.pendingCostumes.Count > 0 && (this.boolTable != null || this.pathTable != null))
        {
            this.ApplyPendingCostumes();
        }
    }

    private void RefreshWeaponForCostume(Costume costume)
    {
        if (this.boolTable == null || this.pathTable == null)
        {
            Log.Debug("Weapon tables not yet fully loaded; queueing refresh.");
            if (!this.pendingCostumes.Contains(costume))
            {
                this.pendingCostumes.Add(costume);
            }
            return;
        }

        this.ApplyWeapon(costume);
    }

    private void ApplyPendingCostumes()
    {
        foreach (var costume in this.pendingCostumes.ToArray())
        {
            this.ApplyWeapon(costume);
        }

        this.pendingCostumes.Clear();
    }

    private void ApplyWeapon(Costume costume)
    {
        var rowName = $"PC{(int)costume.Character}";
        var weaponMeshPath = costume.Config.Weapon.MeshPath;
        var hasWeapon = !string.IsNullOrEmpty(weaponMeshPath);

        if (this.boolTable != null)
        {
            var boolRow = this.boolTable.Rows.FirstOrDefault(x => x.Name == rowName);
            if (boolRow != null)
            {
                var boolRowPtr = boolRow.Self;
                boolRowPtr->Pose = this.unreal.FString(hasWeapon ? "True" : "False");
                Log.Debug($"Set WeaponBool for {rowName} to {(hasWeapon ? "True" : "False")}.");
            }
        }

        if (this.pathTable != null && hasWeapon)
        {
            var pathRow = this.pathTable.Rows.FirstOrDefault(x => x.Name == rowName);
            if (pathRow != null)
            {
                var pathRowPtr = pathRow.Self;
                var fullPath = AssetUtils.GetUnrealAssetPath(weaponMeshPath!);
                pathRowPtr->Pose = this.unreal.FString(fullPath);
                Log.Debug($"Set WeaponPath for {rowName} to {fullPath}.");
            }
        }
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