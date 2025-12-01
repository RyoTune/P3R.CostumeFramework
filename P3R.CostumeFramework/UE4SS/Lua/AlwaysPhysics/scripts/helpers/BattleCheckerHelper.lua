local BTL_GUI_CORE_CLASS_NAME = "BP_BtlGuiCore_C"

local cached_battle_gui_core = nil
local GUI_PARTY_PANEL_PROPERTY = "PartyPanelVisible"

local function coerce_party_panel_visible(value)
    if type(value) == "boolean" then
        return value
    elseif type(value) == "number" then
        return value ~= 0
    end

    return value ~= nil
end

local function is_valid_object(object)
    return object and object:IsValid() and not object:HasAnyInternalFlags(EInternalObjectFlags.PendingKill)
end

local function find_battle_gui_core()
    if is_valid_object(cached_battle_gui_core) then
        return cached_battle_gui_core
    end

    cached_battle_gui_core = nil

    local actors = FindAllOf(BTL_GUI_CORE_CLASS_NAME)
    if not actors then
        return nil
    end

    for _, actor in ipairs(actors) do
        if is_valid_object(actor) then
            cached_battle_gui_core = actor
            return actor
        end
    end

    return nil
end

local M = {}

function M.get_battle_gui_core()
    local gui_core = find_battle_gui_core()
    if not is_valid_object(gui_core) then
        return nil
    end
    return gui_core
end

function M.BATTLE_CHECK()
    local gui_core = M.get_battle_gui_core()
    if not gui_core then
        return false
    end

    local party_panel_visible = gui_core[GUI_PARTY_PANEL_PROPERTY]
    if party_panel_visible == nil then
        return true
    end

    return coerce_party_panel_visible(party_panel_visible)
end

return M
