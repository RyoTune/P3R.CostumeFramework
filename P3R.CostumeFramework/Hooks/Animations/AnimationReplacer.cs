using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Hooks.Animations.Models;
using P3R.CostumeFramework.Hooks.Models;
using P3R.CostumeFramework.Hooks.Services;
using p3rpc.nativetypes.Interfaces;
using UE.Toolkit.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Animations;

public unsafe class AnimationReplacer(CharAnim type, Character target, Character replacer, IUnrealClasses toolkitClasses, ObjSpawn spawn)
{
    private readonly ObjSpawn spawn = spawn;
    private readonly IUnrealClasses toolkitClasses = toolkitClasses;
    private readonly CharAnim type = type;
    private readonly Character target = target;
    private readonly Character replacer = replacer;
    private readonly string targetAnimName = AssetUtils.GetAnimName(target, type) ?? string.Empty;
    private readonly string newAnimPath = AssetUtils.GetAnimPath(replacer, type);

    public void Update(UnrealObject obj)
    {
        if (this.newAnimPath == null)
        {
            return;
        }

        if (obj.Name.Equals(this.targetAnimName, StringComparison.OrdinalIgnoreCase))
        {
            var targetAnim = (UAnimSequence*)obj.Self;

            var ogBaseObj = targetAnim->baseObj.baseObj.baseObj;
            var ogSkeleton = targetAnim->baseObj.baseObj.Skeleton;
            this.toolkitClasses.GetClassInfoFromName("AnimSequence", out var animSequenceType);
            
            var newAnim = (UAnimSequence*)this.spawn.StaticLoadObjectImpl((UClass*)animSequenceType!.Ptr, null, this.newAnimPath);
            if (newAnim == null)
            {
                Log.Error($"Failed to create new animation: {this.newAnimPath}");
                return;
            }

            *targetAnim = *newAnim;
            //this.targetAnim->baseObj.baseObj.baseObj = ogBaseObj;
            //this.targetAnim->baseObj.baseObj.Skeleton = ogSkeleton;
        }
    }
}
public enum ELoadFlags : uint
{
    LOAD_None = 0x00000000,
};