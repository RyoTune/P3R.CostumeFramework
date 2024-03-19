using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Hooks.Models;

namespace P3R.CostumeFramework.Hooks.Services;

internal static class AssetUtils
{
    /// <summary>
    /// Gets the expected asset path for the given character's costume ID and asset type.
    /// </summary>
    /// <param name="character">Character.</param>
    /// <param name="costumeId">Costume ID.</param>
    /// <param name="type">Asset type.</param>
    /// <returns></returns>
    public static string GetAssetPath(Character character, int costumeId, CostumeAssetType type)
    {
        string assetFile = type switch
        {
            CostumeAssetType.Base => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_BaseSkeleton.uasset",
            CostumeAssetType.Costume => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_C{costumeId:000}.uasset",
            CostumeAssetType.Hair => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_H{costumeId:000}.uasset",
            CostumeAssetType.Face => $"/Game/Xrd777/Characters/Player/PC{GetCharIdString(character)}/Models/SK_PC{GetCharIdString(character)}_F{costumeId:000}.uasset",
            _ => throw new Exception(),
        };

        return GetAssetPath(assetFile);
    }

    /// <summary>
    /// Gets the expected asset path from asset file path.
    /// Simply replaces the .uasset extension with file name again.
    /// </summary>
    /// <param name="assetFile">Asset .uasset file path.</param>
    /// <returns>Asset path.</returns>
    public static string GetAssetPath(string assetFile)
    {
        var adjustedPath = assetFile.Replace('\\', '/').Replace("uasset", Path.GetFileNameWithoutExtension(assetFile), StringComparison.OrdinalIgnoreCase);
        if (!adjustedPath.StartsWith("/Game/"))
        {
            adjustedPath = $"/Game/{adjustedPath}";
        }

        return adjustedPath;
    }

    public static Character GetCharFromEquip(EquipFlag flag)
        => Enum.Parse<Character>(flag.ToString());

    public static EquipFlag GetEquipFromChar(Character character)
        => Enum.Parse<EquipFlag>(character.ToString());

    public static string GetCharIdString(Character character)
        => ((int)character).ToString("0000");
}
