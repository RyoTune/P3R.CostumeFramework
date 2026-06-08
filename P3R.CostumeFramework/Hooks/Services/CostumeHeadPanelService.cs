using System.Numerics;
using System.Runtime.InteropServices;
using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using p3rpc.asyncassetloader.Interfaces;
// using p3rpc.nativetypes.Interfaces;
using Reloaded.Hooks.Definitions;
using RyoTune.Persona3Reload.Types;
using UE.Toolkit.Core.Types;
using UE.Toolkit.Core.Types.Unreal.Factories.Interfaces;
using UE.Toolkit.Core.Types.Unreal.UE5_4_4;
using UE.Toolkit.Interfaces;
using FBaseHeadPanel = P3R.CostumeFramework.Types.FBaseHeadPanel;
using FBattleHeadPanel = P3R.CostumeFramework.Types.FBattleHeadPanel;
using FFieldHeadPanel = P3R.CostumeFramework.Types.FFieldHeadPanel;

namespace P3R.CostumeFramework.Hooks.Services;

public unsafe class CostumeHeadPanelAssets
{
    private UTexture2D** Storage = (UTexture2D**)NativeMemory.AllocZeroed((nuint)sizeof(nint) * 3);

    public UTexture2D* GetBattleTexture() => Storage[0];
    public UTexture2D* GetCampTexture() => Storage[1];
    public UTexture2D* GetFieldTexture() => Storage[2];

    public UTexture2D** GetBattleTexturePtr() => Storage;
    public UTexture2D** GetCampTexturePtr() => Storage + 1;
    public UTexture2D** GetFieldTexturePtr() => Storage + 2;

    public int BattleSprIndex { get; set; } = -1;
    public int CampSprIndex { get; set; } = -1;
    public int FieldSprIndex { get; set; } = -1;
}

internal unsafe class CostumeHeadPanelService
{
    private readonly CostumeManager manager;
    private readonly IUnrealMemory toolkitMemory;
    private readonly IUnrealObjects toolkitObjects;
    private readonly IUnrealSpawning toolkitSpawning;
    private readonly IAssetLoader assetLoader;

    private ToolkitUObject<USprAsset>? SPR_UI_Battle_PartyPanel;
    private ToolkitUObject<USprAsset>? SPR_UI_Camp_PartyPanel;
    private ToolkitUObject<USprAsset>? SPR_UI_Field_PartyPanel;
    private IUObject? CostumeFrameworkAssetLoader;

    private Dictionary<int, CostumeHeadPanelAssets> CostumeAssets = new();

    private static FSprData CreateSpriteInner(float Width, float Height, FVector2D TexStart, FVector2D TexEnd, UTexture2D* Tex)
    {
        var ValueOut = new FSprData
        {
            Width = Width,
            Height = Height,
            U0 = TexStart.X,
            V0 = TexStart.Y,
            U1 = TexEnd.X,
            v1 = TexEnd.Y,
            Texture = (UTexture*)Tex,
        };
        // These could be arrays (fixed works for primitive types) for but toolkit's struct gen doesn't account for that.
        var RGBA = (uint*)((nint)(&ValueOut) + 0x20);
        var StretchLen = (ushort*)((nint)(&ValueOut) + 0x30);
        var ScalingSize = (uint*)((nint)(&ValueOut) + 0x38);
        for (var i = 0; i < 4; i++)
        {
            RGBA[i] = uint.MaxValue;
            StretchLen[i] = 0;
            ScalingSize[i % 2] = 0;
        }
        return ValueOut;
    }

    private static FSprData CreateBattleSprite(int Index, UTexture2D* Tex)
    {
        // Treat the image as a 4 x 4 grid
        var PlatformSize = *(int**)((nint)Tex + 0x190); // FTexturePlatformData
        var Width = PlatformSize[0] / 4; // FTexturePlatformData->SizeX
        var Height = PlatformSize[1] / 4; // FTexturePlatformData->SizeY
        var TexStart = new FVector2D
        {
            X = Index % 4 / 4f,
            Y = Index / 4 / 4f
        };
        var TexEnd = new FVector2D
        {
            X = (Index % 4 + 1) / 4f,
            Y = (Index + 4) / 4 / 4f
        };
        return CreateSpriteInner(Width, Height, TexStart, TexEnd, Tex);
    }

    private static FSprData CreateSingleSprite(UTexture2D* Tex)
    {
        var PlatformSize = *(int**)((nint)Tex + 0x190); // FTexturePlatformData
        return CreateSpriteInner(
            PlatformSize[0], 
            PlatformSize[1], 
            new FVector2D { X = 0, Y = 0 }, 
            new FVector2D { X = 1, Y = 1 }, 
            Tex);
    }
    
