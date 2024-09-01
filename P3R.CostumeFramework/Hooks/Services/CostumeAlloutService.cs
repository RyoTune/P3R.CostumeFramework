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
            if (this.itemEquip.TryGetEquipCostume(character, out var costume))
            {
                var alloutRowName = $"PC{(int)character}";
                var alloutRow = (FBtlAlloutFinishTexture*)table.Rows.First(x => x.Name == alloutRowName).Self;

                ModUtils.IfNotNull(costume.Config.Allout.NormalPath, path => this.SetAssetPath(alloutRow, CostumeAssetType.AlloutNormal, path!));
                ModUtils.IfNotNull(costume.Config.Allout.NormalMaskPath, path => this.SetAssetPath(alloutRow, CostumeAssetType.AlloutNormalMask, path!));
                ModUtils.IfNotNull(costume.Config.Allout.SpecialPath, path => this.SetAssetPath(alloutRow, CostumeAssetType.AlloutSpecial, path!));
                ModUtils.IfNotNull(costume.Config.Allout.SpecialMaskPath, path => this.SetAssetPath(alloutRow, CostumeAssetType.AlloutSpecialMask, path!));
                ModUtils.IfNotNull(costume.Config.Allout.PlgPath, path => this.SetAssetPath(alloutRow, CostumeAssetType.AlloutPlg, path!));
                ModUtils.IfNotNull(costume.Config.Allout.TextPath, path => this.SetAssetPath(alloutRow, CostumeAssetType.AlloutText, path!));
            }
        }
    }

    private void SetAssetPath(FBtlAlloutFinishTexture* allout, CostumeAssetType type, string path)
    {
        var unrealPathFName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(path)!);
        switch (type)
        {
            case CostumeAssetType.AlloutNormal:
                allout->TextureNormal.baseObj.baseObj.ObjectId.AssetPathName = unrealPathFName;
                break;
            case CostumeAssetType.AlloutNormalMask:
                allout->TextureNormalMask.baseObj.baseObj.ObjectId.AssetPathName = unrealPathFName;
                break;
            case CostumeAssetType.AlloutSpecial:
                allout->TextureSpecialOutfit.baseObj.baseObj.ObjectId.AssetPathName = unrealPathFName;
                break;
            case CostumeAssetType.AlloutSpecialMask:
                allout->TextureSpecialMask.baseObj.baseObj.ObjectId.AssetPathName = unrealPathFName;
                break;
            case CostumeAssetType.AlloutPlg:
                allout->TexturePlg.baseObj.baseObj.ObjectId.AssetPathName = unrealPathFName;
                break;
            case CostumeAssetType.AlloutText:
                allout->TextureText.baseObj.baseObj.ObjectId.AssetPathName = unrealPathFName;
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
