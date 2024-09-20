using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Hooks.Animations.Models;
using P3R.CostumeFramework.Hooks.Models;
using P3R.CostumeFramework.Hooks.Services;
using p3rpc.classconstructor.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Animations;

public unsafe class AnimationReplacer(CharAnim type, Character target, Character replacer, IObjectMethods objMethods)
{
    private readonly IObjectMethods objMethods = objMethods;
    private readonly CharAnim type = type;
    private readonly Character target = target;
    private readonly Character replacer = replacer;
    private readonly string targetAnimName = AssetUtils.GetAnimName(target, type) ?? string.Empty;
    private readonly string newAnimName = AssetUtils.GetAnimName(replacer, type) ?? string.Empty;

    private UAnimSequence* targetAnim;
    private UAnimSequence* newAnim;

    private bool hasReplaced;

    public void Update(UnrealObject obj)
    {
        if (this.hasReplaced)
        {
            return;
        }

        if (obj.Name.Equals(this.targetAnimName, StringComparison.OrdinalIgnoreCase))
        {
            this.targetAnim = (UAnimSequence*)obj.Self;
        }
        else if (obj.Name.Equals(this.newAnimName, StringComparison.OrdinalIgnoreCase))
        {
            this.newAnim = (UAnimSequence*)obj.Self;
        }

        if (this.targetAnim != null && this.newAnim != null)
        {
            var ogBaseObj = targetAnim->baseObj.baseObj.baseObj;
            var ogSkel = targetAnim->baseObj.baseObj.Skeleton;

            var newObj = (UAnimSequence*)this.objMethods.SpawnObject("AnimSequence", null);
            *newObj = *this.newAnim;

            *this.targetAnim = *newObj;

            this.targetAnim->baseObj.baseObj.baseObj = ogBaseObj;
            this.targetAnim->baseObj.baseObj.Skeleton = ogSkel;

            this.hasReplaced = true;
            Log.Information($"DngAnim Replaced: {this.type} || Target: {this.target} || New: {this.replacer}");
        }
    }
}