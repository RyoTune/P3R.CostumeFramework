﻿using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Hooks.Animations;
using P3R.CostumeFramework.Hooks.Animations.Models;
using p3rpc.classconstructor.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;

namespace P3R.CostumeFramework.Hooks.Services;

public class AnimationManager
{
    private readonly List<AnimationReplacer> replacers = [];

    public AnimationManager(IObjectMethods objMethods)
    {
        foreach (var type in Enum.GetValues<CharAnim>())
        {
            this.replacers.Add(new(type, Character.Player, Character.Mitsuru, objMethods));
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