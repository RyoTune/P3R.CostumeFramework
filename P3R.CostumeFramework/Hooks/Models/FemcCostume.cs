using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks.Models;

internal class FemcCostume : CostumeConfig
{
    public FemcCostume()
    {
        this.Base.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_BaseSkelton";
        this.Base.AnimPath = AssetUtils.GetAssetFile(Character.Player, 51, CostumeAssetType.BaseAnim);
        this.Costume.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_C998";
        this.Hair.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_H999";
        this.Face.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_F999";

        this.Allout.NormalPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutNormal);
        this.Allout.NormalMaskPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutNormalMask);
        this.Allout.SpecialPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutSpecial);
        this.Allout.SpecialMaskPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutSpecialMask);
        this.Allout.TextPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutText);
        this.Allout.PlgPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutPlg);
    }
}
