using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks.Models;

internal class FemcCostume : Costume
{
    public FemcCostume()
    {
        this.Character = Character.Player;
        this.Config.Base.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_BaseSkelton";
        this.Config.Base.AnimPath = AssetUtils.GetAssetFile(Character.Player, 51, CostumeAssetType.BaseAnim);
        this.Config.Costume.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_C998";
        this.Config.Hair.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_H999";
        this.Config.Face.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_F999";
        this.Config.Costume.AnimPath = "None";
        this.Config.Hair.AnimPath = "None";
        this.Config.Face.AnimPath = "None";

        this.Config.Allout.NormalPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutNormal);
        this.Config.Allout.NormalMaskPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutNormalMask);
        this.Config.Allout.SpecialPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutSpecial);
        this.Config.Allout.SpecialMaskPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutSpecialMask);
        this.Config.Allout.TextPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutText);
        this.Config.Allout.PlgPath = AssetUtils.GetAssetFile(Character.Player, 0, CostumeAssetType.AlloutPlg);
    }
}
