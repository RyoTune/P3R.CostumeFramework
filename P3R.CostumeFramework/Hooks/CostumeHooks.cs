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

        if ((isCostumesRandom || costumeId == GameCostumes.RANDOMIZED_COSTUME_ID) && this.registry.GetRandomCostume(character) is Costume costume)
        {
            costumeId = costume.CostumeId;
            Log.Debug($"{nameof(SetCostumeId)} || {character} || Costume ID: {costumeId} || Randomized: {costume.Name}");
        }

        if (costumeId >= 1000)
        {
            if (this.registry.TryGetCostume(character, costumeId, out var redirectCostume)
                && redirectCostume.Config.Costume.MeshPath != null)
            {
                var costumeAssetPath = AssetUtils.GetAssetPath(redirectCostume.Config.Costume.MeshPath);
                if (costumeAssetPath.Length > 74)
                {
                    Log.Warning($"Costume asset paths longer than 74 characters is currently unsupported.\nAsset: {costumeAssetPath}");
                    Log.Warning("Alternatively, use a shorter folder name and set the costume name in the \"config.yaml\".");
                    return;
                }

                // Redirect to shell costume.
                var currentShell = this.costumeShells.GetShellCostume(character, costumeId);
                costumeId = currentShell.CostumeId;

                var shellFStrings = this.costumeShells.GetShellFStrings(currentShell);
                shellFStrings.CostumeMesh->SetString(costumeAssetPath);
                //shellFStrings.HairMesh->SetString(AssetUtils.GetAssetPath(redirectCostume.Config.Hair.MeshPath));
                //shellFStrings.FaceMesh->SetString(AssetUtils.GetAssetPath(redirectCostume.Config.Face.MeshPath));

                Log.Debug($"Replacing: {currentShell.CostumeMeshPath}");
                Log.Debug($"With: {costumeAssetPath}");
            }
        }

        comp->mSetCostumeID = costumeId;
        this.costumeMusic.Refresh(character, costumeId);
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

            Log.Debug($"Added costume item: {costume.Name} || Costume Item ID: {newItemIndex} || Costume ID: {costume.CostumeId}");
            newItemIndex++;
        }

        this.costumeDesc.Init();
    }
}
