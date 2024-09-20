using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Costumes;
using P3R.CostumeFramework.Hooks.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using p3rpc.classconstructor.Interfaces;
using Unreal.ObjectsEmitter.Interfaces;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeShellService
{
    private const int SHELL_COSTUME_ID = 51;
    private readonly Dictionary<Character, nint> shellDataPtrs = [];
    private readonly Dictionary<Character, int> prevCostumeIds = [];
    private readonly Dictionary<Character, Costume> defaultCostumes = [];
    private readonly Dictionary<Character, nint> charRows = [];

    private readonly IUnreal unreal;
    private readonly CostumeTableService costumeTable;
    private readonly CostumeRegistry costumes;

    public CostumeShellService(IDataTables dt, IUObjects uobjs, IUnreal unreal, CostumeRegistry costumes, IObjectMethods objMethods, CostumeTableService costumeTable)
    {
        this.unreal = unreal;
        this.costumes = costumes;
        this.costumeTable = costumeTable;

        dt.FindDataTable<FAppCharTableRow>("DT_Costume", table =>
        {
            foreach (var character in Characters.PC)
            {
                var charRowName = $"PC{(int)character}";
                var charRowObj = table.Rows.FirstOrDefault(x => x.Name == charRowName);
                if (charRowObj == null)
                {
                    Log.Debug($"Character row not found: {charRowName}");
                    continue;
                }

                var charRow = charRowObj.Self;
                var charCostumes = charRow->Costumes;
                if (charCostumes.TryGet(SHELL_COSTUME_ID, out var charCostume))
                {
                    this.shellDataPtrs[character] = (nint)charCostume;
                    this.prevCostumeIds[character] = -1;

                    if (character == Character.AigisReal)
                    {
                        this.shellDataPtrs[Character.Aigis] = (nint)charCostume;
                        this.prevCostumeIds[Character.Aigis] = -1;
                    }
                }
                else
                {
                    Log.Error($"{character} missing shell Costume ID: {SHELL_COSTUME_ID}");
                }

                this.defaultCostumes[character] = new DefaultCostume(character);
                this.charRows[character] = (nint)charRow;
            }
        });
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
