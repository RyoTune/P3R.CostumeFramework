using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks.Models;

internal class DefaultCostume : CostumeConfig
{
    public DefaultCostume(Character character)
    {
        this.Base.MeshPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.BaseMesh);
        this.Base.AnimPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.BaseAnim);
        this.Costume.MeshPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.CostumeMesh);
        //this.Costume.AnimPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.Costume_Anim);
        this.Hair.MeshPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.HairMesh);
        //this.Hair.AnimPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.Hair_Anim);
        this.Face.MeshPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.FaceMesh);
        //this.Face.AnimPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.Face_Anim);
        this.Allout.NormalPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutNormal);
        this.Allout.NormalMaskPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutNormalMask);
        this.Allout.SpecialPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutSpecial);
        this.Allout.SpecialMaskPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutSpecialMask);
        this.Allout.TextPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutText);
        this.Allout.PlgPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutPlg);
    }
}
