using System.Runtime.InteropServices;
using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Animations;
using P3R.CostumeFramework.Hooks.Animations.Models;
using P3R.CostumeFramework.Hooks.Models;
using p3rpc.classconstructor.Interfaces;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;
using UObject = p3rpc.nativetypes.Interfaces.UObject;
using UClass = p3rpc.nativetypes.Interfaces.UClass;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeFaceAnimService
{
    // So I don't omega spam log
    private delegate UObject* StaticLoadObject(
        UClass* uclass, UObject* outer, nint name, void* fileName,
        ELoadFlags loadFlags, void* uPackageMap, bool bAllowObjectReconciliation, void* instancingContext);
    private StaticLoadObject? staticLoadObject;

    private readonly IObjectMethods objMethods;
    private readonly CostumeManager manager;

    private readonly Dictionary<Character, nint> assets = new();

    // Vanilla pointers 
    private readonly Dictionary<Character, Dictionary<int, nint>> vanilla = new();

    // Hi cache!!!!
    private readonly Dictionary<string, nint> loadedAnims = new(StringComparer.OrdinalIgnoreCase);

    public CostumeFaceAnimService(IUObjects uobjs, IObjectMethods objMethods, CostumeManager manager, CostumeHooks hooks)
    {
        this.objMethods = objMethods;
        this.manager = manager;

        Project.Scans.AddScanHook(
            "FaceAnim_StaticLoadObject",
            "40 55 53 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC D8 06 00 00",
            (result, hooks) => this.staticLoadObject = hooks.CreateWrapper<StaticLoadObject>(result, out _));

        foreach (var character in Characters.PC)
        {
            var name = $"DA_PC{AssetUtils.GetCharIdString(character)}_FaceAnim";
            uobjs.FindObject(name, obj => this.OnAssetLoaded(character, obj));
        }

        hooks.OnCostumeChanged += _ => this.ApplyAll();
    }

    private void OnAssetLoaded(Character character, UnrealObject obj)
    {
        var asset = (UAppCharFaceAnimDataAsset*)obj.Self;
        this.assets[character] = (nint)asset;

        if (!this.vanilla.ContainsKey(character))
        {
            var snapshot = new Dictionary<int, nint>();
            for (int i = 0; i < asset->Anims.mapNum; i++)
            {
                var element = &asset->Anims.elements[i];
                snapshot[element->Key] = element->Value;
            }

            this.vanilla[character] = snapshot;
            Log.Debug($"FaceAnim asset loaded: {character} || {obj.Name} || {snapshot.Count} expressions.");
        }

        this.ApplyAll();
    }

    private void ApplyAll()
    {
        foreach (var costume in this.manager.GetCurrentCostumes())
        {
            this.Apply(costume);
        }
    }

    private void Apply(Costume costume)
    {
        if (!this.assets.TryGetValue(costume.Character, out var assetPtr))
        {
            return;
        }

        var asset = (UAppCharFaceAnimDataAsset*)assetPtr;

        this.RestoreVanilla(costume.Character, asset);

        foreach (var (faceId, assetFile) in this.GetOverrides(costume))
        {
            var element = asset->Anims.TryGetElement(faceId);
            if (element == null)
            {
                Log.Warning($"Face expression not present in asset, skipping: {costume.Character} || {(FaceAnimId)faceId} ({faceId}).");
                continue;
            }

            var unrealPath = AssetUtils.GetUnrealAssetPath(assetFile);
            var anim = this.LoadAnim(unrealPath);
            if (anim == nint.Zero)
            {
                Log.Error($"Failed to load face animation: {costume.Character} || {(FaceAnimId)faceId} || {unrealPath}");
                continue;
            }

            element->Value = anim;
            Log.Debug($"Face animation set: {costume.Character} || {(FaceAnimId)faceId} || {costume.Name}");
        }
    }

    private void RestoreVanilla(Character character, UAppCharFaceAnimDataAsset* asset)
    {
        if (!this.vanilla.TryGetValue(character, out var snapshot))
        {
            return;
        }

        foreach (var (key, ptr) in snapshot)
        {
            var element = asset->Anims.TryGetElement(key);
            if (element != null)
            {
                element->Value = ptr;
            }
        }
    }

    private IEnumerable<(int FaceId, string AssetFile)> GetOverrides(Costume costume)
    {
        foreach (var (key, value) in costume.Config.FacialAnimation)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (Enum.TryParse<FaceAnimId>(key, true, out var faceId))
            {
                yield return ((int)faceId, value);
            }
            else if (int.TryParse(key, out var rawId))
            {
                yield return (rawId, value);
            }
            else
            {
                Log.Warning($"Unknown facial animation key '{key}' for {costume.Character} || {costume.Name}.");
            }
        }
    }

    private nint LoadAnim(string unrealPath)
    {
        if (this.loadedAnims.TryGetValue(unrealPath, out var cached))
        {
            return cached;
        }

        if (this.staticLoadObject == null)
        {
            Log.Error("Cannot load face animation: StaticLoadObject not yet resolved.");
            return nint.Zero;
        }

        var animClass = this.objMethods.GetType("AnimSequence");
        var namePtr = Marshal.StringToHGlobalUni(unrealPath);
        try
        {
            var loaded = this.staticLoadObject(animClass, null, namePtr, null, ELoadFlags.LOAD_None, null, true, null);
            var ptr = (nint)loaded;
            if (ptr != nint.Zero)
            {
                this.loadedAnims[unrealPath] = ptr;
            }

            return ptr;
        }
        finally
        {
            Marshal.FreeHGlobal(namePtr);
        }
    }
}