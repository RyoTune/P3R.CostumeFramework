using System.Runtime.InteropServices;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Models;

[StructLayout(LayoutKind.Explicit, Pack = 8, Size = 0x28)]
public struct FBustupParamTable
{
    [FieldOffset(0x0)] public FTableRowBase Super;
    [FieldOffset(0x8)] public ushort CharaID;
    [FieldOffset(0xA)] public ushort FaceID;
    [FieldOffset(0xC)] public ushort ClothID;
    [FieldOffset(0x10)] public FString Pose;
    [FieldOffset(0x20)] public bool EyeAnim;
    [FieldOffset(0x21)] public bool MouthAnim;
    [FieldOffset(0x22)] public byte InBetween;
}
