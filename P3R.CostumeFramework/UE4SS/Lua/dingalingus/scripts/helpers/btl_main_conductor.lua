local BTL_MAIN_CONDUCTOR_CLASS_NAME = "BP_BtlMainConductor_C"

local cached_conductor = nil

local function is_valid_actor(actor)
    return actor and actor:IsValid() and not actor:HasAnyInternalFlags(EInternalObjectFlags.PendingKill)
end

local function find_main_conductor()
    if is_valid_actor(cached_conductor) then
        return cached_conductor
    end

    cached_conductor = nil

    local actors = FindAllOf(BTL_MAIN_CONDUCTOR_CLASS_NAME)
    if not actors then
        return nil
    end

    for _, actor in ipairs(actors) do
        if is_valid_actor(actor) then
            cached_conductor = actor
            return actor
        end
    end

    return nil
end

local function get_encount_param(conductor)
    if not conductor then
        return nil
    end

    return conductor["Encount Parameter"]
        or conductor.EncountParameter
        or conductor.EncountParam
end

local M = {}

function M.get_encounter_details()
    local conductor = find_main_conductor()
    if not conductor then
        return nil, "No valid BP_BtlMainConductor actor was found."
    end

    local encount_param = get_encount_param(conductor)
    if not encount_param then
        return nil, "Unable to read Encount parameters from BP_BtlMainConductor."
    end

    return {
        EncountID = encount_param.EncountID,
        StageMajor = encount_param.StageMajor,
        StageMinor = encount_param.StageMinor,
    }
end

return M