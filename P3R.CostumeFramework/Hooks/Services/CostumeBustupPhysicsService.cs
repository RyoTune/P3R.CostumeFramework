using P3R.CostumeFramework.Costumes.Models;
using Unreal.ObjectsEmitter.Interfaces;

namespace P3R.CostumeFramework.Hooks.Services;

internal sealed class CostumeBustupPhysicsService : PoseTableServiceBase
{
    public CostumeBustupPhysicsService(IDataTables dt, IUnreal unreal, CostumeManager manager, CostumeHooks hooks)
        : base(dt, unreal, manager, hooks, "LabubuJigglePhysics", "battle physics") { } // rip labubu, gomenasorry.....

    protected override string GetPoseValue(Costume costume, string rowName)
    {
        var raw = costume.Config.BattlePhysics;
        var enabled = raw ?? false;
        Log.Information($"Battle physics config for {rowName}: raw={raw?.ToString() ?? "null"}, applied={enabled}.");
        return enabled ? "True" : "False";
    }
}
// haudhwbiuwabndiuba