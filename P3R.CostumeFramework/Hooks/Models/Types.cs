using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Models;

[StructLayout(LayoutKind.Explicit, Size = 0x50)]
public unsafe struct FAppCharCostumePartsData
{
    [FieldOffset(0x0000)] public TSoftObjectPtr<USkeletalMesh> Mesh;
    [FieldOffset(0x0028)] public TSoftClassPtr<UObject> Anim;
}

[StructLayout(LayoutKind.Explicit, Size = 0x148)]
public unsafe struct FAppCharCostumeData
{
    [FieldOffset(0x0000)] public FAppCharCostumePartsData Base;
    [FieldOffset(0x0050)] public FAppCharCostumePartsData Costume;
    [FieldOffset(0x00A0)] public FAppCharCostumePartsData Hair;
    [FieldOffset(0x00F0)] public FAppCharCostumePartsData Face;
    [FieldOffset(0x0140)] public int BagKeyID;
}

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct FTableRowBase
{
}

[StructLayout(LayoutKind.Explicit, Size = 0x180)]
public unsafe struct FAppCharTableRow
{
    [FieldOffset(0x0000)] public FTableRowBase baseObj;
    [FieldOffset(0x0008)] public float CapsuleHalfHeight;
    //[FieldOffset(0x000C)] public FVector MeshLocation;
    //[FieldOffset(0x0018)] public TMap<EAnimPackID, TSoftObjectPtr<UAppCharAnimDataAsset>> Anims;
    //[FieldOffset(0x0068)] public TSoftObjectPtr<UAppCharFaceAnimDataAsset> FaceAnim;
    [FieldOffset(0x0090)] public TMap<int, FAppCharCostumeData> Costumes;
    [FieldOffset(0x00E0)] public TMap<int, FAppCharWeaponData> WeaponType;
    //[FieldOffset(0x0130)] public TMap<int, FAppCharBagData> BagType;
}

[StructLayout(LayoutKind.Explicit, Size = 0x3A0)]
public unsafe struct USkeletalMesh
{
    //[FieldOffset(0x0000)] public UStreamableRenderAsset baseObj;
    //[FieldOffset(0x0080)] public USkeleton* Skeleton;
    //[FieldOffset(0x0088)] public FBoxSphereBounds ImportedBounds;
    //[FieldOffset(0x00A4)] public FBoxSphereBounds ExtendedBounds;
    //[FieldOffset(0x00C0)] public FVector PositiveBoundsExtension;
    //[FieldOffset(0x00CC)] public FVector NegativeBoundsExtension;
    //[FieldOffset(0x00D8)] public TArray<FSkeletalMaterial> Materials;
    //[FieldOffset(0x00E8)] public TArray<FBoneMirrorInfo> SkelMirrorTable;
    //[FieldOffset(0x00F8)] public TArray<FSkeletalMeshLODInfo> LODInfo;
    //[FieldOffset(0x0158)] public FPerPlatformInt MinLOD;
    //[FieldOffset(0x015C)] public FPerPlatformBool DisableBelowMinLodStripping;
    //[FieldOffset(0x015D)] public EAxis SkelMirrorAxis;
    //[FieldOffset(0x015E)] public EAxis SkelMirrorFlipAxis;
    //[FieldOffset(0x015F)] public byte bUseFullPrecisionUVs;
    //[FieldOffset(0x015F)] public byte bUseHighPrecisionTangentBasis;
    //[FieldOffset(0x015F)] public byte bHasBeenSimplified;
    //[FieldOffset(0x015F)] public byte bHasVertexColors;
    //[FieldOffset(0x015F)] public byte bEnablePerPolyCollision;
    //[FieldOffset(0x0160)] public UBodySetup* BodySetup;
    //[FieldOffset(0x0168)] public UPhysicsAsset* PhysicsAsset;
    //[FieldOffset(0x0170)] public UPhysicsAsset* ShadowPhysicsAsset;
    //[FieldOffset(0x0178)] public TArray<IntPtr> NodeMappingData;
    //[FieldOffset(0x0188)] public byte bSupportRayTracing;
    //[FieldOffset(0x0190)] public TArray<IntPtr> MorphTargets;
    //[FieldOffset(0x0318)] public TSubclassOf<UAnimInstance> PostProcessAnimBlueprint;
    //[FieldOffset(0x0320)] public TArray<IntPtr> MeshClothingAssets;
    //[FieldOffset(0x0330)] public FSkeletalMeshSamplingInfo SamplingInfo;
    //[FieldOffset(0x0360)] public TArray<IntPtr> AssetUserData;
    //[FieldOffset(0x0370)] public TArray<IntPtr> Sockets;
    //[FieldOffset(0x0390)] public TArray<FSkinWeightProfileInfo> SkinWeightProfiles;
}

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public unsafe struct FAppCharWeaponMeshData
{
    [FieldOffset(0x0000)] public TSoftObjectPtr<USkeletalMesh> Mesh;
    [FieldOffset(0x0028)] public bool MultiEquip;
}

