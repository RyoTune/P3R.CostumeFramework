using P3R.CostumeFramework.Costumes;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeShellService
{
    private readonly Dictionary<Character, CharShellCostumes> charShellCostumes = new();

    private readonly Dictionary<ShellCostume, ShellCostumeFStrings> shellFStrings = new();

    private readonly IUnreal unreal;

    public CostumeShellService(IUnreal unreal)
    {
        this.unreal = unreal;
        foreach (var character in Enum.GetValues<Character>())
        {
            this.charShellCostumes[character] = new(character);
        }
    }

    /// <summary>
    /// Gets the shell costume to use for the given character
    /// and costume ID.
    /// </summary>
    /// <param name="character">Character.</param>
    /// <param name="costumeId">Costume ID requesting shell.</param>
    /// <returns>Shell costume to use.</returns>
    public ShellCostume GetShellCostume(Character character, int costumeId)
        => this.charShellCostumes[character].GetShellCostume(costumeId);

    /// <summary>
    /// Gets the FString of the shell's original costume mesh path.
    /// </summary>
    /// <param name="shellCostume">Shell costume.</param>
    /// <returns>Shell costume's original FString.</returns>
    public ShellCostumeFStrings GetShellFStrings(ShellCostume shellCostume)
    {
        if (this.shellFStrings.TryGetValue(shellCostume, out var fstrings))
        {
            return fstrings;
        }

        // FNames do update in real time?
        // Changing the FString value of an FName means you can't retrieve it with the same string.
        // I guess makes sense since how else does it know if a string has been made before?
        var costumeMesh_FName = this.unreal.FName(shellCostume.CostumeMeshPath);
        var costumeMesh_FString = this.unreal.GetPool()->GetFString(costumeMesh_FName->pool_location);

        var hairMesh_FName = this.unreal.FName(shellCostume.HairMeshPath);
        var hairMesh_FString = this.unreal.GetPool()->GetFString(hairMesh_FName->pool_location);

        var faceMesh_FName = this.unreal.FName(shellCostume.FaceMeshPath);
        var faceMesh_FString = this.unreal.GetPool()->GetFString(faceMesh_FName->pool_location);

        this.shellFStrings[shellCostume] = new(costumeMesh_FString, hairMesh_FString, faceMesh_FString);
        return this.shellFStrings[shellCostume];
    }

    public class ShellCostumeFStrings
    {
        public ShellCostumeFStrings(FStringAnsi* costumeMesh, FStringAnsi* hairMesh, FStringAnsi* faceMesh)
        {
            CostumeMesh = costumeMesh;
            HairMesh = hairMesh;
            FaceMesh = faceMesh;
        }

        public FStringAnsi* CostumeMesh { get; set; }

        public FStringAnsi* HairMesh { get; set; }

        public FStringAnsi* FaceMesh { get; set; }
    }
}