    public CostumeHeadPanelService(CostumeManager manager, IUnrealMemory toolkitMemory,
        IUnrealObjects toolkitObjects, IUnrealSpawning toolkitSpawning, IAssetLoader assetLoader)
    {
        this.manager = manager;
        this.toolkitMemory = toolkitMemory;
        this.toolkitObjects = toolkitObjects;
        this.toolkitSpawning = toolkitSpawning;
        this.assetLoader = assetLoader;
        Project.Scans.AddScanHook(
            nameof(FBattleHeadPanel_UpdateState),
            "4C 8B DC 49 89 4B ?? 55 53 49 8D 6B ??",
            (result, hooks) => _FBattleHeadPanel_UpdateState = hooks
                .CreateHook<FBattleHeadPanel_UpdateState>(FBattleHeadPanel_UpdateStateImpl, result)
                .Activate()
        );
        Project.Scans.AddScanHook(
            nameof(FCampHeadPanel_SetPlayerTextureIndex),
            "48 89 5C 24 ?? 57 48 83 EC 30 33 FF 89 51 ??",
            (result, hooks) => _FCampHeadPanel_SetPlayerTextureIndex = hooks
                .CreateHook<FCampHeadPanel_SetPlayerTextureIndex>(FCampHeadPanel_SetPlayerTextureIndexImpl, result)
                .Activate()
        );
        
        Project.Scans.AddScanHook(
            nameof(FFieldHeadPanel_SetPlayerTextureIndex),
            "40 53 48 83 EC 20 44 89 41 ??",
            (result, hooks) => _FFieldHeadPanel_SetPlayerTextureIndexImpl = hooks
                .CreateHook<FFieldHeadPanel_SetPlayerTextureIndex>(FFieldHeadPanel_SetPlayerTextureIndexImpl, result)
                .Activate()
        );
        this.toolkitObjects.OnObjectLoadedByName<USprAsset>("SPR_UI_Battle_PartyPanel", x =>
        {
            SPR_UI_Battle_PartyPanel = x;
            foreach (var Costume in CostumeAssets.Values)
                Costume.BattleSprIndex = -1;
        });
        this.toolkitObjects.OnObjectLoadedByName<USprAsset>("SPR_UI_Camp_PartyPanel", x =>
        {
            SPR_UI_Camp_PartyPanel = x;
            foreach (var Costume in CostumeAssets.Values)
                Costume.CampSprIndex = -1;
        });
        this.toolkitObjects.OnObjectLoadedByName<USprAsset>("SPR_UI_Field_PartyPanel", x =>
        {
            SPR_UI_Field_PartyPanel = x;
            foreach (var Costume in CostumeAssets.Values)
                Costume.FieldSprIndex = -1;
        });
    }
    
    private delegate void FBattleHeadPanel_UpdateState(FBattleHeadPanel* self, float a2, float a3, float a4, uint a5);
    
    private IHook<FBattleHeadPanel_UpdateState>? _FBattleHeadPanel_UpdateState;

    private void FBattleHeadPanel_UpdateStateImpl(FBattleHeadPanel* self, float a2, float a3, float a4, uint a5)
    {
        foreach (var costume in manager.GetCurrentCostumes()
                     .Where(x => x.Character == (Character)self->Super.PlayerId))
        {
            if (CostumeAssets.TryGetValue(costume.CostumeId, out var CurrentCostume)
                && CurrentCostume.BattleSprIndex != -1)
            {
                self->PortraitBaseId = CurrentCostume.BattleSprIndex;
                self->PortraitOutlineHigh = CurrentCostume.BattleSprIndex + 12;
            }
        }
        _FBattleHeadPanel_UpdateState!.OriginalFunction(self, a2, a3, a4, a5);
    }

    private delegate void FCampHeadPanel_SetPlayerTextureIndex(FBaseHeadPanel* self, int playerId, int panelState, nint a4);
    
    private IHook<FCampHeadPanel_SetPlayerTextureIndex>? _FCampHeadPanel_SetPlayerTextureIndex;

    private void FCampHeadPanel_SetPlayerTextureIndexImpl(FBaseHeadPanel* self, int playerId, int panelState,
        nint a4)
    {
        _FCampHeadPanel_SetPlayerTextureIndex!.OriginalFunction(self, playerId, panelState, a4);
        foreach (var costume in manager.GetCurrentCostumes()
                     .Where(x => x.Character == (Character)playerId))
        {
            if (CostumeAssets.TryGetValue(costume.CostumeId, out var CurrentCostume) 
                && CurrentCostume.CampSprIndex != -1)
                self->Portrait.SpriteIndex = CurrentCostume.CampSprIndex;
        }
    }
    
    private delegate void FFieldHeadPanel_SetPlayerTextureIndex(FFieldHeadPanel* self, int playerId, int panelState, nint a4);
    
    private IHook<FFieldHeadPanel_SetPlayerTextureIndex>? _FFieldHeadPanel_SetPlayerTextureIndexImpl;
    
    private void FFieldHeadPanel_SetPlayerTextureIndexImpl(FFieldHeadPanel* self, int playerId, int panelState,
        nint a4)
    {
        _FFieldHeadPanel_SetPlayerTextureIndexImpl!.OriginalFunction(self, playerId, panelState, a4);
        foreach (var costume in manager.GetCurrentCostumes()
                     .Where(x => x.Character == (Character)playerId))
        {
            if (!CostumeAssets.TryGetValue(costume.CostumeId, out var CurrentCostume) ||
                CurrentCostume.FieldSprIndex == -1) continue;
            self->Super.Portrait.SpriteIndex = CurrentCostume.FieldSprIndex;
            self->portraitShadow0.SpriteIndex = CurrentCostume.FieldSprIndex;
            self->portraitShadow1.SpriteIndex = CurrentCostume.FieldSprIndex;
        }
    }
    
