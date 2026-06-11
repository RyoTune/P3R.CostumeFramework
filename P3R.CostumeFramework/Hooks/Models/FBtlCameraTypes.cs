using System.Runtime.InteropServices;

namespace P3R.CostumeFramework.Hooks.Models;

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public struct FBtlCommandCamera
{
    [FieldOffset(0x00)] public float CameraHeight;
    [FieldOffset(0x04)] public float CameraRoll;
    [FieldOffset(0x08)] public float CameraYaw;
    [FieldOffset(0x0C)] public float CranePitch;
    [FieldOffset(0x10)] public float CraneYaw;
    [FieldOffset(0x14)] public float CraneLength;
}

[StructLayout(LayoutKind.Explicit, Size = 0x0C)]
public struct FBtlVector3
{
    [FieldOffset(0x00)] public float X;
    [FieldOffset(0x04)] public float Y;
    [FieldOffset(0x08)] public float Z;
}
