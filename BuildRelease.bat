cd "C:\Users\Snowy\Documents\My Games\Terraria\ModLoader\Mod Sources\SnowyBotCSharp\bin\Debug\net6.0"
ren config.json config.tmp
xcopy /v /h /k /s /i /e /y "C:\Users\Snowy\Documents\My Games\Terraria\ModLoader\Mod Sources\SnowyBotCSharp\bin\Debug\net6.0" "C:\Users\Snowy\Documents\My Games\Terraria\ModLoader\Mod Sources\SnowyBotCSharp\StableRelease"
ren config.tmp config.json