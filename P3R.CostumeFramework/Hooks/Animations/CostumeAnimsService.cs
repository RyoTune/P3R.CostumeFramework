using P3R.CostumeFramework.Hooks.Animations.Models;
using P3R.CostumeFramework.Hooks.Models;
using p3rpc.classconstructor.Interfaces;
using Unreal.ObjectsEmitter.Interfaces;

namespace P3R.CostumeFramework.Hooks.Animations;

internal unsafe class CostumeAnimsService
{
    //private readonly AnimationManager animManager;

    public CostumeAnimsService(IUObjects uobjs, IObjectMethods objMethods)
    {
        //this.animManager = new(objMethods);
        //uobjs.ObjectCreated += animManager.Update;

        uobjs.FindObject("SKEL_Human", obj =>
        {
            var bones = new GameBones();
            string[] keywords =
            [
                "head",
                "eye",
                "brow",
                "cheek",
                "nose",
                "mouth",
                "jaw",
                "tongue",
                "lips",
                "hair",
                "face",
                "mask",
                "iris",
                "pupil",
                "laugh",
                "tooth"
            ];

            var skel = (USkeleton*)obj.Self;
            for (int i = 0; i < skel->BoneTree.Num; i++)
            {
                var bone = &skel->BoneTree.AllocatorInstance[i];
                var name = bones[i];
                bone->TranslationRetargetingMode = EBoneTranslationRetargetingMode.OrientAndScale;
                if (keywords.Any(x => name.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Debug($"SKEL_Human ({name}): Retarget bone animation to {EBoneTranslationRetargetingMode.OrientAndScale}.");
                    bone->TranslationRetargetingMode = EBoneTranslationRetargetingMode.OrientAndScale;
                }
            }
        });
    }
}
