using P3R.CostumeFramework.Costumes.Models;

namespace P3R.CostumeFramework.Costumes;

internal class CostumeFactory
{
    private readonly GameCostumes costumes;

    public CostumeFactory(GameCostumes costumes)
    {
        this.costumes = costumes;
    }

    public Costume? Create(CostumeMod mod, string costumeDir, Character chararcter)
    {
        var costume = this.GetAvailableCostume();
        if (costume == null)
        {
            return null;
        }

        costume.Name = Path.GetFileName(costumeDir);
        costume.IsEnabled = true;
        costume.OwnerModId = mod.ModId;
        costume.Character = chararcter;

        ProcessCostume(mod, costume, costumeDir);
        Log.Information($"Costume created: {costume.Character} || Costume ID: {costume.CostumeId} || Folder: {costumeDir}");
        return costume;
    }

    public Costume? CreateFromExisting(Character character, string name, int costumeId)
    {
        var costume = this.GetAvailableCostume();
        if (costume == null)
        {
            return null;
        }

        costume.Character = character;
        costume.Name = name;
        costume.Config.Costume.MeshPath = $"/Game/Xrd777/Characters/Player/PC{character:0000}/Models/SK_PC{character:0000}_C{costumeId:000}.uasset";
        return costume;
    }

    private static void ProcessCostume(CostumeMod mod, Costume costume, string costumeDir)
    {
        AddCostumeFiles(mod, costume, costumeDir);
    }

    private static void AddCostumeFiles(CostumeMod mod, Costume costume, string costumeDir)
    {
        SetCostumeFile(mod, Path.Join(costumeDir, "base-mesh.uasset"), path => costume.Config.Base.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "base-anim.uasset"), path => costume.Config.Base.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "costume-mesh.uasset"), path => costume.Config.Costume.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "costume-anim.uasset"), path => costume.Config.Costume.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "hair-mesh.uasset"), path => costume.Config.Hair.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "hair-anim.uasset"), path => costume.Config.Hair.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "face-mesh.uasset"), path => costume.Config.Face.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "face-anim.uasset"), path => costume.Config.Face.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "music.pme"), path => costume.MusicScriptFile = path, SetType.Full);
        SetCostumeFile(mod, Path.Join(costumeDir, "battle.theme.pme"), path => costume.BattleThemeFile = path, SetType.Full);
    }

    private static void SetCostumeFile(CostumeMod mod, string modFile, Action<string> setFile, SetType type = SetType.Relative)
    {
        if (File.Exists(modFile))
        {
            if (type == SetType.Relative)
            {
                setFile(Path.GetRelativePath(mod.ContentDir, modFile));
            }
            else
            {
                setFile(modFile);
            }
        }
    }

    private Costume? GetAvailableCostume()
    {
        var costume = this.costumes.FirstOrDefault(x => x.Character == Character.NONE);
        if (costume == null)
        {
            Log.Warning("No available costume slot.");
        }

        return costume;
    }

    private enum SetType
    {
        Relative,
        Full,
    }
}