[StructLayout(LayoutKind.Explicit, Size = 0x90)]
public unsafe struct FAppCharWeaponTableRow
{
    [FieldOffset(0x0000)] public FTableRowBase baseObj;
    [FieldOffset(0x0008)] public TMap<int, FAppCharWeaponMeshData> Data;
    [FieldOffset(0x0058)] public TSoftClassPtr<UObject> Anim;
    //[FieldOffset(0x0080)] public TArray<FAppCharWeapoAnimAssetTypeData> AnimAsset;
}

[StructLayout(LayoutKind.Explicit, Size = 0x448)]
public unsafe struct AAppCharWeaponBase
{
    //[FieldOffset(0x0000)] public AAppActor baseObj;
    [FieldOffset(0x0278)] public int PlayerId;
    //[FieldOffset(0x0280)] public USceneComponent* Root;
    //[FieldOffset(0x0288)] public USkeletalMeshComponent* Mesh;
    [FieldOffset(0x0290)] public FAppCharWeaponTableRow WeaponTbl;
    [FieldOffset(0x0320)] public FName AttachSocketName;
    [FieldOffset(0x0328)] public int HideMaterialID;
    //[FieldOffset(0x0330)] public UAppCharWeaponAnimDataAsset* AnimPack;
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe struct FAppCharWeaponData
{
    [FieldOffset(0x0000)] public TArray<TSoftClassPtr<AAppCharWeaponBase>> BluePrints;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSoftClassPtr<T> where T : unmanaged
{
    public FSoftObjectPtr baseObj;
}

[StructLayout(LayoutKind.Sequential, Size = 40)]
public unsafe struct SoftObjectProperty
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

[StructLayout(LayoutKind.Sequential)]
public struct TSoftObjectPtr<T> where T : unmanaged
{
    public FSoftObjectPtr baseObj;
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

[StructLayout(LayoutKind.Sequential)]
public unsafe struct TMap<KeyType, ValueType>
    where KeyType : unmanaged, IEquatable<KeyType>
    where ValueType : unmanaged
{
    public TMapElement<KeyType, ValueType>* elements;
    public int mapNum;
    public int mapMax;

    public ValueType* TryGet(KeyType key)
    {
        if (mapNum == 0 || elements == null) return null;
        ValueType* value = null;
        for (int i = 0; i < mapNum; i++)
        {
            var currElem = &elements[i];
            if (currElem->Key.Equals(key))
            {
                value = &currElem->Value;
                break;
            }
        }
        return value;
    }

    public bool TryGet(KeyType key, out ValueType* value)
    {
        value = TryGet(key);
        return value != null;
    }

    public ValueType* GetByIndex(int idx)
    {
        if (idx < 0 || idx > mapNum) return null;
        return &elements[idx].Value;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct TMapElement<KeyType, ValueType>
    where KeyType : unmanaged, IEquatable<KeyType>
    where ValueType : unmanaged
{
    public KeyType Key;
    public ValueType Value;
    public uint HashNextId;
    public uint HashIndex;
}

public interface IMapHashable
{
    public uint GetTypeHash();
}