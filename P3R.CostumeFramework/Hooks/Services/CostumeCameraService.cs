using System.Globalization;
using System.Xml.Linq;
using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Models;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

internal sealed unsafe class CostumeCameraService
{
    private readonly CostumeManager manager;

    private readonly OverlayTable<FBtlCommandCamera> itemCamera;
    private readonly OverlayTable<FBtlCommandCamera> skillCamera;
    private readonly OverlayTable<FBtlCommandCamera> theurgiaCamera;
    private readonly OverlayTable<FBtlCommandCamera> theurgiaNoUseCamera;

    private readonly OverlayTable<FBtlVector3> kiraOffset;
    private readonly OverlayTable<FBtlVector3> lookat;
    private readonly OverlayTable<FBtlVector3> lookatNoUse;

    private readonly Dictionary<string, CameraOverride> fileCache = new(StringComparer.OrdinalIgnoreCase);

    public CostumeCameraService(IDataTables dt, CostumeManager manager, CostumeHooks hooks)
    {
        this.manager = manager;

        this.itemCamera = new(dt, "DT_BtlCommandItemCameraTable", this.ApplyAll);
        this.skillCamera = new(dt, "DT_BtlCommandSkillCameraTable", this.ApplyAll);
        this.theurgiaCamera = new(dt, "DT_BtlCommandTheurgiaCameraTable", this.ApplyAll);
        this.theurgiaNoUseCamera = new(dt, "DT_BtlCommandTheurgiaNoUseCameraTable", this.ApplyAll);

        this.kiraOffset = new(dt, "DT_BtlTheurgiaKiraOffset", this.ApplyAll);
        this.lookat = new(dt, "DT_BtlTheurgiaLookat", this.ApplyAll);
        this.lookatNoUse = new(dt, "DT_BtlTheurgiaLookatNoUse", this.ApplyAll);

        hooks.OnCostumeChanged += _ => this.ApplyAll();
    }

    private void ApplyAll()
    {
        this.itemCamera.RestoreVanilla();
        this.skillCamera.RestoreVanilla();
        this.theurgiaCamera.RestoreVanilla();
        this.theurgiaNoUseCamera.RestoreVanilla();
        this.kiraOffset.RestoreVanilla();
        this.lookat.RestoreVanilla();
        this.lookatNoUse.RestoreVanilla();

        foreach (var costume in this.manager.GetCurrentCostumes())
        {
            if (string.IsNullOrEmpty(costume.CameraFile))
            {
                continue;
            }

            var rowName = GetRowName(costume.Character);
            if (rowName == null)
            {
                continue;
            }

            var ov = this.GetOverride(costume.CameraFile);

            if (ov.Item is { } item)
                this.itemCamera.Apply(rowName, p => ApplyCamera((FBtlCommandCamera*)p, item));
            if (ov.Skill is { } skill)
                this.skillCamera.Apply(rowName, p => ApplyCamera((FBtlCommandCamera*)p, skill));
            if (ov.Theurgia is { } theu)
                this.theurgiaCamera.Apply(rowName, p => ApplyCamera((FBtlCommandCamera*)p, theu));
            if (ov.TheurgiaNoUse is { } theuNo)
                this.theurgiaNoUseCamera.Apply(rowName, p => ApplyCamera((FBtlCommandCamera*)p, theuNo));

            if (ov.KiraOffset is { } kira)
                this.kiraOffset.Apply(rowName, p => ApplyVector((FBtlVector3*)p, kira));
            if (ov.Lookat is { } look)
                this.lookat.Apply(rowName, p => ApplyVector((FBtlVector3*)p, look));
            if (ov.LookatNoUse is { } lookNo)
                this.lookatNoUse.Apply(rowName, p => ApplyVector((FBtlVector3*)p, lookNo));

            Log.Debug($"Camera overrides applied: {costume.Character} || row {rowName} || {costume.Name}.");
        }
    }

    private static void ApplyCamera(FBtlCommandCamera* ptr, CameraValues v)
    {
        if (v.CameraHeight.HasValue) ptr->CameraHeight = v.CameraHeight.Value;
        if (v.CameraRoll.HasValue) ptr->CameraRoll = v.CameraRoll.Value;
        if (v.CameraYaw.HasValue) ptr->CameraYaw = v.CameraYaw.Value;
        if (v.CranePitch.HasValue) ptr->CranePitch = v.CranePitch.Value;
        if (v.CraneYaw.HasValue) ptr->CraneYaw = v.CraneYaw.Value;
        if (v.CraneLength.HasValue) ptr->CraneLength = v.CraneLength.Value;
    }

    private static void ApplyVector(FBtlVector3* ptr, VectorValues v)
    {
        if (v.X.HasValue) ptr->X = v.X.Value;
        if (v.Y.HasValue) ptr->Y = v.Y.Value;
        if (v.Z.HasValue) ptr->Z = v.Z.Value;
    }

