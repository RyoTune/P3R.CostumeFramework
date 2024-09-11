namespace P3R.CostumeFramework.Costumes;

public enum Character
    : ushort
{
    NONE,
    Player,
    Yukari,
    Stupei,
    Akihiko,
    Mitsuru,
    Fuuka,
    Aigis,
    Ken,
    Koromaru,
    Shinjiro,

    Metis,
    Aigis12,

    // Side-characters.
    Kenji = 101,
    Hidetoshi,
    Bunkichi,
    Mitsuko,
    Kazushi,
    Yuko,
    Keisuke = 108,
    Chihiro,
    Maiko,
    Pharos,
    Andre_Laurent_Jean_Geraux,
    Tanaka,
    Mutatsu,
    Mamoru,
    Akinari,

    Igor = 201,
    Elizabeth,

    Takaya = 211,
    Jin,
    Chidori,

    Ryoji = 221,
    Ikutsuki,
    Natsuki,
    Takeharu,

    FEMC = 999,
}

public static class Characters
{
    public static readonly Character[] PC =
    [
        Character.Player,
        Character.Yukari,
        Character.Stupei,
        Character.Akihiko,
        Character.Mitsuru,
        Character.Fuuka,
        Character.Aigis,
        Character.Ken,
        Character.Koromaru,
        Character.Shinjiro,
        Character.Metis,
        Character.Aigis12,
    ];
}
