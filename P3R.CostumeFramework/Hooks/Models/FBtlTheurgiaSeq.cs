using System.Runtime.InteropServices;

namespace P3R.CostumeFramework.Hooks.Models;

public struct ULevelSequence { }

[StructLayout(LayoutKind.Sequential, Size = 0xC)]
public struct FTheurgiaVector
{
    public float X;
    public float Y;
    public float Z;
}

[StructLayout(LayoutKind.Explicit, Pack = 8, Size = 0x99)]
public unsafe struct FBtlTheurgiaSeq
{
    [FieldOffset(0x00)] public TSoftObjectPtr<ULevelSequence> Sequence;          // 0x28
    [FieldOffset(0x28)] public TSoftObjectPtr<ULevelSequence> SequenceEvolved;   // 0x28
    [FieldOffset(0x50)] public TSoftObjectPtr<ULevelSequence> SequenceSound;     // 0x28
    [FieldOffset(0x78)] public FTheurgiaVector PersonaScaleA;                    // 0x0C
    [FieldOffset(0x84)] public FTheurgiaVector PersonaScaleB;                    // 0x0C
    [FieldOffset(0x90)] public EBtlPersonaSceneAnimationType PersonaLoopAnimType;// 0x01
    [FieldOffset(0x91)] public bool NeedCommonSkillSceneFromTheurgia;            // 0x01
    [FieldOffset(0x94)] public int TheurgiaVoiceIndex;                           // 0x04
    [FieldOffset(0x98)] public bool DisableLOD;                                  // 0x01
}
public enum EBtlPersonaSceneAnimationType : byte
{
    BTL_ANIM_THEURGIA_A = 0,
    BTL_ANIM_THEURGIA_A_LOOP,
    BTL_ANIM_THEURGIA_B,
    BTL_ANIM_THEURGIA_B_LOOP,
    BTL_ANIM_MIXRAID_A,
    BTL_ANIM_MIXRAID_A_LOOP,
}