using System.Runtime.InteropServices;

namespace P3R.CostumeFramework.Types;

[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 0xEF0)]
public struct FFieldHeadPanel
{
    [FieldOffset(0x0)] public FBaseHeadPanel Super;
    [FieldOffset(0xca0)] public FSpriteDrawInstance cardBlueBgTrans;
    [FieldOffset(0xd08)] public FSpriteDrawInstance portraitShadow0;
    [FieldOffset(0xd70)] public FSpriteDrawInstance portraitShadow1;
}