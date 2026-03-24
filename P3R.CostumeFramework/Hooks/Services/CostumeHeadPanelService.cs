using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Types;
using Reloaded.Hooks.Definitions;

namespace P3R.CostumeFramework.Hooks.Services;

internal unsafe class CostumeHeadPanelService
{
    private readonly CostumeManager manager;
    
    public CostumeHeadPanelService(CostumeManager manager)
    {
        this.manager = manager;
        ScanHooks.Add(
            nameof(FCampHeadPanel_SetPlayerTextureIndex),
            "4C 8B DC 49 89 4B ?? 55 53 49 8D 6B ??",
            (hooks, result) => _FBattleHeadPanel_UpdateState = hooks
                .CreateHook<FBattleHeadPanel_UpdateState>(FBattleHeadPanel_UpdateStateImpl, result)
                .Activate()
        );
        ScanHooks.Add(
            nameof(FCampHeadPanel_SetPlayerTextureIndex),
            "48 89 5C 24 ?? 57 48 83 EC 30 33 FF 89 51 ??",
            (hooks, result) => _FCampHeadPanel_SetPlayerTextureIndex = hooks
                .CreateHook<FCampHeadPanel_SetPlayerTextureIndex>(FCampHeadPanel_SetPlayerTextureIndexImpl, result)
                .Activate()
        );
        
        ScanHooks.Add(
            nameof(FFieldHeadPanel_SetPlayerTextureIndex),
            "40 53 48 83 EC 20 44 89 41 ??",
            (hooks, result) => _FFieldHeadPanel_SetPlayerTextureIndexImpl = hooks
                .CreateHook<FFieldHeadPanel_SetPlayerTextureIndex>(FFieldHeadPanel_SetPlayerTextureIndexImpl, result)
                .Activate()
        );
    }
    
    private delegate void FBattleHeadPanel_UpdateState(FBattleHeadPanel* self, float a2, float a3, float a4, uint a5);
    
    private IHook<FBattleHeadPanel_UpdateState>? _FBattleHeadPanel_UpdateState;

    private void FBattleHeadPanel_UpdateStateImpl(FBattleHeadPanel* self, float a2, float a3, float a4, uint a5)
    {
        foreach (var costume in manager.GetCurrentCostumes()
                     .Where(x => x.Character == (Character)self->Super.PlayerId))
        {
            if (costume.CostumeId == 1000)
            {
                // Log.Debug($"Current PortraitBaseId: {self->PortraitBaseId}");
                self->PortraitBaseId = 100;
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
            if (costume.CostumeId == 1000)
            {
                self->Portrait.SpriteIndex = 15;
            }
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
            if (costume.CostumeId == 1000)
            {
                self->Super.Portrait.SpriteIndex = 15;
                self->portraitShadow0.SpriteIndex = 15;
                self->portraitShadow1.SpriteIndex = 15;
            }
        }
    }
}