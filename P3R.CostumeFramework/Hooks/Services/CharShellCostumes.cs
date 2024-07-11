using P3R.CostumeFramework.Costumes;

namespace P3R.CostumeFramework.Hooks.Services;

public class CharShellCostumes
{
    private readonly ShellCostume shell_1;
    private readonly ShellCostume shell_2;

    private ShellCostume currShellCostume;
    private int currShellCostumeId = 0;

    public CharShellCostumes(Character character)
    {
        this.Character = character;
        this.shell_1 = new ShellCostume(character, 502);
        this.shell_2 = new ShellCostume(character, 504);

        this.currShellCostume = this.shell_1;
    }
    public Character Character { get; }

    public ShellCostume GetShellCostume(int costumeId)
    {
        // Swap shell costumes if different costume ID.
        if (this.currShellCostumeId != costumeId)
        {
            this.currShellCostume = (this.currShellCostume == this.shell_1) ? this.shell_2 : this.shell_1;
            this.currShellCostumeId = costumeId;
        }

        return this.currShellCostume;
    }
}