using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Services;

namespace P3R.CostumeFramework.Hooks;

#pragma warning disable CS8601 // Possible null reference assignment.
public record ShellCostume(Character Character, int CostumeId)
{
    public string CostumeMeshPath { get; } = AssetUtils.GetAssetPath(Character, CostumeId, CostumeAssetType.CostumeMesh);

    // Using Costume ID 0 for other meshes.
    public string HairMeshPath { get; } = AssetUtils.GetAssetPath(Character, 0, CostumeAssetType.HairMesh);

    public string FaceMeshPath { get; } = AssetUtils.GetAssetPath(Character, 0, CostumeAssetType.FaceMesh);
}