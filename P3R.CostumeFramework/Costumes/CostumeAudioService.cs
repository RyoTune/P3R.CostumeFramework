using Ryo.Interfaces;
using Ryo.Interfaces.Classes;

namespace P3R.CostumeFramework.Costumes;

internal class CostumeAudioService
{
    private readonly IRyoApi ryo;
    private readonly Dictionary<Character, IContainerGroup?> currentCostumeGroups = [];
    private readonly CostumeRegistry costumes;

    public CostumeAudioService(IRyoApi ryo, CostumeRegistry costumes)
    {
        this.ryo = ryo;
        this.costumes = costumes;
    }

    public void Refresh(Character character, int costumeId)
    {
        this.currentCostumeGroups.TryGetValue(character, out var group);

        if (this.costumes.TryGetCostume(character, costumeId, out var costume))
        {
            if (group?.Id != costume.AudioGroupId)
            {
                group?.Disable();
                var newGroup = this.ryo.GetContainerGroup(costume.AudioGroupId);
                this.currentCostumeGroups[character] = newGroup;
                newGroup.Enable();
            }
        }
        else
        {
            group?.Disable();
            this.currentCostumeGroups[character] = null;
        }
    }
}
