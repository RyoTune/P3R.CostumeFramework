using System.Runtime.InteropServices;

namespace P3R.CostumeFramework.Hooks.Models;

// ULevelSequence is already declared in FBtlTheurgiaSeq.cs (same namespace),
// so it is reused here rather than redeclared.
[StructLayout(LayoutKind.Explicit, Pack = 8, Size = 0x28)]
public unsafe struct FBtlResultSequence
{
    [FieldOffset(0x0)] public TSoftObjectPtr<ULevelSequence> Sequencer; // 0x28
}
