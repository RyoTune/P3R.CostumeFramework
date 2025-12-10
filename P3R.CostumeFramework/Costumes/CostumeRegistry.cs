using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Types;
using Ryo.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace P3R.CostumeFramework.Costumes;

internal class CostumeRegistry
{
    private readonly CostumeFactory costumeFactory;
    private readonly bool useFemcPlayer;

    public CostumeRegistry(IRyoApi ryo, CostumeFilter filter, bool useExtendedOutfits, bool useFemcPlayer)
    {
        this.useFemcPlayer = useFemcPlayer;
        this.Costumes = new(filter, useExtendedOutfits);
        this.costumeFactory = new(ryo, this.Costumes);
    }

    public GameCostumes Costumes { get; }

    public Costume[] GetActiveCostumes()
        => this.Costumes.Where(this.IsActiveCostume).ToArray();

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
        costume = this.FindCostume(character, costumeId, allowInactive: false)
            ?? (character == Character.Aigis ? this.FindCostume(Character.AigisReal, costumeId, allowInactive: false) : null);

        // fall back to inactive costumes, surely this will cause no issues
        costume ??= this.FindCostume(character, costumeId, allowInactive: true)
            ?? (character == Character.Aigis ? this.FindCostume(Character.AigisReal, costumeId, allowInactive: true) : null);

        return costume != null;
    }

    public bool TryGetCostumeByItemId(int itemId, [NotNullWhen(true)] out Costume? costume)
    {
        var costumeItemId = Costume.GetCostumeItemId(itemId);
        costume = this.Costumes.FirstOrDefault(x => x.CostumeItemId == costumeItemId && this.IsActiveCostume(x));
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
            foreach (var costumeDir in Directory.EnumerateDirectories(characterDir))
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
        }
    }

    private Costume? FindCostume(Character character, int costumeId, bool allowInactive)
        => this.Costumes.FirstOrDefault(x => this.IsRequestedCostume(x, character, costumeId, allowInactive));

    private bool IsRequestedCostume(Costume costume, Character character, int costumeId, bool allowInactive)
    {
        if (costume.Character == character
            && costume.CostumeId == costumeId
            && (allowInactive || this.IsActiveCostume(costume)))
        {
            return true;
        }

        return false;
    }

    private bool IsActiveCostume(Costume costume)
        => costume.IsEnabled
        && costume.Character != Character.NONE
        && this.IsValidForPlayerType(costume);

    private bool IsValidForPlayerType(Costume costume)
        => costume.Config.PlayerType switch
        {
            PlayerType.Makoto => !this.useFemcPlayer,
            PlayerType.Femc => this.useFemcPlayer,
            _ => true,
        };
}
