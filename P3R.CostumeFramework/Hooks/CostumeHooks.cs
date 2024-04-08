using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using P3R.CostumeFramework.Hooks.Services;
using P3R.CostumeFramework.Types;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace P3R.CostumeFramework.Hooks;

internal unsafe class CostumeHooks
{
    [Function(CallingConventions.Microsoft)]
    private delegate void UAppCharacterComp_Update(UAppCharacterComp* comp);
    private UAppCharacterComp_Update? characterCompUpdate;

    [Function(Register.rcx, Register.rax, true)]
    private delegate void SetCostumeId(UAppCharacterComp* comp);
    private IReverseWrapper<SetCostumeId>? setCostumeWrapper;
    private IAsmHook? setCostumeHook;

    private readonly IUnreal unreal;
    private readonly IUObjects uobjects;
    private readonly CostumeRegistry registry;
    private readonly CostumeDescService costumeDesc;
    private readonly CostumeShellService costumeShells;
    private readonly CostumeMusicService costumeMusic;
    private readonly Dictionary<Character, DefaultCostume> defaultCostumes = new();

    private bool isCostumesRandom;

    public CostumeHooks(
        IUObjects uobjects,
        IUnreal unreal,
        CostumeRegistry registry,
        CostumeDescService costumeDesc,
        CostumeMusicService costumeMusic)
    {
        this.uobjects = uobjects;
        this.unreal = unreal;
        this.registry = registry;
        this.costumeDesc = costumeDesc;
        this.costumeMusic = costumeMusic;
        this.costumeShells = new(unreal);
        
        foreach (var character in Enum.GetValues<Character>())
        {
            if (character > Character.Shinjiro) break;
            this.defaultCostumes[character] = new(character);
        }

        this.uobjects.FindObject("DatItemCostumeDataAsset", this.SetCostumeData);

        ScanHooks.Add(
            nameof(UAppCharacterComp_Update),
            "48 8B C4 48 89 48 ?? 55 41 54 48 8D 68 ?? 48 81 EC 48 01 00 00",
            (hooks, result) =>
            {
                this.characterCompUpdate = hooks.CreateWrapper<UAppCharacterComp_Update>(result, out _);

                var setCostumeAddress = result + 0x255;
                var setCostumePatch = new string[]
                {
                    "use64",
                    Utilities.PushCallerRegisters,
                    hooks.Utilities.GetAbsoluteCallMnemonics(this.SetCostumeIdImpl, out this.setCostumeWrapper),
                    Utilities.PopCallerRegisters,
                    "mov rax, qword [rcx]",
                };

                this.setCostumeHook = hooks.CreateAsmHook(setCostumePatch, setCostumeAddress).Activate();
            });
    }

    public void SetRandomizeCostumes(bool isCostumesRandom) => this.isCostumesRandom = isCostumesRandom;

    private void SetCostumeIdImpl(UAppCharacterComp* comp)
    {
        var character = comp->baseObj.Character;
        var costumeId = comp->mSetCostumeID;

        // Ignore non-player characters.
        if (character < Character.Player || character > Character.Shinjiro)
        {
            return;
        }

        // Apply any costume overrides.
        if (this.registry.GetCostumeOverride(character, costumeId) is Costume overrideCostume)
        {
            costumeId = overrideCostume.CostumeId;
            Log.Debug($"{nameof(SetCostumeId)} || {character} || Costume ID: {costumeId} || Override: {overrideCostume.Name}");
        }

        // Apply randomized costumes.
        if ((isCostumesRandom || costumeId == GameCostumes.RANDOMIZED_COSTUME_ID)
            && this.registry.GetRandomCostume(character) is Costume randomCostume)
        {
            costumeId = randomCostume.CostumeId;
            Log.Debug($"{nameof(SetCostumeId)} || {character} || Costume ID: {costumeId} || Randomized: {randomCostume.Name}");
        }

        // Update before costume ID is set to shell costume.
        this.costumeMusic.Refresh(character, costumeId);

        comp->mSetCostumeID = costumeId;
        Log.Debug($"{nameof(SetCostumeId)} || {character} || Costume ID: {costumeId}");
    }

