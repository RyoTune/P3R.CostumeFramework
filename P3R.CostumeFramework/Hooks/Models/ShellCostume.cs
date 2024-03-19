using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks;

public record ShellCostume(Character Character, int CostumeId)
{
    public string CostumeMeshPath { get; } = AssetUtils.GetAssetPath(Character, CostumeId, CostumeAssetType.Costume);
}