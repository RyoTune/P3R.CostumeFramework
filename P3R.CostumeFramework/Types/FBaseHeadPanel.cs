using System.Runtime.InteropServices;

namespace P3R.CostumeFramework.Types;

[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 0xCA0)]
public struct FBaseHeadPanel
{
    [FieldOffset(0x0)] public nint Vtable;
    [FieldOffset(0x10)] public ushort PlayerId;
    [FieldOffset(0x30)] public FSpriteDrawInstance Portrait;
    [FieldOffset(0x98)] public FSpriteDrawInstance OutlineHigh;
}

// FSprDefStruct, SprDefStruct1
[StructLayout(LayoutKind.Explicit, Size = 0x68)]
public struct FSpriteDrawInstance
{
    [FieldOffset(0x64)] public int SpriteIndex;
}