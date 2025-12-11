local UEHelpers        = require("UEHelpers")
local conductor_helper = require("helpers.btl_main_conductor")

local MOD_PREFIX = "[PhysicsCallMod]"

local BTL_GUI_CORE_CLASS_NAME = "BP_BtlGuiCore_C"

local ENCOUNTER_POLL_DELAY_MS = 100


local function get_script_dir()
    local info = debug and debug.getinfo and debug.getinfo(1, "S")
    local source = info and info.source or ""
    if source:sub(1, 1) == "@" then
        source = source:sub(2)
    end
    local dir = source:match("^(.*[\\/])")
    return dir or ""
end

local SCRIPT_DIR = get_script_dir()

local function log(msg)
    print(string.format("%s %s\n", MOD_PREFIX, tostring(msg)))
end

local function write_battlecheck_file(value)
    local path = SCRIPT_DIR .. "battlecheck.txt"

    local file, err = io.open(path, "w")
    if not file then
        log("Failed to open battlecheck.txt for writing: " .. tostring(err))
        return
    end

    file:write(value and "true" or "false")
    file:close()
end

local last_battlecheck_value = nil
local function update_battlecheck_value(value)
    if last_battlecheck_value == value then
        return
    end
    last_battlecheck_value = value
    write_battlecheck_file(value)
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

local function poll_encounter_and_maybe_fire()
    local encounter, err = conductor_helper.get_encounter_details()

    local enc_id = encounter and encounter.EncountID or nil
    local battle_detected = false

    if encounter and enc_id and enc_id ~= 0 and is_battle_ui_ready() then
        battle_detected = true
    end

    update_battlecheck_value(battle_detected)
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
