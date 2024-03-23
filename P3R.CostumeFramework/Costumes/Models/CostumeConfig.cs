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

    public string? GetAssetFile(CostumeAssetType assetType)
        => assetType switch
        {
            CostumeAssetType.Base_Mesh => this.Base.MeshPath,
            CostumeAssetType.Base_Anim => this.Base.AnimPath,
            CostumeAssetType.Costume_Mesh => this.Costume.MeshPath,
            CostumeAssetType.Costume_Anim => this.Costume.AnimPath,
            CostumeAssetType.Face_Mesh => this.Face.MeshPath,
            CostumeAssetType.Face_Anim => this.Face.AnimPath,
            CostumeAssetType.Hair_Mesh => this.Hair.MeshPath,
            CostumeAssetType.Hair_Anim => this.Hair.AnimPath,
            _ => throw new Exception("Unknown asset type."),
        };
}

internal class CostumePartsData
{
    public string? MeshPath { get; set; }

    public string? AnimPath { get; set; }
}
