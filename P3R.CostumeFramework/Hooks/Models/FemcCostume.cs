using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks.Models;

internal class FemcCostume : CostumeConfig
{
    public FemcCostume()
    {
        this.Base.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_BaseSkelton";
        this.Base.AnimPath = AssetUtils.GetAssetFile(Character.Player, 51, CostumeAssetType.Base_Anim);
        this.Costume.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_C998";
        this.Hair.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_H999";
        this.Face.MeshPath = "/Game/Xrd777/Characters/Player/PC0002/Models/SK_PC0002_F999";
    }
}
