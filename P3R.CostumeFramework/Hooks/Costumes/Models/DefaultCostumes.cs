using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace P3R.CostumeFramework.Hooks.Costumes.Models;

internal class DefaultCostumes : IReadOnlyDictionary<Character, Costume>
{
    private readonly Dictionary<Character, Costume> costumes = [];

    public DefaultCostumes(bool isPlayerFemc = false)
    {
        foreach (var character in Characters.PC)
        {
            if (character == Character.Player)
            {
                if (isPlayerFemc)
                    costumes[character] = new FemcCostume();
                else
                    costumes[character] = new DefaultCostume(Character.Player);
            }
            else
            {
                costumes[character] = new DefaultCostume(character);
            }
        }
    }

    public Costume this[Character key] => throw new NotImplementedException();

    public IEnumerable<Character> Keys => throw new NotImplementedException();

    public IEnumerable<Costume> Values => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool ContainsKey(Character key)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<Character, Costume>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(Character key, [MaybeNullWhen(false)] out Costume value)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