    private void SetCostumeData(UnrealObject obj)
    {
        var costumeItemList = (UCostumeItemListTable*)obj.Self;

        Log.Debug("Setting costume item data.");
        var activeCostumes = this.registry.GetActiveCostumes();

        for (int i = 0; i < costumeItemList->Count; i++)
        {
            var costumeItem = (*costumeItemList)[i];
            var existingCostume = activeCostumes.FirstOrDefault(x => x.CostumeId == costumeItem.CostumeID && x.Character == AssetUtils.GetCharFromEquip(costumeItem.EquipID));
            if (existingCostume != null)
            {
                existingCostume.SetCostumeItemId(i);
                continue;
            }
        }

        var newItemIndex = 120;
        foreach (var costume in this.registry.GetActiveCostumes())
        {
            var newItem = &costumeItemList->Data.AllocatorInstance[newItemIndex];
            newItem->CostumeID = (ushort)costume.CostumeId;
            newItem->EquipID = AssetUtils.GetEquipFromChar(costume.Character);
            costume.SetCostumeItemId(newItemIndex);
            this.costumeDesc.SetCostumeDesc(newItemIndex, costume.Description);

            if (costume.CostumeId >= GameCostumes.BASE_MOD_COSTUME_ID)
            {
                this.SetCostumePaths(costume);
            }

            Log.Debug($"Added costume item: {costume.Name} || Costume Item ID: {newItemIndex} || Costume ID: {costume.CostumeId}");
            newItemIndex++;
        }

        this.costumeDesc.Init();
    }

    private void RedirectToCharAsset(string assetFile, Character redirectChar)
    {
        var fnames = new AssetFNames(assetFile);
        this.unreal.AssignFName(Mod.NAME, fnames.AssetName, fnames.AssetName.Replace("0001", $"{(int)redirectChar:0000}"));
        this.unreal.AssignFName(Mod.NAME, fnames.AssetPath, fnames.AssetPath.Replace("0001", $"{(int)redirectChar:0000}"));
    }

    private void SetCostumePaths(Costume costume)
    {
        foreach (var assetType in Enum.GetValues<CostumeAssetType>())
        {
            this.SetCostumeFile(costume, assetType);
        }
    }

    private void SetCostumeFile(Costume costume, CostumeAssetType assetType)
    {
        var ogAssetFile = AssetUtils.GetAssetFile(costume.Character, costume.CostumeId, assetType);
        var currentAssetFile = costume.Config.GetAssetFile(assetType) ?? this.GetDefaultAsset(costume.Character, assetType);

        if (ogAssetFile == null)
        {
            Log.Debug($"Asset has no original: {assetType} || Costume: {costume.Name}");
            return;
        }

        if (currentAssetFile == null)
        {
            Log.Debug($"Asset has no default or new: {assetType} || Costume: {costume.Name}");
            return;
        }

        if (ogAssetFile == currentAssetFile)
        {
            return;
        }

        var ogAssetFNames = new AssetFNames(ogAssetFile);
        var newAssetFNames = new AssetFNames(currentAssetFile);

        this.unreal.AssignFName(Mod.NAME, ogAssetFNames.AssetPath, newAssetFNames.AssetPath);
        this.unreal.AssignFName(Mod.NAME, ogAssetFNames.AssetName, newAssetFNames.AssetName);
    }

    private string? GetDefaultAsset(Character character, CostumeAssetType assetType)
        => this.defaultCostumes[character].GetAssetFile(assetType);

    private record AssetFNames(string AssetFile)
    {
        public string AssetName { get; } = Path.GetFileNameWithoutExtension(AssetFile);

        public string AssetPath { get; } = AssetUtils.GetAssetPath(AssetFile);
    };
}
