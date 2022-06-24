cd "C:\Users\Snowy\source\repos\SnowyBot\bin\Debug\net6.0"
ren config.json config.tmp
xcopy /v /h /k /s /i /e /y "C:\Users\Snowy\source\repos\SnowyBot\bin\Debug\net6.0" "C:\Users\Snowy\source\repos\SnowyBot\StableRelease"
ren config.tmp config.json