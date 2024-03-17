using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Types;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using System.Collections;
using System.Runtime.InteropServices;
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
    private bool isCostumesRandom;

    public CostumeHooks(
        IUObjects uobjects,
        IUnreal unreal,
        CostumeRegistry registry)
    {
        this.uobjects = uobjects;
        this.unreal = unreal;
        this.registry = registry;

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

        if (isCostumesRandom && this.registry.GetRandomCostume(character) is Costume costume)
        {
            comp->mSetCostumeID = costume.CostumeId;
            Log.Debug($"{nameof(SetCostumeId)} || {character} || Costume ID: {costumeId} || Randomized: {costume.Name}");
        }
        else
        {
            Log.Debug($"{nameof(SetCostumeId)} || {character} || Costume ID: {costumeId}");
        }
    }

    private void SetCostumeData(UnrealObject obj)
    {
        var costumeItemList = (UCostumeItemListTable*)obj.Self;

        Log.Debug("Setting costume item data.");

        var newItemIndex = 120;
        foreach (var costume in this.registry.GetActiveCostumes())
        {
            if ((*costumeItemList).Any(x => x.CostumeID == costume.CostumeId && GetCharFromEquip(x.EquipID) == costume.Character))
            {
                continue;
            }

            var newItem = &costumeItemList->Data.AllocatorInstance[newItemIndex];
            newItem->CostumeID = (ushort)costume.CostumeId;
            newItem->EquipID = GetEquipFromChar(costume.Character);
            costume.SetCostumeItemId(newItemIndex);
            this.SetCostumePaths(costume);

            Log.Information($"Added costume: {costume.Name} || Costume ID: {costume.CostumeId}");
            newItemIndex++;
        }
    }

    private void SetCostumePaths(Costume costume)
    {
        if (costume.Config.Costume.MeshPath != null)
        {
            this.SetCostumeFilePath(costume, costume.Config.Costume.MeshPath, CostumeAssetType.Costume);
        }
    }

    /// <summary>
    /// Assigns the expected FName file path (trim .uasset) and file name.
    /// </summary>
    /// <param name="filePath"></param>
    private void SetCostumeFilePath(Costume costume, string filePath, CostumeAssetType type)
    {
        var newAssetPaths = new AssetFNames(filePath);
        var originalAssetPaths = new AssetFNames(GetAssetPath(costume, type));

        this.unreal.AssignFName(Mod.NAME, originalAssetPaths.AssetPath, newAssetPaths.AssetPath);
        this.unreal.AssignFName(Mod.NAME, originalAssetPaths.AssetName, newAssetPaths.AssetName);
    }

    private record AssetFNames(string AssetFilePath)
    {
        public string AssetPath { get; } = AssetFilePath.Replace(".uasset", string.Empty);

        public string AssetName { get; } = Path.GetFileNameWithoutExtension(AssetFilePath);
    };

    private static string GetAssetPath(Costume costume, CostumeAssetType type)
        => type switch
        {
            CostumeAssetType.Base => $"/Game/Xrd777/Characters/Player/PC{costume.Character:0000}/Models/SK_PC{costume.Character:0000}_BaseSkeleton.uasset",
            CostumeAssetType.Costume => $"/Game/Xrd777/Characters/Player/PC{costume.Character:0000}/Models/SK_PC{costume.Character:0000}_C{costume.CostumeId:000}.uasset",
            CostumeAssetType.Hair => $"/Game/Xrd777/Characters/Player/PC{costume.Character:0000}/Models/SK_PC{costume.Character:0000}_H{costume.CostumeId:000}.uasset",
            CostumeAssetType.Face => $"/Game/Xrd777/Characters/Player/PC{costume.Character:0000}/Models/SK_PC{costume.Character:0000}_F{costume.CostumeId:000}.uasset",
            _ => throw new Exception()
        };

    private enum CostumeAssetType
    {
        Base,
        Costume,
        Hair,
        Face,
    }

    private static Character GetCharFromEquip(EquipFlag flag)
        => Enum.Parse<Character>(flag.ToString());
    private static EquipFlag GetEquipFromChar(Character character)
        => Enum.Parse<EquipFlag>(character.ToString());
}


[StructLayout(LayoutKind.Explicit, Size = 0x40)]
public unsafe struct UCostumeItemListTable : IEnumerable<FCostumeItemList>
{
    //[FieldOffset(0x0000)] public UAppDataAsset baseObj;
    [FieldOffset(0x0030)] public TArray<FCostumeItemList> Data;

    public readonly IEnumerator<FCostumeItemList> GetEnumerator() => new TArrayWrapper<FCostumeItemList>(Data).GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}


[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public unsafe struct FCostumeItemList
{
    [FieldOffset(0x0000)] public FString ItemDef;
    [FieldOffset(0x0010)] public ushort SortNum;
    [FieldOffset(0x0014)] public uint ItemType;
    [FieldOffset(0x0018)] public EquipFlag EquipID;
    [FieldOffset(0x001C)] public uint Price;
    [FieldOffset(0x0020)] public uint SellPrice;
    [FieldOffset(0x0024)] public ushort GetFLG;
    [FieldOffset(0x0028)] public uint ReflectType;
    [FieldOffset(0x002C)] public ushort CostumeID;
}

[Flags]
public enum EquipFlag
{
    NONE = 0,
    Player = 1 << 1,
    Yukari = 1 << 2,
    Stupei = 1 << 3,
    Akihiko = 1 << 4,
    Mitsuru = 1 << 5,
    Fuuka = 1 << 6,
    Aigis = 1 << 7,
    Ken = 1 << 8,
    Koromaru = 1 << 9,
    Shinjiro = 1 << 10,
}
