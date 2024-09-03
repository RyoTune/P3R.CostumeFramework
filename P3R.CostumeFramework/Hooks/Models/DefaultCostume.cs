using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks.Models;

internal class DefaultCostume : Costume
{
    public DefaultCostume(Character character)
    {
        this.Character = character;
        this.Config.Base.MeshPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.BaseMesh);
        this.Config.Base.AnimPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.BaseAnim);
        this.Config.Costume.MeshPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.CostumeMesh);
        this.Config.Hair.MeshPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.HairMesh);
        this.Config.Face.MeshPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.FaceMesh);
        this.Config.Costume.AnimPath = "None";
        this.Config.Hair.AnimPath = "None";
        this.Config.Face.AnimPath = "None";

        this.Config.Allout.NormalPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutNormal);
        this.Config.Allout.NormalMaskPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutNormalMask);
        this.Config.Allout.SpecialPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutSpecial);
        this.Config.Allout.SpecialMaskPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutSpecialMask);
        this.Config.Allout.TextPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutText);
        this.Config.Allout.PlgPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.AlloutPlg);
    }
}
