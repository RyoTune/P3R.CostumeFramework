using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Utils;

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
        Log.Information($"Costume created: {costume.Character} || Costume ID: {costume.CostumeId}\nFolder: {costumeDir}");
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
        costume.IsEnabled = true;
        costume.Name = name;
        costume.Config.Costume.MeshPath = $"/Game/Xrd777/Characters/Player/PC{character:0000}/Models/SK_PC{character:0000}_C{costumeId:000}.uasset";
        return costume;
    }

    public Costume? CreateFromExisting(Character character, string name, string existingMesh)
    {
        var costume = this.GetAvailableCostume();
        if (costume == null)
        {
            return null;
        }

        costume.Character = character;
        costume.IsEnabled = true;
        costume.Name = name;
        costume.Config.Costume.MeshPath = existingMesh;
        return costume;
    }

    private static void ProcessCostume(CostumeMod mod, Costume costume, string costumeDir)
    {
        LoadCostumeData(mod, costume, costumeDir);
    }

    private static void LoadCostumeData(CostumeMod mod, Costume costume, string costumeDir)
    {
        // Load config first so costume asset stuff is
        // overwritten by actual files.
        SetCostumeFile(mod, Path.Join(costumeDir, "config.yaml"), path =>
        {
            var config = YamlSerializer.DeserializeFile<CostumeConfig>(path);

            if (config.Name != null) costume.Name = config.Name;
            if (config.Base.MeshPath != null) costume.Config.Base.MeshPath = config.Base.MeshPath;
            if (config.Costume.MeshPath != null) costume.Config.Costume.MeshPath = config.Costume.MeshPath;
            if (config.Face.MeshPath != null) costume.Config.Face.MeshPath = config.Face.MeshPath;
            if (config.Hair.MeshPath != null) costume.Config.Hair.MeshPath = config.Hair.MeshPath;
            if (config.Allout.NormalPath != null) costume.Config.Allout.NormalPath = config.Allout.NormalPath;
            if (config.Allout.NormalMaskPath != null) costume.Config.Allout.NormalMaskPath = config.Allout.NormalMaskPath;
            if (config.Allout.SpecialPath != null) costume.Config.Allout.SpecialPath = config.Allout.SpecialPath;
            if (config.Allout.SpecialMaskPath != null) costume.Config.Allout.SpecialMaskPath = config.Allout.SpecialMaskPath;
            if (config.Allout.TextPath != null) costume.Config.Allout.TextPath = config.Allout.TextPath;
            if (config.Allout.PlgPath != null) costume.Config.Allout.PlgPath = config.Allout.PlgPath;

        }, SetType.Full);

        SetCostumeFile(mod, Path.Join(costumeDir, "base-mesh.uasset"), path => costume.Config.Base.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "base-anim.uasset"), path => costume.Config.Base.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "costume-mesh.uasset"), path => costume.Config.Costume.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "costume-anim.uasset"), path => costume.Config.Costume.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "hair-mesh.uasset"), path => costume.Config.Hair.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "hair-anim.uasset"), path => costume.Config.Hair.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "face-mesh.uasset"), path => costume.Config.Face.MeshPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "face-anim.uasset"), path => costume.Config.Face.AnimPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "allout-normal.uasset"), path => costume.Config.Allout.NormalPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "allout-normal-mask.uasset"), path => costume.Config.Allout.NormalMaskPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "allout-special.uasset"), path => costume.Config.Allout.SpecialPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "allout-special-mask.uasset"), path => costume.Config.Allout.SpecialMaskPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "allout-text.uasset"), path => costume.Config.Allout.TextPath = path);
        SetCostumeFile(mod, Path.Join(costumeDir, "allout-plg.uasset"), path => costume.Config.Allout.PlgPath = path);

        SetCostumeFile(mod, Path.Join(costumeDir, "music.pme"), path => costume.MusicScriptFile = path, SetType.Full);
        SetCostumeFile(mod, Path.Join(costumeDir, "battle.theme.pme"), path => costume.BattleThemeFile = path, SetType.Full);

        SetCostumeFile(mod, Path.Join(costumeDir, "description.msg"), path => costume.Description = File.ReadAllText(path), SetType.Full);
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
