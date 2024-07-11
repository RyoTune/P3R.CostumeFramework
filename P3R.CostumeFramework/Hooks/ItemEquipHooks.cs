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

    private nint GetCharWork(ushort charId)
        => this.getGlobalWork!() + 0x1b0 + ((nint)charId * 0x2b4);

    public int GetEquip(ushort charId, ushort equipId)
        => *(ushort*)(this.GetCharWork(charId) + 0x28c + ((nint)equipId * 2));
}
