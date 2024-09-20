using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using P3R.CostumeFramework.Hooks.Services;
using P3R.CostumeFramework.Utils;
using Reloaded.Hooks.Definitions;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Costumes;

internal unsafe class CostumeTableService
{
    private readonly IUnreal unreal;
    private readonly CostumeRegistry costumes;
    private DefaultCostumes defaultCostumes = new();
    private DataTable<FAppCharTableRow>? table;
    private bool useFemc;
    private IAsmHook? fullDtHook;

    public CostumeTableService(IDataTables dt, IUnreal unreal, CostumeRegistry costumes)
    {
        this.unreal = unreal;
        this.costumes = costumes;

        ScanHooks.Add(
            "Use Full DT_Costume",
            "33 DB 48 8D 4D ?? 48 89 5D ?? 48 89 5D ?? 8D 53 ?? 84 C0 74 ?? E8 ?? ?? ?? ?? 8B 55 ?? 8D 7A",
            (hooks, result) => this.fullDtHook = hooks.CreateAsmHook("use64\nmov rax, 1", result).Activate());

        dt.FindDataTable<FAppCharTableRow>("DT_Costume", table =>
        {
            this.defaultCostumes = new(this.useFemc);
            this.table = table;

            this.UpdateCostumeTable();
        });
    }

    public void SetUseFemc(bool useFemc) => this.useFemc = useFemc;

    private void UpdateCostumeTable()
    {
        if (this.table == null) return;

        foreach (var costume in this.costumes.Costumes)
        {
            if (costume.CostumeId < GameCostumes.BASE_MOD_COSTUME_ID)
            {
                this.UpdateCostume(costume);
            }
        }
    }

    private void UpdateCostume(Costume costume)
    {
        if (this.table == null) return;

        var charRow = this.GetCharacterRow(costume.Character);
        if (charRow == null) return;

        if (charRow.Self->Costumes.TryGet(costume.CostumeId, out var costumeData))
        {
            this.SetCostumeData(costumeData, costume);
        }
        else
        {
            Log.Warning($"Failed to update costume: {costume.Character} || ID: {costume.CostumeId} || {costume.Name}");
        }
    }

    private void SetCostumeData(FAppCharCostumeData* costumeData, Costume costume)
    {
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.BaseMesh), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.BaseMesh, path!));
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.BaseAnim), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.BaseAnim, path!));
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.CostumeMesh), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.CostumeMesh, path!));
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.CostumeAnim), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.CostumeAnim, path!));
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.FaceMesh), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.FaceMesh, path!));
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.FaceAnim), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.FaceAnim, path!));
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.HairMesh), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.HairMesh, path!));
        ModUtils.IfNotNull(costume.Config.GetAssetFile(CostumeAssetType.HairAnim), path => this.SetCostumeAsset(costumeData, costume.Character, CostumeAssetType.HairAnim, path!));
    }

    private void SetCostumeAsset(FAppCharCostumeData* costumeData, Character character, CostumeAssetType assetType, string? newAssetFile)
    {
        var assetFile = newAssetFile ?? this.GetDefaultAsset(character, assetType);
        if (assetFile == null)
        {
            Log.Error($"Costume asset path is null: {character} || {assetType}");
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

        switch (assetType)
        {
            case CostumeAssetType.BaseMesh:
                costumeData->Base.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Base.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.CostumeMesh:
                costumeData->Costume.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Costume.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.HairMesh:
                costumeData->Hair.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Hair.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.FaceMesh:
                costumeData->Face.Mesh.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Face.Mesh.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.BaseAnim:
                costumeData->Base.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Base.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.CostumeAnim:
                costumeData->Costume.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Costume.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.HairAnim:
                costumeData->Hair.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Hair.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            case CostumeAssetType.FaceAnim:
                costumeData->Face.Anim.baseObj.baseObj.ObjectId.AssetPathName = assetFName;
                costumeData->Face.Anim.baseObj.baseObj.WeakPtr = new();
                break;
            default:
                break;
        }
    }

    private Row<FAppCharTableRow>? GetCharacterRow(Character character) => table?.Rows.FirstOrDefault(x => x.Name == $"PC{(int)character}");

    private string? GetDefaultAsset(Character character, CostumeAssetType assetType) => this.defaultCostumes[character].Config.GetAssetFile(assetType);
}