    private static string? GetRowName(Character character) => character switch
    {
        Character.Player => "HERO",
        Character.Yukari => "YUKARI",
        Character.Stupei => "JUNPEI",
        Character.Akihiko => "SANADA",
        Character.Mitsuru => "MITURU",   // sic
        Character.Fuuka => "FUKA",       // sic
        Character.Aigis => "AEGIS",      // sic
        Character.Ken => "AMADA",
        Character.Koromaru => "KOROMARU",
        Character.Shinjiro => "ARAGAKI",
        Character.Metis => "METIS",          
        Character.AigisReal => "HEROAEGIS",  
        _ => null,
    };

    private CameraOverride GetOverride(string file)
    {
        if (this.fileCache.TryGetValue(file, out var cached))
        {
            return cached;
        }

        var parsed = ParseFile(file);
        this.fileCache[file] = parsed;
        return parsed;
    }

    private static CameraOverride ParseFile(string file)
    {
        var ov = new CameraOverride();
        try
        {
            var root = XDocument.Load(file).Root;
            if (root == null)
            {
                return ov;
            }

            ov.Item = ReadCamera(root.Element("ItemCamera"));
            ov.Skill = ReadCamera(root.Element("SkillCamera"));
            ov.Theurgia = ReadCamera(root.Element("TheurgiaCamera"));
            ov.TheurgiaNoUse = ReadCamera(root.Element("TheurgiaNoUseCamera"));

            ov.KiraOffset = ReadVector(root.Element("KiraOffset"));
            ov.Lookat = ReadVector(root.Element("Lookat"));
            ov.LookatNoUse = ReadVector(root.Element("LookatNoUse"));

            Log.Information($"Parsed camera overrides from {Path.GetFileName(file)}.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to parse camera file: {file}");
        }

        return ov;
    }

    private static CameraValues? ReadCamera(XElement? el)
    {
        if (el == null)
        {
            return null;
        }

        return new CameraValues
        {
            CameraHeight = ReadFloat(el, "CameraHeight"),
            CameraRoll = ReadFloat(el, "CameraRoll"),
            CameraYaw = ReadFloat(el, "CameraYaw"),
            CranePitch = ReadFloat(el, "CranePitch"),
            CraneYaw = ReadFloat(el, "CraneYaw"),
            CraneLength = ReadFloat(el, "CraneLength"),
        };
    }

    private static VectorValues? ReadVector(XElement? el)
    {
        if (el == null)
        {
            return null;
        }

        return new VectorValues
        {
            X = ReadFloat(el, "X"),
            Y = ReadFloat(el, "Y"),
            Z = ReadFloat(el, "Z"),
        };
    }

    private static float? ReadFloat(XElement parent, string name)
    {
        var raw = (string?)parent.Element(name)?.Attribute("value");
        if (raw == null)
        {
            return null;
        }

        return float.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
            ? f
            : null;
    }

    private sealed class CameraOverride
    {
        public CameraValues? Item;
        public CameraValues? Skill;
        public CameraValues? Theurgia;
        public CameraValues? TheurgiaNoUse;
        public VectorValues? KiraOffset;
        public VectorValues? Lookat;
        public VectorValues? LookatNoUse;
    }

    private sealed class CameraValues
    {
        public float? CameraHeight;
        public float? CameraRoll;
        public float? CameraYaw;
        public float? CranePitch;
        public float? CraneYaw;
        public float? CraneLength;
    }

    private sealed class VectorValues
    {
        public float? X;
        public float? Y;
        public float? Z;
    }

    private sealed unsafe class OverlayTable<TRow> where TRow : unmanaged
    {
        private readonly string name;
        private readonly List<DataTable<TRow>> tables = new();
        private readonly Dictionary<DataTable<TRow>, Dictionary<string, TRow>> vanilla = new();

        public OverlayTable(IDataTables dt, string name, Action onLoaded)
        {
            this.name = name;

            dt.FindDataTable<TRow>(name, loaded =>
            {
                if (!this.tables.Contains(loaded))
                {
                    this.tables.Add(loaded);
                }

                var snapshot = new Dictionary<string, TRow>(StringComparer.OrdinalIgnoreCase);
                foreach (var row in loaded.Rows)
                {
                    snapshot[row.Name] = *row.Self;
                }

                this.vanilla[loaded] = snapshot;
                Log.Information($"Camera table loaded: {this.name} ({snapshot.Count} rows).");
                onLoaded();
            });
        }

        public void RestoreVanilla()
        {
            foreach (var table in this.tables)
            {
                if (!this.vanilla.TryGetValue(table, out var snapshot))
                {
                    continue;
                }

                foreach (var row in table.Rows)
                {
                    if (snapshot.TryGetValue(row.Name, out var original))
                    {
                        *row.Self = original;
                    }
                }
            }
        }

        public void Apply(string rowName, Action<nint> apply)
        {
            foreach (var table in this.tables)
            {
                var row = table.Rows.FirstOrDefault(x => x.Name.Equals(rowName, StringComparison.OrdinalIgnoreCase));
                if (row != null)
                {
                    apply((nint)row.Self);
                }
            }
        }
    }
}
