using P3R.CostumeFramework.Costumes.Models;
using Ryo.Interfaces;
using Ryo.Interfaces.Classes;

namespace P3R.CostumeFramework.Costumes;

internal class CostumeRyoService
{
    private readonly IRyoApi ryo;
    private readonly Dictionary<Character, IContainerGroup?> currentCostumeGroups = [];
    private readonly CostumeRegistry costumes;

    public CostumeRyoService(IRyoApi ryo, CostumeRegistry costumes)
    {
        this.ryo = ryo;
        this.costumes = costumes;
    }

    public void Refresh(Costume costume)
    {
        var character = costume.Character;
        this.currentCostumeGroups.TryGetValue(character, out var group);

        if (group?.Id != costume.RyoGroupId)
        {
            group?.Disable();
            var newGroup = this.ryo.GetContainerGroup(costume.RyoGroupId);
            this.currentCostumeGroups[character] = newGroup;
            newGroup.Enable();
        }
        else
        {
            group?.Disable();
            this.currentCostumeGroups[character] = null;
        }
    }
}
