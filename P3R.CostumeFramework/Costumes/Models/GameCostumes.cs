using P3R.CostumeFramework.Types;
using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace P3R.CostumeFramework.Costumes.Models;

internal class GameCostumes : IReadOnlyList<Costume>
{
    private const int BASE_MOD_COSTUME_ID = 1000;
    private const int NUM_MOD_COSTUMES = 100;

    private readonly CostumeFilter filterSetting;
    private readonly Dictionary<CostumeFilter, int[]> filters = new()
    {
        [CostumeFilter.Non_Fanservice] = new[] { 102, 104, 106 }
    };

    private readonly List<Costume> costumes = new();

    public GameCostumes(CostumeFilter filter)
    {
        this.filterSetting = filter;

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "P3R.CostumeFramework.Resources.costumes.json";
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var gameCostumes = JsonSerializer.Deserialize<Dictionary<Character, Costume[]>>(json)!;
        foreach (var charCostumes in gameCostumes)
        {
            this.costumes.AddRange(charCostumes.Value);
        }

        // Enable all existing costumes.
        foreach (var costume in this.costumes)
        {
            costume.IsEnabled = IsCostumeEnabled(costume);
        }

        // Add mod costume slots.
        for (int i = 0; i < NUM_MOD_COSTUMES; i++)
        {
            var costumeId = BASE_MOD_COSTUME_ID + i;
            var costume = new Costume(costumeId);
            this.costumes.Add(costume);
        }
    }

    public Costume this[int index] => costumes[index];

    public int Count => costumes.Count;

    public IEnumerator<Costume> GetEnumerator() => costumes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => costumes.GetEnumerator();

    private bool IsCostumeEnabled(Costume costume)
    {
        if (costume.CostumeId == 154
            || costume.CostumeId == 501
            || costume.CostumeId == 502
            || costume.CostumeId == 503)
        {
            return false;
        }

        if (this.filters.TryGetValue(this.filterSetting, out var filter))
        {
            if (filter.Contains(costume.CostumeId))
            {
                return false;
            }
        }

        return true;
    }
}
