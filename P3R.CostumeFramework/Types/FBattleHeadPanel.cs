using System.Runtime.InteropServices;

namespace P3R.CostumeFramework.Types;

[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 0x4020)]
public struct FBattleHeadPanel
{
    [FieldOffset(0x0)] public FBaseHeadPanel Super; // Size: 0xCA0
    [FieldOffset(0x1260)] public int PortraitState;
    [FieldOffset(0x1268)] public int PortraitBaseId;
    [FieldOffset(0x126c)] public int PortraitOutlineHigh;
    [FieldOffset(0x1270)] public int PortraitExprId;
}