using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Types;
using System.Diagnostics.CodeAnalysis;

namespace P3R.CostumeFramework.Costumes;

internal class CostumeRegistry
{
    private readonly CostumeFactory costumeFactory;

    public CostumeRegistry(CostumeFilter filter)
    {
        this.Costumes = new(filter);
        this.costumeFactory = new(this.Costumes);
    }

    public GameCostumes Costumes { get; }

    public Costume[] GetActiveCostumes()
        => this.Costumes.Where(IsActiveCostume).ToArray();

    public Costume? GetRandomCostume(Character character)
    {
        var costumes = this.GetActiveCostumes().Where(x => x.Character == character).ToArray();
        if (costumes.Length < 1)
        {
            return null;
        }

        return costumes[Random.Shared.Next(0, costumes.Length)];
    }

    public bool TryGetCostume(Character character, int costumeId, [NotNullWhen(true)] out Costume? costume)
    {
        costume = this.Costumes.FirstOrDefault(x => IsRequestedCostume(x, character, costumeId));
        if (costume != null)
        {
            return true;
        }

        return false;
    }

    public bool TryGetCostumeByItemId(int itemId, [NotNullWhen(true)] out Costume? costume)
    {
        var costumeItemId = Costume.GetCostumeItemId(itemId);
        costume = this.Costumes.FirstOrDefault(x => x.CostumeItemId == costumeItemId && IsActiveCostume(x));
        return costume != null;
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
            var allDirs = Directory.GetDirectories(characterDir);
            var appendCostumeDirs = allDirs.Where(x => Path.GetFileName(x).StartsWith('_'));
            var costumeDirs = allDirs.Except(appendCostumeDirs);

            foreach (var costumeDir in costumeDirs)
            {
                try
                {
                    this.costumeFactory.Create(mod, costumeDir, character);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to create costume from folder.\nFolder: {costumeDir}");
                }
            }

            // Add costume files for existing costumes.
            foreach (var appendDir in appendCostumeDirs)
            {
                var costumeName = Path.GetFileName(appendDir).TrimStart('_');
                var existingCostume = this.Costumes.FirstOrDefault(x => x.Character == character && x.Name == costumeName);
                if (existingCostume != null)
                {
                    CostumeFactory.LoadCostumeFiles(mod, existingCostume, appendDir);
                }
            }
        }
    }

    private static bool IsRequestedCostume(Costume costume, Character character, int costumeId)
    {
        if (costume.Character == character
            && costume.CostumeId == costumeId
            && IsActiveCostume(costume))
        {
            return true;
        }

        return false;
    }

    private static bool IsActiveCostume(Costume costume)
        => costume.IsEnabled
        && costume.Character != Character.NONE;
}
