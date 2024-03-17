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

    public CostumePartsData Base { get; } = new();

    public CostumePartsData Costume { get; } = new();

    public CostumePartsData Hair { get; } = new();

    public CostumePartsData Face { get; } = new();
}

internal class CostumePartsData
{
    public string? MeshPath { get; set; }

    public string? AnimPath { get; set; }
}
