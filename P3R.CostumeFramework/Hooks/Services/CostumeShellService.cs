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

    private readonly IUnreal unreal;
    private readonly CostumeRegistry costumes;
    private bool useFemc;

    public CostumeShellService(IDataTables dt, IUnreal unreal, CostumeRegistry costumes)
    {
        this.unreal = unreal;
        this.costumes = costumes;

        // FEMC defaults for costumes.
        this.defaultCostumes[Character.FEMC] = new FemcCostume();

        dt.FindDataTable("DT_Costume", table =>
        {
            foreach (var character in Characters.PC)
            {
                var charRowName = $"PC{(int)character}";
                var charRow = (FAppCharTableRow*)table.Rows.First(x => x.Name == charRowName).Self;

                var costumes = charRow->Costumes;
                if (costumes.TryGet(SHELL_COSTUME_ID, out var costume))
                {
                    this.shellDataPtrs[character] = (nint)costume;
                    this.prevCostumeIds[character] = -1;
                }
                else
                {
                    Log.Error($"{character} missing shell Costume ID: {SHELL_COSTUME_ID}");
                }

                this.defaultCostumes[character] = new DefaultCostume(character);
            }

            this.defaultCostumes[Character.FEMC] = new FemcCostume();
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
            this.prevCostumeIds[character] = costumeId;
        }

        return SHELL_COSTUME_ID;
    }

    private void SetCostumeAssets(Costume costume)
    {
        foreach (var assetType in Enum.GetValues<CostumeAssetType>())
        {
            this.SetCostumeAsset(costume, assetType);
        }
    }

    private void SetCostumeAsset(Costume costume, CostumeAssetType assetType)
    {
        var assetFile = costume.Config.GetAssetFile(assetType) ?? this.GetDefaultAsset(costume.Character, assetType);
        if (assetFile == null)
        {
            Log.Error($"Costume asset path is null.\nCostume ({costume.Character}): {costume.Name} || Asset: {assetType}");
            return;
        }

        var assetFName = *this.unreal.FName(AssetUtils.GetUnrealAssetPath(assetFile));
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

    private string? GetDefaultAsset(Character character, CostumeAssetType assetType)
    {
        if (character == Character.Player && this.useFemc)
        {
            return this.defaultCostumes[Character.FEMC].Config.GetAssetFile(assetType);
        }

        return this.defaultCostumes[character].Config.GetAssetFile(assetType);
    }
}
