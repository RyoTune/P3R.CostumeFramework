using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Interfaces;
using P3R.CostumeFramework.Utils;
using System.Diagnostics.CodeAnalysis;

namespace P3R.CostumeFramework.Costumes;

internal class CostumeOverridesRegistry : ICostumeApi
{
    private readonly List<CostumeOverride> costumeOverrides = [];

    public bool TryGetCostumeOverride(Character character, int originalCostumeId, [NotNullWhen(true)]out CostumeOverride? costumeOverride)
    {
        costumeOverride = this.costumeOverrides.FirstOrDefault(x => x.Character == character && x.OriginalCostumeId == originalCostumeId);
        return costumeOverride != null;
    }

    public void AddOverridesFile(string file)
    {
        try
        {
            var overrides = YamlSerializer.DeserializeFile<CostumeOverrideSerialized[]>(file);
            foreach (var item in overrides)
            {
                var character = Enum.Parse<Character>(item.Character, true);
                this.costumeOverrides.Add(new()
                {
                    Character = character,
                    OriginalCostumeId = item.OriginalCostumeId,
                    NewCostumeName = item.NewCostumeName,
                });

                Log.Information($"Costume override: {character} || Costume ID: {item.OriginalCostumeId} || New: {item.NewCostumeName}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to load overrides file.\nFile: {file}");
        }
    }
}
