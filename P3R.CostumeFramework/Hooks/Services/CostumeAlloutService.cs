using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Utils;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeAlloutService
{
    private readonly IUnreal unreal;
    private readonly ItemEquip itemEquip;

    public CostumeAlloutService(IDataTables dt, IUnreal unreal, ItemEquip itemEquip)
    {
        this.unreal = unreal;
        this.itemEquip = itemEquip;

        dt.FindDataTable("DT_BtlAlloutFinishTexture", this.BtlAlloutFinishTextureLoaded);
    }

    private void BtlAlloutFinishTextureLoaded(DataTable table)
    {
        foreach (var character in Characters.PC)
        {
            if (this.itemEquip.TryGetEquippedCostume(character, out var costume))
            {
                var alloutRowName = $"PC{(int)character}";
                var alloutRow = (FBtlAlloutFinishTexture*)table.Rows.First(x => x.Name == alloutRowName).Self;

                ModUtils.IfNotNull(costume.Config.Allout.NormalPath, path => this.SetAlloutAssetPath(alloutRow, CostumeAssetType.AlloutNormal, path!));
                ModUtils.IfNotNull(costume.Config.Allout.NormalMaskPath, path => this.SetAlloutAssetPath(alloutRow, CostumeAssetType.AlloutNormalMask, path!));
                ModUtils.IfNotNull(costume.Config.Allout.SpecialPath, path => this.SetAlloutAssetPath(alloutRow, CostumeAssetType.AlloutSpecial, path!));
                ModUtils.IfNotNull(costume.Config.Allout.SpecialMaskPath, path => this.SetAlloutAssetPath(alloutRow, CostumeAssetType.AlloutSpecialMask, path!));
                ModUtils.IfNotNull(costume.Config.Allout.PlgPath, path => this.SetAlloutAssetPath(alloutRow, CostumeAssetType.AlloutPlg, path!));
                ModUtils.IfNotNull(costume.Config.Allout.TextPath, path => this.SetAlloutAssetPath(alloutRow, CostumeAssetType.AlloutText, path!));
            }
        }
    }

    private void SetAlloutAssetPath(FBtlAlloutFinishTexture* allout, CostumeAssetType type, string path)
    {
        switch (type)
        {
            case CostumeAssetType.AlloutNormal:
                allout->TextureNormal.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(path)!);
                break;
            case CostumeAssetType.AlloutNormalMask:
                allout->TextureNormalMask.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(path)!);
                break;
            case CostumeAssetType.AlloutSpecial:
                allout->TextureSpecialOutfit.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(path)!);
                break;
            case CostumeAssetType.AlloutSpecialMask:
                allout->TextureSpecialMask.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(path)!);
                break;
            case CostumeAssetType.AlloutPlg:
                allout->TexturePlg.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(path)!);
                break;
            case CostumeAssetType.AlloutText:
                allout->TexturePlg.baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(path)!);
                break;
            default:
                break;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct FBtlAlloutFinishTexture
    {
        public SoftObjectProperty TextureNormal;
        public SoftObjectProperty TextureNormalMask;
        public SoftObjectProperty TextureSpecialOutfit;
        public SoftObjectProperty TextureSpecialMask;
        public SoftObjectProperty TextureText;
        public SoftObjectProperty TexturePlg;
    }

    [StructLayout(LayoutKind.Sequential, Size = 40)]
    private unsafe struct SoftObjectProperty
    {
        public FSoftObjectPtr baseObj;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FSoftObjectPtr
    {
        public TPersistentObjectPtr<FSoftObjectPath> baseObj;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TPersistentObjectPtr<T> where T : unmanaged
    {
        public FWeakObjectPtr WeakPtr;

        public int TagAtLastTest;

        public T ObjectId;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x18)]
    public unsafe struct FSoftObjectPath
    {
        [FieldOffset(0x0000)] public FName AssetPathName;
        [FieldOffset(0x0008)] public FString SubPathString;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FWeakObjectPtr
    {
        public int ObjectIndex;
        public int ObjectSerialNumber;
    }
}
