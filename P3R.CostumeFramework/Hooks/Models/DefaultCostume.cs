using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks.Models;

internal class DefaultCostume : CostumeConfig
{
    public DefaultCostume(Character character)
    {
        this.Base.MeshPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.Base_Mesh);
        this.Base.AnimPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.Base_Anim);
        this.Costume.MeshPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.Costume_Mesh);
        //this.Costume.AnimPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.Costume_Anim);
        this.Hair.MeshPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.Hair_Mesh);
        //this.Hair.AnimPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.Hair_Anim);
        this.Face.MeshPath = AssetUtils.GetAssetFile(character, 0, CostumeAssetType.Face_Mesh);
        //this.Face.AnimPath = AssetUtils.GetAssetFile(character, 51, CostumeAssetType.Face_Anim);
    }
}
