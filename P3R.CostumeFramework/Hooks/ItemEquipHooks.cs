using P3R.CostumeFramework.Costumes;

namespace P3R.CostumeFramework.Hooks;

internal unsafe class ItemEquipHooks
{
    private delegate nint GetGlobalWork();
    private GetGlobalWork? getGlobalWork;

    public ItemEquipHooks()
    {
        ScanHooks.Add(
            nameof(GetGlobalWork),
            "48 89 5C 24 ?? 57 48 83 EC 20 48 8B 0D ?? ?? ?? ?? 33 DB",
            (hooks, result) => this.getGlobalWork = hooks.CreateWrapper<GetGlobalWork>(result, out _));
    }

    public nint GetCharWork(Character character)
        => this.getGlobalWork!() + 0x1b0 + ((nint)character * 0x2b4);

    public int GetEquip(Character character, Equip equip)
        => *(ushort*)(this.GetCharWork(character) + 0x28c + ((nint)equip * 2));
}

public enum Equip
    : ushort
{
    Weapon,
    Armor,
    Footwear,
    Accessory,
    Outfit,
}