    private static string MakeAssetPath(string path) => $"{path}.{path.Split("/")[^1]}";

    private void LoadPath(
        Costume costume, 
        string Path, 
        Func<CostumeHeadPanelAssets, nint> TexturePtrCb,
        Func<CostumeHeadPanelAssets, int> SpriteIndexCb,
        Action<CostumeHeadPanelAssets> OnLoadedCb)
    {
        var Target = TexturePtrCb(CostumeAssets[costume.CostumeId]);
        var SpriteIndex = SpriteIndexCb(CostumeAssets[costume.CostumeId]);
        if (Path == null) return;
        // Target is non-zero once asset is loaded for the first time.
        if (*(nint*)Target != nint.Zero)
        {
            // This would be -1 if the party panel file was loaded again (e.g Switching between Journey and Ep Aigis),
            // so call the OnLoadedCb again
            if (SpriteIndex == -1)
            {
                OnLoadedCb(CostumeAssets[costume.CostumeId]);
            }
            return;
        }
        var UPath = MakeAssetPath($"/Game/{Path.Replace('\\', '/')[..Path.LastIndexOf('.')]}");
        Log.Debug($"[Refresh] Costume {costume.CostumeId}: Path: {UPath}");
        assetLoader.LoadAsset(
            CostumeFrameworkAssetLoader!.Ptr, UPath, Target,
            x =>
            {
                toolkitObjects.GUObjectArray.AddToRootSet((*(UObjectBase**)x)->InternalIndex);
                OnLoadedCb(CostumeAssets[costume.CostumeId]);
            });
    }

    public void Refresh(Costume costume)
    {
        if (CostumeFrameworkAssetLoader == null)
            Log.Debug("CostumeHeadPanelService::Refresh: Create asset loader");
        CostumeFrameworkAssetLoader ??= toolkitSpawning.SpawnObject<UAssetLoader>("CostumeFrameworkAssetLoader", null);
        toolkitObjects.GUObjectArray.AddToRootSet(CostumeFrameworkAssetLoader!.InternalIndex);
        CostumeAssets.TryAdd(costume.CostumeId, new());
        assetLoader.CreateHandle(CostumeFrameworkAssetLoader.Ptr);
        LoadPath(
            costume, costume.Config.PartyPanel.BattlePath!, 
            x => (nint)x.GetBattleTexturePtr(),
            x => x.BattleSprIndex,
            CurrentCostume =>
            {
                if (CurrentCostume.BattleSprIndex != -1) return;
                var SprDatas = new TMapDictionary<HashableInt, FSprDataArray>(
                    (TMap<HashableInt, FSprDataArray>*)(&SPR_UI_Battle_PartyPanel!.Self->SprDatas), toolkitMemory);
                var SprArray = new TArrayList<FSprData>(&SprDatas.First().Value.Value->SprDatas, toolkitMemory); // this is always 1
                CurrentCostume.BattleSprIndex = SprArray.Count;
                for (var i = 0; i < 13; i++)
                    SprArray.AddValue(CreateBattleSprite(i, CurrentCostume.GetBattleTexture()));
            });
        LoadPath(costume, costume.Config.PartyPanel.CampPath!, 
            x => (nint)x.GetCampTexturePtr(),
            x => x.CampSprIndex,
            CurrentCostume =>
            {
                if (CurrentCostume.CampSprIndex != -1) return;
                var SprDatas = new TMapDictionary<HashableInt, FSprDataArray>(
                    (TMap<HashableInt, FSprDataArray>*)(&SPR_UI_Camp_PartyPanel!.Self->SprDatas), toolkitMemory);
                var SprArray = new TArrayList<FSprData>(&SprDatas.First().Value.Value->SprDatas, toolkitMemory); // this is always 1
                CurrentCostume.CampSprIndex = SprArray.Count;
                SprArray.AddValue(CreateSingleSprite(CurrentCostume.GetCampTexture()));
            });
        LoadPath(costume, costume.Config.PartyPanel.FieldPath!, 
            x => (nint)x.GetFieldTexturePtr(),
            x => x.FieldSprIndex,
            CurrentCostume =>
            {
                if (CurrentCostume.FieldSprIndex != -1) return;
                var SprDatas = new TMapDictionary<HashableInt, FSprDataArray>(
                    (TMap<HashableInt, FSprDataArray>*)(&SPR_UI_Field_PartyPanel!.Self->SprDatas), toolkitMemory);
                var SprArray = new TArrayList<FSprData>(&SprDatas.First().Value.Value->SprDatas, toolkitMemory); // this is always 1
                CurrentCostume.FieldSprIndex = SprArray.Count;
                SprArray.AddValue(CreateSingleSprite(CurrentCostume.GetFieldTexture()));
            });
        assetLoader.LoadQueuedAssets(CostumeFrameworkAssetLoader.Ptr);
    }
}