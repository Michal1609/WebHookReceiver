# WebHook Receiver

Tento projekt se skládá ze tří aplikací, které spolu komunikují v reálném čase:

1. **WebHookReceiverApi** - ASP.NET Core Web API aplikace, která přijímá webhooky a předává je klientům pomocí SignalR
2. **WebHookNotifier** - Windows aplikace, která zobrazuje notifikace na základě přijatých webhooků
3. **ApiKeyGenerator** - Nástroj pro generování API klíčů pro zabezpečení API

## Technologie

- .NET 9
- ASP.NET Core Web API
- SignalR pro real-time komunikaci
- WPF pro Windows aplikaci
- Hardcodet.NotifyIcon.Wpf pro systémovou lištu

## Struktura projektu

- **WebHookReceiverApi/** - API projekt
  - **Controllers/** - Controllery pro API
  - **Hubs/** - SignalR huby
  - **Middleware/** - Middleware pro ověřování API klíče
  - **Models/** - Datové modely
  - **wwwroot/** - Statické soubory (včetně testovací HTML stránky)

- **WebHookNotifier/** - Windows aplikace
  - **Models/** - Datové modely
  - **Services/** - Služby pro komunikaci a notifikace
  - **Resources/** - Ikony a další zdroje

- **ApiKeyGenerator/** - Nástroj pro generování API klíčů
  - Generuje bezpečné API klíče
  - Aktualizuje konfigurační soubor API
  - Ukládá vygenerovaný klíč do souboru

## Instalace a spuštění

### Požadavky

- .NET 9 SDK
- Windows (pro klientskou aplikaci)

### Spuštění API

```bash
cd WebHookReceiverApi
dotnet run
```

API bude dostupné na adrese `http://localhost:5017`.

### Spuštění Windows aplikace

```bash
cd WebHookNotifier
dotnet run
```

## Použití

### Zabezpečení API klíčem

API je zabezpečeno pomocí API klíče. Pro generování nového API klíče použijte nástroj ApiKeyGenerator:

```bash
cd ApiKeyGenerator
dotnet run
```

Tento nástroj vygeneruje nový API klíč a aktualizuje konfigurační soubor API. Vygenerovaný klíč je také uložen do souboru `apikey.txt`.

Při volání API je nutné přidat hlavičku `X-API-Key` s platnou hodnotou API klíče.

### Testování webhooků

1. Spusťte API projekt
2. Otevřete v prohlížeči `http://localhost:5017/test.html`
3. Vyplňte formulář včetně API klíče a odešlete webhook
4. Spusťte Windows aplikaci a připojte se k API
5. Po odeslání webhooků se zobrazí notifikace v systémové liště

### Konfigurace

#### API

- Porty a další nastavení lze upravit v souboru `WebHookReceiverApi/Properties/launchSettings.json`
- API klíč je uložen v souboru `WebHookReceiverApi/appsettings.json` v sekci `AppSettings.ApiKey`

#### Windows aplikace

- URL API lze nastavit v aplikaci

## Vývoj

### Přidání nových typů webhooků

1. Upravte model `WebhookData` podle potřeby
2. Implementujte zpracování nových typů v `WebhookController`

### Úprava notifikací

Úpravy zobrazení notifikací lze provést v třídě `NotificationService` v projektu Windows aplikace.

## Testování

Projekt obsahuje jednotkové testy pro API. Spuštění testů:

```bash
cd WebHookReceiverApiTests
dotnet test
```

## Licence

Tento projekt je licencován pod MIT licencí.
