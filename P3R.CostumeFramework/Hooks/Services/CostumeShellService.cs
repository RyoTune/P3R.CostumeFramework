using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Costumes;
using P3R.CostumeFramework.Hooks.Costumes.Models;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeShellService
{
    private const int SHELL_COSTUME_ID = 51;
    private readonly Dictionary<Character, int> prevCostumeIds = [];
    private readonly Dictionary<Character, Costume> defaultCostumes = [];

    private readonly CostumeTableService costumeTable;
    private readonly CostumeRegistry costumes;

    public CostumeShellService(CostumeRegistry costumes, CostumeTableService costumeTable)
    {
        this.costumes = costumes;
        this.costumeTable = costumeTable;

        foreach (var character in Characters.PC)
        {
            if (character == Character.AigisReal)
            {
                this.defaultCostumes[character] = new DefaultCostume(Character.Aigis);
            }
            else
            {
                this.defaultCostumes[character] = new DefaultCostume(character);
            }
        }
    }

    public int UpdateCostume(Character character, int costumeId)
    {
        if (costumeId == SHELL_COSTUME_ID)
        {
            this.prevCostumeIds[character] = costumeId;
            this.costumeTable.SetCostumeData(SHELL_COSTUME_ID, defaultCostumes[character]);
            Log.Debug($"{character}: Reset shell costume data.");
        }

        if (costumeId < GameCostumes.BASE_MOD_COSTUME_ID)
        {
            return costumeId;
        }

        var shouldUpdateData = this.prevCostumeIds[character] != costumeId;
        if (shouldUpdateData && this.costumes.TryGetCostume(character, costumeId, out var costume))
        {
            this.costumeTable.SetCostumeData(SHELL_COSTUME_ID, costume);
            this.prevCostumeIds[character] = costumeId;
        }

        return SHELL_COSTUME_ID;
    }
}
