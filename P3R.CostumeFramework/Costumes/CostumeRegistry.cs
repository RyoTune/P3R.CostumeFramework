using P3R.CostumeFramework.Costumes.Models;
using System.Diagnostics.CodeAnalysis;

namespace P3R.CostumeFramework.Costumes;

internal class CostumeRegistry
{
    private readonly CostumeFactory costumeFactory;

    public CostumeRegistry()
    {
        this.costumeFactory = new(this.Costumes);
    }

    public GameCostumes Costumes { get; } = new();

    public Costume[] GetActiveCostumes()
        => this.Costumes.Where(x => x.IsEnabled && x.Character != Character.NONE).ToArray();

    public Costume? GetRandomCostume(Character character)
    {
        var costumes = this.Costumes.Where(x => x.Character == character).ToArray();
        if (costumes.Length < 1)
        {
            return null;
        }

        return costumes[Random.Shared.Next(0, costumes.Length)];
    }

    public bool TryGetCostume(Character character, int costumeId, [NotNullWhen(true)] out Costume? costume)
    {
        costume = this.Costumes.FirstOrDefault(x => IsValidCostume(x, character, costumeId));
        if (costume != null)
        {
            return true;
        }

        return false;
    }

    public void RegisterMod(string modId, string modDir)
    {
        var mod = new CostumeMod(modId, modDir);
        if (!Directory.Exists(mod.CostumesDir))
        {
            return;
        }

        foreach (var character in Enum.GetValues<Character>())
        {
            var characterDir = Path.Join(mod.CostumesDir, character.ToString());
            if (!Directory.Exists(characterDir))
            {
                continue;
            }

            // Build costumes from folders.
            foreach (var costumeDir in Directory.EnumerateDirectories(characterDir))
            {
                this.costumeFactory.Create(mod, costumeDir, character);
            }

            // Add costume files for existing costumes.
            //foreach (var costume in this.CostumesList.Where(x => x.Character == character && x.Name != null))
            //{
            //    this.costumeFactory.AddCostumeFiles(costume, costumesDir, modId);
            //}

            // Build new costumes from GMD files.
            //foreach (var file in Directory.EnumerateFiles(characterDir, "*.gmd", SearchOption.TopDirectoryOnly))
            //{
            //    this.costumeFactory.Create(modId, costumesDir, character, file);
            //}
        }
    }

    private static bool IsValidCostume(Costume costume,  Character character, int costumeId)
    {
        if (costume.Character == character
            && costume.CostumeId == costumeId
            && costume.IsEnabled)
        {
            return true;
        }

        return false;
    }
}
