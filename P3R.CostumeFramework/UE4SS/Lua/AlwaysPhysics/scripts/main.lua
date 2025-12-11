local UEHelpers        = require("UEHelpers")
local conductor_helper = require("helpers.btl_main_conductor")

local MOD_PREFIX = "[PhysicsCallMod]"

local WIDGET_CLASS_NAME = "physicwidget_C"
local WIDGET_CLASS_PATH = "/Game/Mods/Physics/physicwidget.physicwidget_C"

local BTL_GUI_CORE_CLASS_NAME = "BP_BtlGuiCore_C"

local ENCOUNTER_POLL_DELAY_MS = 250

local ENCOUNTER_DELAY_TICKS   = 8   -- 8 * 250ms = 2000ms

local last_encounter_id    = nil      
local cached_widget        = nil       

local cached_btl_gui_core  = nil      

local pending_encounter_id = nil       
local pending_delay_ticks  = 0         


local function log(msg)
    print(string.format("%s %s\n", MOD_PREFIX, tostring(msg)))
end

local function is_valid_object(object)
    return object
       and object.IsValid
       and object:IsValid()
       and not object:HasAnyInternalFlags(EInternalObjectFlags.PendingKill)
end

local function get_btl_gui_core()
    if is_valid_object(cached_btl_gui_core) then
        return cached_btl_gui_core
    end

    cached_btl_gui_core = nil

    local actors = FindAllOf(BTL_GUI_CORE_CLASS_NAME)
    if not actors then
        return nil
    end

    for _, actor in ipairs(actors) do
        if is_valid_object(actor) then
            cached_btl_gui_core = actor
            break
        end
    end

    return cached_btl_gui_core
end

local function coerce_party_panel_visible(value)
    if type(value) == "boolean" then
        return value
    elseif type(value) == "number" then
        return value ~= 0
    end

    return value ~= nil
end

local function is_battle_ui_ready()
    local guiCore = get_btl_gui_core()
    if not is_valid_object(guiCore) then
        return false
    end

    local partyPanelVisible = coerce_party_panel_visible(guiCore.PartyPanelVisible)
    return partyPanelVisible
end

local function find_widget_instance()
    if is_valid_object(cached_widget) then
        return cached_widget
    end

    cached_widget = nil

    local widgets = FindAllOf(WIDGET_CLASS_NAME)
    if widgets then
        for _, widget in ipairs(widgets) do
            if is_valid_object(widget) then
                cached_widget = widget
                return cached_widget
            end
        end
    end

    return nil
end

local function call_widget_event(eventName)
    local widget = find_widget_instance()
    if not widget then
        log("physicwidget not found yet; will retry next time")
        return false
    end

    local attemptedNames = { eventName }
    local compactName    = eventName:gsub("%s+", "")
    if compactName ~= eventName then
        table.insert(attemptedNames, compactName)
    end

    local underscoredName = eventName:gsub("%s+", "_")
    if underscoredName ~= eventName and underscoredName ~= compactName then
        table.insert(attemptedNames, underscoredName)
    end

    local lastError = nil

    for _, name in ipairs(attemptedNames) do
        local fn = widget[name]
        if fn and fn.IsValid and fn:IsValid() then
            local ok, result = pcall(fn)
            if ok then
                --log("Widget event '" .. name .. "' called successfully.")
                return true
            end
            lastError = result
        else
            lastError = string.format("function '%s' is not valid", name)
        end
    end

    --log("Failed to invoke " .. eventName .. " on physicwidget: " .. tostring(lastError))
    return false
end

local function poll_encounter_and_maybe_fire()
    local encounter, err = conductor_helper.get_encounter_details()
    if not encounter then
        return
    end

    local enc_id = encounter.EncountID
    if not enc_id or enc_id == 0 then
        return
    end

    if not is_battle_ui_ready() then
        return
    end

    if last_encounter_id == enc_id then
        return
    end

    if pending_encounter_id and pending_encounter_id ~= enc_id then
        pending_encounter_id = nil
        pending_delay_ticks  = 0
    end

    if not pending_encounter_id then
        pending_encounter_id = enc_id
        pending_delay_ticks  = ENCOUNTER_DELAY_TICKS
        log("New encounter detected (EncountID=" .. tostring(enc_id) ..
            "); UI ready. Will call physicwidget.PhysicsCall in ~2 seconds.")
        return
    end

    if pending_encounter_id == enc_id then
        if pending_delay_ticks > 0 then
            pending_delay_ticks = pending_delay_ticks - 1
            return
        end

        --log("2-second delay elapsed for EncountID=" .. tostring(enc_id) ..
            --"; trying to call physicwidget.PhysicsCall now.")

        local ok = call_widget_event("PhysicsCall")
        if ok then
            last_encounter_id    = enc_id
            pending_encounter_id = nil
            pending_delay_ticks  = 0
        else

        end
    end
end

local function start_encounter_polling()
    if type(LoopAsync) ~= "function" then
        log("LoopAsync is unavailable; encounter polling disabled.")
        return
    end

    LoopAsync(ENCOUNTER_POLL_DELAY_MS, function()
        ExecuteInGameThread(function()
            local ok, err = pcall(poll_encounter_and_maybe_fire)
            if not ok then
               -- log("Error in encounter poll: " .. tostring(err))
            end
        end)
    end)
end

start_encounter_polling()

if type(RegisterKeyBind) == "function" then
    RegisterKeyBind(Key.F6, function()
        ExecuteInGameThread(function()
            local ok, err = pcall(function()
                call_widget_event("PhysicsCall")
            end)

            if not ok then
            end
        end)
    end)
else
end