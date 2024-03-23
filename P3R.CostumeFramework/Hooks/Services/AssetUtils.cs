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
            CostumeAssetType.Base_Mesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_BaseSkeleton.uasset",
            CostumeAssetType.Costume_Mesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_C{costumeId:000}.uasset",
            CostumeAssetType.Hair_Mesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_H{costumeId:000}.uasset",
            CostumeAssetType.Face_Mesh => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_F{costumeId:000}.uasset",

            CostumeAssetType.Base_Anim => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/ABP_PC{GetCharIdString(character)}.uasset",
            CostumeAssetType.Costume_Anim => "/CharacterBase/Human/Blueprints/Animation/ABP_CH_CostumeBase.uasset",
            CostumeAssetType.Hair_Anim => "/CharacterBase/Human/Blueprints/Animation/ABP_CH_HairBase.uasset",
            CostumeAssetType.Face_Anim => null,
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
}
