using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace P3R.CostumeFramework.Utils;

internal static class YamlSerializer
{
    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public static T DeserializeFile<T>(string file)
        => deserializer.Deserialize<T>(File.ReadAllText(file));
}
