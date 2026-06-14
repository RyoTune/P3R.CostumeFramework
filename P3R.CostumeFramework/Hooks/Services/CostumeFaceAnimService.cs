using System.Runtime.InteropServices;
using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using P3R.CostumeFramework.Hooks.Animations;
using P3R.CostumeFramework.Hooks.Animations.Models;
using P3R.CostumeFramework.Hooks.Models;
using Unreal.ObjectsEmitter.Interfaces;
using Unreal.ObjectsEmitter.Interfaces.Types;
using UObject = p3rpc.nativetypes.Interfaces.UObject;
using UClass = p3rpc.nativetypes.Interfaces.UClass;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeFaceAnimService
{
    // UE 4.27 UObjectBase: ClassPrivate (UClass*) lives at offset 0x10.
    private const int ClassPrivateOffset = 0x10;

    // So I don't omega spam log
    private delegate UObject* StaticLoadObject(
        UClass* uclass, UObject* outer, nint name, void* fileName,
        ELoadFlags loadFlags, void* uPackageMap, bool bAllowObjectReconciliation, void* instancingContext);
    private StaticLoadObject? staticLoadObject;

    private readonly CostumeManager manager;

    private readonly Dictionary<Character, nint> assets = new();

    // Vanilla pointers 
    private readonly Dictionary<Character, Dictionary<int, nint>> vanilla = new();

    // Hi cache!!!!
    private readonly Dictionary<string, nint> loadedAnims = new(StringComparer.OrdinalIgnoreCase);

    // Cached AnimSequence UClass, derived from a live vanilla anim object.
    private nint animSequenceClass;

    public CostumeFaceAnimService(IUObjects uobjs, CostumeManager manager, CostumeHooks hooks)
    {
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
        var newPtr = (nint)asset;

        // Journey -> Ep. Aigis reloads this asset as a new instance. Drop the old
        // snapshot so we capture the pristine Astrea state instead of restoring
        // Journey pointers into it.
        if (this.assets.TryGetValue(character, out var oldPtr) && oldPtr != newPtr)
        {
            this.vanilla.Remove(character);
            this.loadedAnims.Clear();        // cached UAnimSequence* may now be freed
            this.animSequenceClass = nint.Zero; // re-derive from a live Astrea object
        }

        this.assets[character] = newPtr;

        if (!this.vanilla.ContainsKey(character))
        {
            var snapshot = new Dictionary<int, nint>();
            for (int i = 0; i < asset->Anims.mapNum; i++)
            {
                var element = &asset->Anims.elements[i];
                snapshot[element->Key] = element->Value;
            }
            this.vanilla[character] = snapshot;
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

    /// <summary>
    /// Resolves the AnimSequence UClass by reading ClassPrivate off any already-loaded
    /// vanilla face animation object. Cached after the first successful lookup.
    /// </summary>
    private nint GetAnimSequenceClass()
    {
        if (this.animSequenceClass != nint.Zero)
        {
            return this.animSequenceClass;
        }

        foreach (var snapshot in this.vanilla.Values)
        {
            foreach (var ptr in snapshot.Values)
            {
                if (ptr == nint.Zero)
                {
                    continue;
                }

                var classPtr = *(nint*)(ptr + ClassPrivateOffset);
                if (classPtr != nint.Zero)
                {
                    this.animSequenceClass = classPtr;
                    return this.animSequenceClass;
                }
            }
        }

        return nint.Zero;
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

        var animClassPtr = this.GetAnimSequenceClass();
        if (animClassPtr == nint.Zero)
        {
            Log.Error("Cannot load face animation: failed to resolve AnimSequence class.");
            return nint.Zero;
        }

        var animClass = (UClass*)animClassPtr;
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