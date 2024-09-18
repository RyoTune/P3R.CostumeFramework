using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeShellService
{
    private const int SHELL_COSTUME_ID = 51;
    private readonly Dictionary<Character, nint> shellDataPtrs = [];
    private readonly Dictionary<Character, int> prevCostumeIds = [];
    private readonly Dictionary<Character, Costume> defaultCostumes = [];
    private readonly Dictionary<Character, nint> charRows = [];

    private readonly IUnreal unreal;
    private readonly CostumeRegistry costumes;
    private readonly DungeonAnimManager dngManager = new();
    private bool useFemc;

    public CostumeShellService(IDataTables dt, IUObjects uobjs, IUnreal unreal, CostumeRegistry costumes)
    {
        this.unreal = unreal;
        this.costumes = costumes;

        dt.FindDataTable<FAppCharTableRow>("DT_Costume", table =>
        {
            foreach (var character in Characters.PC)
            {
                var charRowName = $"PC{(int)character}";
                var charRowObj = table.Rows.FirstOrDefault(x => x.Name == charRowName);
                if (charRowObj == null)
                {
                    Log.Debug($"Character row missing: {charRowName}");
                    continue;
                }

                var charRow = charRowObj.Self;
                var costumes = charRow->Costumes;
                if (costumes.TryGet(SHELL_COSTUME_ID, out var costume))
                {
                    this.shellDataPtrs[character] = (nint)costume;
                    this.prevCostumeIds[character] = -1;

                    if (character == Character.AigisReal)
                    {
                        this.shellDataPtrs[Character.Aigis] = (nint)costume;
                        this.prevCostumeIds[Character.Aigis] = -1;
                    }
                }
                else
                {
                    Log.Error($"{character} missing shell Costume ID: {SHELL_COSTUME_ID}");
                }

                this.defaultCostumes[character] = new DefaultCostume(character);
                this.charRows[character] = (nint)charRow;
            }

            this.defaultCostumes[Character.FEMC] = new FemcCostume();
        });

        uobjs.FindObject("SKEL_Human", obj =>
        {
            var bones = new Bones();
            string[] keywords =
            [
                "head",
                "eye",
                "brow",
                "cheek",
                "nose",
                "mouth",
                "jaw",
                "tongue",
                "lips",
                "hair",
                "face",
                "mask",
                "iris",
                "pupil",
                "laugh",
                "tooth"
            ];

            var skel = (USkeleton*)obj.Self;
            for (int i = 0; i < skel->BoneTree.Num; i++)
            {
                var bone = &skel->BoneTree.AllocatorInstance[i];
                var name = bones[i];
                bone->TranslationRetargetingMode = EBoneTranslationRetargetingMode.OrientAndScale;
                if (keywords.Any(x => name.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Debug($"SKEL_Human ({name}): Retarget bone animation to {EBoneTranslationRetargetingMode.OrientAndScale}.");
                    bone->TranslationRetargetingMode = EBoneTranslationRetargetingMode.OrientAndScale;
                }
            }
        });

        uobjs.ObjectCreated += this.dngManager.Update;
    }

    public void SetUseFemc(bool useFemc) => this.useFemc = useFemc;

    public int UpdateCostume(Character character, int costumeId)
    {
        if (costumeId == SHELL_COSTUME_ID)
        {
            this.prevCostumeIds[character] = costumeId;
            this.SetCostumeAssets(defaultCostumes[character]);
            Log.Debug($"{character}: Reset shell costume data.");
        }

        if (costumeId < GameCostumes.BASE_MOD_COSTUME_ID)
        {
            return costumeId;
        }

        var shouldUpdateData = this.prevCostumeIds[character] != costumeId;
        if (shouldUpdateData && this.costumes.TryGetCostume(character, costumeId, out var costume))
        {
            this.SetCostumeAssets(costume);
            this.SetCostumeAnims(costume);
            this.prevCostumeIds[character] = costumeId;
        }

        return SHELL_COSTUME_ID;
    }

    private void SetCostumeAssets(Costume costume)
    {
        this.SetCostumeAsset(costume, CostumeAssetType.BaseMesh);
        this.SetCostumeAsset(costume, CostumeAssetType.CostumeMesh);
        this.SetCostumeAsset(costume, CostumeAssetType.FaceMesh);
        this.SetCostumeAsset(costume, CostumeAssetType.HairMesh);
        this.SetCostumeAsset(costume, CostumeAssetType.BaseAnim);
        this.SetCostumeAsset(costume, CostumeAssetType.CostumeAnim);
        this.SetCostumeAsset(costume, CostumeAssetType.FaceAnim);
        this.SetCostumeAsset(costume, CostumeAssetType.HairAnim);
    }

    private void SetCostumeAsset(Costume costume, CostumeAssetType assetType)
    {
        var assetFile = costume.Config.GetAssetFile(assetType) ?? this.GetDefaultAsset(costume.Character, assetType);
        if (assetFile == null)
        {
            Log.Error($"Costume asset path is null.\nCostume ({costume.Character}): {costume.Name} || Asset: {assetType}");
            return;
        }

        var assetPath = AssetUtils.GetUnrealAssetPath(assetFile);
        if (assetType == CostumeAssetType.BaseAnim
            || assetType == CostumeAssetType.CostumeAnim
            || assetType == CostumeAssetType.FaceAnim
            || assetType == CostumeAssetType.HairAnim)
        {
            assetPath += "_C";
        }

        var assetFName = assetFile != "None" ? *this.unreal.FName(assetPath) : *this.unreal.FName("None");
        var data = (FAppCharCostumeData*)this.shellDataPtrs[costume.Character];

        switch (assetType)
        {
            case CostumeAssetType.BaseMesh:
                data->Base.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Base.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.CostumeMesh:
                data->Costume.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Costume.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.HairMesh:
                data->Hair.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Hair.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.FaceMesh:
                data->Face.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Face.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.BaseAnim:
                data->Base.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Base.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.CostumeAnim:
                data->Costume.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Costume.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.HairAnim:
                data->Hair.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Hair.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.FaceAnim:
                data->Face.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                data->Face.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            default:
                break;
        }
    }

    private void SetCostumeAnims(Costume costume)
    {
        var charRow = (FAppCharTableRow*)this.charRows[costume.Character];
        var anims = charRow->Anims;

        if (Enum.TryParse<Character>(costume.Config.Anims.Common, true, out var commonAnim))
        {
            var anim = anims.GetByIndex(0);
            var animAsset = AssetUtils.GetUnrealAssetPath(commonAnim, 0, CostumeAssetType.CommonAnim)!;
            anim->baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(animAsset);
            anim->baseObj.baseObj.WeakPtr = new();
        }

        if (Enum.TryParse<Character>(costume.Config.Anims.Dungeon, true, out var dungeonAnim))
        {
            var anim = anims.GetByIndex(1);
            var animAsset = AssetUtils.GetUnrealAssetPath(dungeonAnim, 0, CostumeAssetType.DungeonAnim)!;
            anim->baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(animAsset);
            anim->baseObj.baseObj.WeakPtr = new();
        }

        if (Enum.TryParse<Character>(costume.Config.Anims.Combine, true, out var combineAnim))
        {
            var anim = anims.GetByIndex(2);
            var animAsset = AssetUtils.GetUnrealAssetPath(combineAnim, 0, CostumeAssetType.CombineAnim)!;
            anim->baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(animAsset);
            anim->baseObj.baseObj.WeakPtr = new();
        }

        if (Enum.TryParse<Character>(costume.Config.Anims.Event, true, out var eventAnim))
        {
            var anim = anims.GetByIndex(3);
            var animAsset = AssetUtils.GetUnrealAssetPath(eventAnim, 0, CostumeAssetType.EventAnim)!;
            anim->baseObj.baseObj.ObjectId.AssetPathName = *this.unreal.FName(animAsset);
            anim->baseObj.baseObj.WeakPtr = new();
        }
    }

    private string? GetDefaultAsset(Character character, CostumeAssetType assetType)
    {
        if (character == Character.Player && this.useFemc)
        {
            return this.defaultCostumes[Character.FEMC].Config.GetAssetFile(assetType);
        }

        return this.defaultCostumes[character].Config.GetAssetFile(assetType);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x1C0)]
public unsafe struct UAnimSequence
{
    //[FieldOffset(0x0000)] public UAnimSequenceBase baseObj;
    //[FieldOffset(0x00A8)] public int NumFrames;
    //[FieldOffset(0x00B0)] public TArray<FTrackToSkeletonMap> TrackToSkeletonMapTable;
    //[FieldOffset(0x00D0)] public UAnimBoneCompressionSettings* BoneCompressionSettings;
    //[FieldOffset(0x00D8)] public UAnimCurveCompressionSettings* CurveCompressionSettings;
    //[FieldOffset(0x0150)] public EAdditiveAnimationType AdditiveAnimType;
    //[FieldOffset(0x0151)] public EAdditiveBasePoseType RefPoseType;
    //[FieldOffset(0x0158)] public UAnimSequence* RefPoseSeq;
    //[FieldOffset(0x0160)] public int RefFrameIndex;
    //[FieldOffset(0x0164)] public FName RetargetSource;
    //[FieldOffset(0x0170)] public TArray<FTransform> RetargetSourceAssetReferencePose;
    //[FieldOffset(0x0180)] public EAnimInterpolationType Interpolation;
    //[FieldOffset(0x0181)] public bool bEnableRootMotion;
    //[FieldOffset(0x0182)] public ERootMotionRootLock RootMotionRootLock;
    //[FieldOffset(0x0183)] public bool bForceRootLock;
    //[FieldOffset(0x0184)] public bool bUseNormalizedRootMotionScale;
    //[FieldOffset(0x0185)] public bool bRootMotionSettingsCopiedFromMontage;
    //[FieldOffset(0x0188)] public TArray<FAnimSyncMarker> AuthoredSyncMarkers;
    //[FieldOffset(0x01B0)] public TArray<FBakedCustomAttributePerBoneData> BakedPerBoneCustomAttributeData;
}

public class DungeonAnimManager
{
    private readonly List<DngAnimReplacer> replacers =
    [
    ];

    public DungeonAnimManager()
    {
        foreach (var type in Enum.GetValues<DungeonAnim>())
        {
            this.replacers.Add(new(type, Character.Player, Character.Mitsuru));
        }
    }

    public void Update(UnrealObject obj)
    {
        foreach (var replacer in replacers)
        {
            replacer.Update(obj);
        }
    }
}

public unsafe class DngAnimReplacer(DungeonAnim type, Character target, Character replacer)
{
    private readonly DungeonAnim type = type;
    private readonly Character target = target;
    private readonly Character replacer = replacer;
    private readonly string targetAnimName = GetDungeonAnimPath(target, type);
    private readonly string newAnimName = GetDungeonAnimPath(replacer, type);

    private UAnimSequence* targetAnim;
    private UAnimSequence* newAnim;

    private bool hasReplaced;

    public void Update(UnrealObject obj)
    {
        if (this.hasReplaced)
        {
            return;
        }

        if (obj.Name.Equals(this.targetAnimName, StringComparison.OrdinalIgnoreCase))
        {
            this.targetAnim = (UAnimSequence*)obj.Self;
        }
        else if (obj.Name.Equals(this.newAnimName, StringComparison.OrdinalIgnoreCase))
        {
            this.newAnim = (UAnimSequence*)obj.Self;
        }

        if (this.targetAnim != null && this.newAnim != null)
        {
            *this.targetAnim = *this.newAnim;
            this.hasReplaced = true;
            Log.Information($"DngAnim Replaced: {this.type} || Target: {this.target} || New: {this.replacer}");
        }
    }

    private static string GetDungeonAnimPath(Character character, DungeonAnim anim)
        => anim switch
        {
            DungeonAnim.DngAlsNPose => $"A_PC{GetCharIdString(character)}_DNG_ALS_N_POSE",
            DungeonAnim.DngAlsNRunF => $"A_PC{GetCharIdString(character)}_DNG_ALS_N_Run_F",
            DungeonAnim.DngAlsNWalkF => $"A_PC{GetCharIdString(character)}_DNG_ALS_N_Walk_F",
            DungeonAnim.DngAlsPoseRunStrideZero => $"A_PC{GetCharIdString(character)}_DNG_ALS_POSE_RunStrideZero",
            DungeonAnim.DngAlsStopPose => $"A_PC{GetCharIdString(character)}_DNG_ALS_STOP_POSE",
            DungeonAnim.DngPoseNeutral => $"A_PC{GetCharIdString(character)}_DNG0001_POSE_Neutral",
            DungeonAnim.DngBaseSuddenStop => $"A_PC{GetCharIdString(character)}_DNG0002_BASE_SuddenStop",
            DungeonAnim.DngBaseBreath => $"A_PC{GetCharIdString(character)}_DNG0003_BASE_Breath",
            DungeonAnim.DngBaseRun => $"A_PC{GetCharIdString(character)}_DNG0006_BASE_Run",
            DungeonAnim.DngBaseWalk => $"A_PC{GetCharIdString(character)}_DNG0008_BASE_Walk",
            DungeonAnim.DngBasePActionA => $"A_PC{GetCharIdString(character)}_DNG0031_BASE_PActionA",
            DungeonAnim.DngBasePActionB => $"A_PC{GetCharIdString(character)}_DNG0032_BASE_PActionB",
            DungeonAnim.DngBaseDashRun => $"A_PC{GetCharIdString(character)}_DNG0101_BASE_DashRun",
            DungeonAnim.DngBaseLocoStopL => $"A_PC{GetCharIdString(character)}_DNG0103_BASE_LocomotionStopL",
            DungeonAnim.DngBaseTalk => $"A_PC{GetCharIdString(character)}_DNG0113_BASE_Talk",
            DungeonAnim.BSDngAlsNWalkRun => $"BS_PC{GetCharIdString(character)}_DNG_ALS_N_WalkRun",
            _ => throw new NotImplementedException(),
        };

    private static string GetCharIdString(Character character)
        => ((int)character).ToString("0000");
}

public enum DungeonAnim
{
    DngAlsNPose,
    DngAlsNRunF,
    DngAlsNWalkF,
    DngAlsPoseRunStrideZero,
    DngAlsStopPose,
    DngPoseNeutral,
    DngBaseSuddenStop,
    DngBaseBreath,
    DngBaseRun,
    DngBaseWalk,
    DngBasePActionA,
    DngBasePActionB,
    DngBaseDashRun,
    DngBaseLocoStopL,
    DngBaseTalk,
    BSDngAlsNWalkRun, // BlendSpace?
}