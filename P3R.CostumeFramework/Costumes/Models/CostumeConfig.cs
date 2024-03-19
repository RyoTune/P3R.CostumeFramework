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
}

internal class CostumePartsData
{
    public string? MeshPath { get; set; }

    public string? AnimPath { get; set; }
}
