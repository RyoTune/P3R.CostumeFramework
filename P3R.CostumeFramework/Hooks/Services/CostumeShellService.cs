using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using Unreal.ObjectsEmitter.Interfaces;

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
                if (keywords.Any(x => name.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Debug($"SKEL_Human ({name}): Retarget bone animation to skeleton.");
                    bone->TranslationRetargetingMode = EBoneTranslationRetargetingMode.Skeleton;
                }
            }
        });
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