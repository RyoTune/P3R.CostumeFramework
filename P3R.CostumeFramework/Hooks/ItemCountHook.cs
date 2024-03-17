using P3R.CostumeFramework.Costumes.Models;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;

namespace P3R.CostumeFramework.Hooks;

internal class ItemCountHook
{
    [Function(CallingConventions.Microsoft)]
    private delegate nint FUN_14c15cad0(int itemId);
    private IHook<FUN_14c15cad0>? hook;

    public ItemCountHook()
    {
        ScanHooks.Add(
            "GET_ITEM_NUM",
            "49 89 E3 48 81 EC 88 00 00 00 48 8B 05 ?? ?? ?? ?? 48 31 E0",
            (hooks, result) => this.hook = hooks.CreateHook<FUN_14c15cad0>(this.Hook, result).Activate());
    }

    private nint Hook(int itemId)
    {
        if (Costume.IsItemIdCostume(itemId)
            && IsDlcCostume(itemId) == false)
        {
            return 1;
        }

        return this.hook!.OriginalFunction(itemId);
    }

    private static bool IsDlcCostume(int itemId)
    {
#if RELEASE
        var costumeItemId = Costume.GetCostumeItemId(itemId);
        if (costumeItemId >= 90
            && costumeItemId <= 119)
        {
            return true;
        }
#endif

        return false;
    }
}
