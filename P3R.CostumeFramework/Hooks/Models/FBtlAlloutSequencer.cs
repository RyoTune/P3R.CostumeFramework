using System.Runtime.InteropServices;

namespace P3R.CostumeFramework.Hooks.Models;

[StructLayout(LayoutKind.Explicit, Pack = 8, Size = 0x50)]
public unsafe struct FBtlAlloutSequencer
{
    [FieldOffset(0x0)] public TSoftObjectPtr<ULevelSequence> AlloutA;  // 0x28
    [FieldOffset(0x28)] public TSoftObjectPtr<ULevelSequence> AlloutB; // 0x28
}
