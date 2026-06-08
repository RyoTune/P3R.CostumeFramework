local _ModActor = nil

print("Loading CostumeFramework Lua Component")


RegisterCustomEvent("CostuemFrameworkLuaFunctionStartNya", function (ModActor)
    if ModActor:get() ~= nil and ModActor:get():IsValid() then
        _ModActor = ModActor:get()
        print("CostumeFramework Lua Component loaded successfully")
    end
end)


NotifyOnNewObject("/Script/xrd777.BtlActor", function(BtlActor)
    _ModActor:CFBattleStartTriggerNyam(BtlActor)

end)