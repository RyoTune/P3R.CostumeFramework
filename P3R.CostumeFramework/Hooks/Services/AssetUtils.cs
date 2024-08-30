using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;

namespace P3R.CostumeFramework.Hooks.Services;

internal static class AssetUtils
{
    /// <summary>
    /// Gets the expected asset file for the given character's costume ID and asset type.
    /// </summary>
    /// <param name="character">Character.</param>
    /// <param name="costumeId">Costume ID.</param>
    /// <param name="type">Asset type.</param>
    /// <returns></returns>
    public static string? GetAssetFile(Character character, int costumeId, CostumeAssetType type)
    {
        string? assetFile = type switch
        {
            CostumeAssetType.BaseMesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_BaseSkeleton.uasset",
            CostumeAssetType.CostumeMesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_C{costumeId:000}.uasset",
            CostumeAssetType.HairMesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_H{costumeId:000}.uasset",
            CostumeAssetType.FaceMesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_F{costumeId:000}.uasset",

            CostumeAssetType.BaseAnim => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/ABP_PC{GetCharIdString(character)}.uasset",
            CostumeAssetType.CostumeAnim => "/CharacterBase/Human/Blueprints/Animation/ABP_CH_CostumeBase.uasset",
            CostumeAssetType.HairAnim => "/CharacterBase/Human/Blueprints/Animation/ABP_CH_HairBase.uasset",
            CostumeAssetType.FaceAnim => null,
            CostumeAssetType.AlloutNormal => $"/Game/Xrd777/Battle/Allout/Materials/Finish2D/T_Btl_AlloutFinish_Pc{ GetCharIdStringShort(character)}_A1{(costumeId >= 1000 ? $"_{costumeId}" : string.Empty)}",
            CostumeAssetType.AlloutNormalMask => $"/Game/Xrd777/Battle/Allout/Materials/Finish2D/T_Btl_AlloutFinish_Pc{GetCharIdStringShort(character)}_A2{(costumeId >= 1000 ? $"_{costumeId}" : string.Empty)}",
            CostumeAssetType.AlloutSpecial => $"/Game/Xrd777/Battle/Allout/Materials/Finish2D/T_Btl_AlloutFinish_Pc{GetCharIdStringShort(character)}_B1{(costumeId >= 1000 ? $"_{costumeId}" : string.Empty)}",
            CostumeAssetType.AlloutSpecialMask => $"/Game/Xrd777/Battle/Allout/Materials/Finish2D/T_Btl_AlloutFinish_Pc{GetCharIdStringShort(character)}_B2{(costumeId >= 1000 ? $"_{costumeId}" : string.Empty)}",
            CostumeAssetType.AlloutText => $"/Game/Xrd777/Battle/Allout/Materials/Finish2D/T_Btl_AlloutFinishText_Pc{GetCharIdStringShort(character)}{(costumeId >= 1000 ? $"_{costumeId}" : string.Empty)}",
            CostumeAssetType.AlloutPlg => $"{GetCharacterPlg(character)}{(costumeId >= 1000 ? $"_{costumeId}" : string.Empty)}",
            _ => throw new Exception(),
        };

        return assetFile;
    }

    /// <summary>
    /// Gets the expected asset path from asset file path.
    /// Simply removes the .uasset extension and/or adds the game content path.
    /// </summary>
    /// <param name="assetFile">Asset .uasset file path.</param>
    /// <returns>Asset path.</returns>
    public static string GetAssetPath(string assetFile)
    {
        var adjustedPath = assetFile.Replace('\\', '/').Replace(".uasset", string.Empty);
        if (!adjustedPath.StartsWith("/Game/"))
        {
            adjustedPath = $"/Game/{adjustedPath}";
        }

        return adjustedPath;
    }

    public static string? GetAssetPath(Character character, int costumeId, CostumeAssetType type)
    {
        var assetFile = GetAssetFile(character, costumeId, type);
        return assetFile != null ? GetAssetPath(assetFile) : null;
    }

    public static Character GetCharFromEquip(EquipFlag flag)
        => Enum.Parse<Character>(flag.ToString());

    public static EquipFlag GetEquipFromChar(Character character)
        => Enum.Parse<EquipFlag>(character.ToString());

    public static string GetCharIdString(Character character)
        => ((int)character).ToString("0000");

    public static string GetCharIdStringShort(Character character)
        => ((int)character).ToString("00");

    private static string GetCharacterPlg(Character character)
        => character switch
        {
            Character.Player & Character.FEMC => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Hero",
            Character.Yukari => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Yukari",
            Character.Stupei => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Junpei",
            Character.Akihiko => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Sanada",
            Character.Mitsuru => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Mituru",
            Character.Fuuka => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout",
            Character.Aigis => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Aegis",
            Character.Ken => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Amada",
            Character.Koromaru => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Koromaru",
            Character.Shinjiro => "/Game/Xrd777/Battle/Allout/Materials/Finish2D/PLG_UI_Battle_Allout_Last_Aragaki",
            _ => throw new NotImplementedException(),
        };
}
