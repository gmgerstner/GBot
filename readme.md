# GBot

## Description

This is a windows service that keeps GoDaddy up to date with the current IP Address assigned by Google Fiber.

## Install the Service

- Open an elevated command prompt (Run as administrator).
- Use the following command, replacing placeholders with your values:
```cmd
sc create "GBot Service" binPath= "D:\Programs\GBot\GMG.GBot.GBotWorkerService.exe" start= auto
```

## Update Configuration Values

| Setting | Description |
|---------|-------------|
| ConnectionStrings:LogDB			| Logging Database (Must use the table Log as per Serilog |
| IPAddressProvider:RequestUri		| The url of the IP Address provider (eg. https://api.ipify.org/?format=text) |
| GBotService:Token | RequestUri	| Discord Service Token |
| GBotService:GeneralChannelID		| Discord Service General Channel ID |
| GBotService:DNSName				| DNS Name to be monitored |
| GBotService:RootRecord			| DNS RootRecord  (eg. @)|
| GBotService:UseGoDadddyProvider	| Use the GoDadddy Provider  |
| GBotService:UseDNSLookupProvider	| Use the DNSLookup Provider  |
| GoDaddyProvider:GoogleApiKey		| GoDaddy GoogleApiKey |
| GoDaddyProvider:GoogleApiSecret	| GoDaddy GoogleApiSecret|
| GoDaddyProvider:BaseURL			| GoDaddy API BaseURL|



## Startup the Service
```cmd
sc start "GBot Service"
```