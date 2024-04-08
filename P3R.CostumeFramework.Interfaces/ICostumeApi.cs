namespace P3R.CostumeFramework.Interfaces;

public interface ICostumeApi
{
    /// <summary>
    /// Add a costume overrides YAML file.
    /// </summary>
    /// <param name="file">File path.</param>
    void AddOverridesFile(string file);
}
