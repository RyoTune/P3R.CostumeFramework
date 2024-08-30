namespace P3R.CostumeFramework.Costumes.Models;

internal class CostumeConfig
{
    /// <summary>
    /// Overrides costume name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Should costume be used as a default when needed.
    /// </summary>
    public bool IsDefault { get; set; }

    public CostumePartsData Base { get; set; } = new();

    public CostumePartsData Costume { get; set; } = new();

    public CostumePartsData Hair { get; set; } = new();

    public CostumePartsData Face { get; set; } = new();

    public CostumeAllout Allout { get; set; } = new();

    public string? GetAssetFile(CostumeAssetType assetType)
        => assetType switch
        {
            CostumeAssetType.BaseMesh => this.Base.MeshPath,
            CostumeAssetType.BaseAnim => this.Base.AnimPath,
            CostumeAssetType.CostumeMesh => this.Costume.MeshPath,
            CostumeAssetType.CostumeAnim => this.Costume.AnimPath,
            CostumeAssetType.FaceMesh => this.Face.MeshPath,
            CostumeAssetType.FaceAnim => this.Face.AnimPath,
            CostumeAssetType.HairMesh => this.Hair.MeshPath,
            CostumeAssetType.HairAnim => this.Hair.AnimPath,
            CostumeAssetType.AlloutNormal => this.Allout.NormalPath,
            CostumeAssetType.AlloutNormalMask => this.Allout.NormalMaskPath,
            CostumeAssetType.AlloutSpecial => this.Allout.SpecialPath,
            CostumeAssetType.AlloutSpecialMask => this.Allout.SpecialMaskPath,
            CostumeAssetType.AlloutText => this.Allout.TextPath,
            CostumeAssetType.AlloutPlg => this.Allout.PlgPath,
            _ => throw new Exception("Unknown asset type."),
        };
}

internal class CostumePartsData
{
    public string? MeshPath { get; set; }

    public string? AnimPath { get; set; }
}

internal class CostumeAllout
{
    public string? NormalPath { get; set; }

    public string? NormalMaskPath { get; set; }

    public string? SpecialPath { get; set; }

    public string? SpecialMaskPath { get; set; }

    public string? TextPath { get; set; }

    public string? PlgPath { get; set; }
}