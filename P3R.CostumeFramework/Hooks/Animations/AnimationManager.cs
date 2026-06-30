using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Hooks.Animations;
using P3R.CostumeFramework.Hooks.Animations.Models;
using UE.Toolkit.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

public unsafe class AnimationManager
{
    private readonly List<AnimationReplacer> replacers = [];

    public AnimationManager(IUnrealClasses toolkitClasses)
    {
        var spawn = new ObjSpawn();
        foreach (var type in Enum.GetValues<CharAnim>())
        {
            this.replacers.Add(new(type, Character.Player, Character.Mitsuru, toolkitClasses, spawn));
        }
    }

    public void Update(UnrealObject obj)
    {
        foreach (var replacer in replacers)
        {
            replacer.Update(obj);
        }
    }
}
