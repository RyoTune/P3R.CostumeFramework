using System;
using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;

namespace P3R.CostumeFramework.Hooks.Services;

internal class CostumeManager
{
    private readonly Dictionary<Character, Costume> current = [];

    public CostumeManager(CostumeHooks costumeHooks)
    {
        costumeHooks.OnCostumeChanged += costume =>
        {
            this.current[costume.Character] = costume;
            // Removed the Invoke call
        };
    }

    public Costume[] GetCurrentCostumes() => this.current.Values.ToArray();
}