# 📘 WebHook Receiver - Podrobný návod k použití

Tento dokument obsahuje podrobný návod k použití všech aplikací v rámci projektu WebHook Receiver, včetně příkladů, nastavení a vysvětlení funkcí.

## 📋 Obsah

- [Přehled komponent](#přehled-komponent)
- [WebHookReceiverApi](#webhookreceiverapi)
  - [Instalace a spuštění](#instalace-a-spuštění-api)
  - [Zabezpečení API](#zabezpečení-api)
  - [Příklad volání API pomocí cURL](#příklad-volání-api-pomocí-curl)
  - [Struktura dat](#struktura-dat)
  - [SignalR klíč](#signalr-klíč)
- [WebHookNotifier (Windows)](#webhooknotifier-windows)
  - [Instalace a spuštění](#instalace-a-spuštění-windows-aplikace)
  - [Připojení k API](#připojení-k-api)
  - [Nastavení](#nastavení-windows-aplikace)
  - [Historie notifikací](#historie-notifikací-windows)
- [WebHookNotifierMaui (Android)](#webhooknotifiermaui-android)
  - [Instalace a spuštění](#instalace-a-spuštění-android-aplikace)
  - [Připojení k API](#připojení-k-api-android)
  - [Nastavení](#nastavení-android-aplikace)
  - [Historie notifikací](#historie-notifikací-android)
- [ApiKeyGenerator](#apikeygenerator)
  - [Instalace a spuštění](#instalace-a-spuštění-apikeygenerator)
  - [Použití](#použití-apikeygenerator)
- [Často kladené otázky](#často-kladené-otázky)

## Přehled komponent

WebHook Receiver se skládá z následujících komponent:

1. **WebHookReceiverApi** - ASP.NET Core Web API aplikace, která přijímá webhooky a přeposílá je klientům pomocí SignalR
2. **WebHookNotifier** - Windows desktopová aplikace (WPF), která zobrazuje notifikace na základě přijatých webhooků
3. **WebHookNotifierMaui** - Multiplatformní aplikace (.NET MAUI) pro Windows, Android, iOS a macOS
4. **ApiKeyGenerator** - Nástroj pro generování API klíčů pro zabezpečení API

## WebHookReceiverApi

WebHookReceiverApi je centrální komponenta, která přijímá webhooky od externích služeb a přeposílá je připojeným klientům v reálném čase pomocí SignalR.

### Instalace a spuštění API

1. Stáhněte si nejnovější verzi z [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Rozbalte soubor `WebHookReceiverApi-[verze].zip`
3. Spusťte aplikaci pomocí příkazu:

```bash
dotnet WebHookReceiverApi.dll
```

Nebo na Windows můžete spustit `WebHookReceiverApi.exe`.

API bude dostupné na adrese `http://localhost:5017`.

### Zabezpečení API

API je zabezpečeno pomocí API klíče. Každý požadavek na API musí obsahovat hlavičku `X-API-Key` s platným API klíčem.

API klíč je uložen v souboru `appsettings.json` v sekci `AppSettings.ApiKey`. Pro generování nového API klíče použijte nástroj ApiKeyGenerator.

Kromě API klíče pro přístup k API je také potřeba SignalR klíč pro připojení klientů k SignalR hubu. Tento klíč je uložen v souboru `appsettings.json` v sekci `AppSettings.SignalRKey`.

### Příklad volání API pomocí cURL

Zde je příklad, jak poslat webhook na API pomocí cURL:

```bash
curl -X POST "http://localhost:5017/api/webhook" \
     -H "Content-Type: application/json" \
     -H "X-API-Key: vaš-api-klíč-zde" \
     -d '{
           "event": "deployment",
           "message": "Aplikace byla úspěšně nasazena",
           "timestamp": "2025-04-14T12:00:00Z",
           "source": "CI/CD Pipeline",
           "severity": "info",
           "data": {
             "version": "1.0.0",
             "environment": "production",
             "duration": 120
           }
         }'
```

### Struktura dat

Webhook data mají následující strukturu:

```json
{
  "event": "string",       // Typ události (povinné)
  "message": "string",     // Zpráva (povinné)
  "timestamp": "string",   // Časové razítko ve formátu ISO 8601 (volitelné, výchozí je aktuální čas)
  "source": "string",      // Zdroj události (volitelné)
  "severity": "string",    // Závažnost (volitelné, možnosti: info, warning, error, critical)
  "data": {                // Dodatečná data (volitelné)
    "klíč1": "hodnota1",
    "klíč2": "hodnota2"
  }
}
```

### SignalR klíč

Pro připojení klientů k SignalR hubu je potřeba SignalR klíč. Tento klíč je přidán jako parametr v URL při připojování k hubu:

```
http://localhost:5017/notificationHub?signalRKey=váš-signalr-klíč-zde
```

SignalR klíč je uložen v souboru `appsettings.json` v sekci `AppSettings.SignalRKey`.

## WebHookNotifier (Windows)

WebHookNotifier je Windows desktopová aplikace, která zobrazuje notifikace na základě přijatých webhooků.

### Instalace a spuštění Windows aplikace

1. Stáhněte si nejnovější verzi z [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Rozbalte soubor `WebHookNotifier-[verze].zip`
3. Spusťte aplikaci `WebHookNotifier.exe`

Aplikace se spustí a zobrazí se v systémové liště.

### Připojení k API

1. Klikněte na ikonu aplikace v systémové liště pro zobrazení hlavního okna
2. Zadejte URL API (např. `http://localhost:5017/notificationHub`)
3. Zadejte SignalR klíč
4. Klikněte na tlačítko "Connect"

Po úspěšném připojení se zobrazí zpráva "Connected to [URL]" a aplikace začne přijímat notifikace.

### Nastavení Windows aplikace

Klikněte na "Settings" v hlavním okně nebo v kontextovém menu v systémové liště pro otevření okna s nastavením.

#### Nastavení notifikací

- **Minimum seconds between notifications** - Minimální počet sekund mezi zobrazením notifikací (pro omezení počtu notifikací)
- **Maximum queued notifications** - Maximální počet notifikací ve frontě (pro omezení počtu notifikací)
- **Enable notification sounds** - Povolení zvuků při zobrazení notifikace
- **Enable encryption** - Povolení šifrování dat mezi API a klientem

#### Nastavení historie

- **Enable history tracking** - Povolení sledování historie notifikací
- **Days to retain history** - Počet dní, po které se uchovává historie notifikací
- **Database type** - Typ databáze pro ukládání historie (SQLite nebo SQL Server)
- **Connection string** - Připojovací řetězec pro SQL Server (pouze pokud je vybrán SQL Server)

### Historie notifikací (Windows)

Klikněte na "View History" v hlavním okně nebo v kontextovém menu v systémové liště pro otevření okna s historií notifikací.

#### Funkce historie

- **Vyhledávání** - Vyhledávání v historii notifikací podle obsahu
- **Filtrování podle data** - Filtrování notifikací podle data
- **Filtrování podle typu události** - Filtrování notifikací podle typu události
- **Export do CSV** - Export historie do CSV souboru
- **Export do JSON** - Export historie do JSON souboru

## WebHookNotifierMaui (Android)

WebHookNotifierMaui je multiplatformní aplikace pro Windows, Android, iOS a macOS, která zobrazuje notifikace na základě přijatých webhooků.

### Instalace a spuštění Android aplikace

1. Stáhněte si nejnovější verzi z [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Nainstalujte APK soubor `WebHookNotifierMaui-[verze].apk` na vaše Android zařízení
3. Spusťte aplikaci "WebHook Notifier"

### Připojení k API (Android)

1. Na hlavní obrazovce zadejte URL API (např. `http://192.168.1.100:5017/notificationHub`)
   - Poznámka: Použijte IP adresu počítače, na kterém běží API, místo `localhost`
2. Zadejte SignalR klíč
3. Klikněte na tlačítko "Connect"

Po úspěšném připojení se zobrazí zpráva "Connected to [URL]" a aplikace začne přijímat notifikace.

### Nastavení Android aplikace

Klikněte na ikonu nastavení v navigačním menu pro otevření obrazovky s nastavením.

#### Nastavení notifikací

- **Minimum seconds between notifications** - Minimální počet sekund mezi zobrazením notifikací
- **Maximum queued notifications** - Maximální počet notifikací ve frontě
- **Enable notification sounds** - Povolení zvuků při zobrazení notifikace
- **Enable encryption** - Povolení šifrování dat mezi API a klientem

#### Nastavení připojení

- **Use direct WebSockets on Android** - Použití přímých WebSocketů místo SignalR na Androidu (může zlepšit výkon)

#### Nastavení historie

- **Enable history tracking** - Povolení sledování historie notifikací
- **Days to retain history** - Počet dní, po které se uchovává historie notifikací

### Historie notifikací (Android)

Klikněte na "History" v navigačním menu pro otevření obrazovky s historií notifikací.

#### Funkce historie

- **Vyhledávání** - Vyhledávání v historii notifikací podle obsahu
- **Filtrování podle data** - Filtrování notifikací podle data
- **Filtrování podle typu události** - Filtrování notifikací podle typu události
- **Sdílení** - Sdílení vybrané notifikace

## ApiKeyGenerator

ApiKeyGenerator je nástroj pro generování API klíčů pro zabezpečení API.

### Instalace a spuštění ApiKeyGenerator

1. Stáhněte si nejnovější verzi z [GitHub Releases](https://github.com/Michal1609/WebHookReceiver/releases)
2. Rozbalte soubor `ApiKeyGenerator-[verze].zip`
3. Spusťte aplikaci `ApiKeyGenerator.exe`

### Použití ApiKeyGenerator

1. Zadejte cestu k souboru `appsettings.json` (výchozí je `../WebHookReceiverApi/appsettings.json`)
2. Klikněte na tlačítko "Generate API Key"
3. Nový API klíč bude vygenerován a uložen do souboru `appsettings.json`
4. Klíč bude také uložen do souboru `apikey.txt` pro pozdější použití

## Často kladené otázky

### Jak změnit port API?

Port API můžete změnit v souboru `WebHookReceiverApi/Properties/launchSettings.json` v sekci `profiles.WebHookReceiverApi.applicationUrl`.

### Jak zabezpečit komunikaci pomocí HTTPS?

Pro zabezpečení komunikace pomocí HTTPS je potřeba:

1. Vygenerovat SSL certifikát
2. Nakonfigurovat API pro použití HTTPS v souboru `WebHookReceiverApi/Properties/launchSettings.json`
3. Aktualizovat URL v klientských aplikacích na `https://`

### Jak řešit problémy s připojením na Androidu?

1. Ujistěte se, že používáte správnou IP adresu počítače, na kterém běží API
2. Zkontrolujte, zda je port API (výchozí 5017) otevřený ve firewallu
3. Zkuste povolit "Use direct WebSockets on Android" v nastavení aplikace
4. Zkontrolujte, zda máte správný SignalR klíč

### Jak přidat vlastní typ notifikace?

Pro přidání vlastního typu notifikace:

1. Upravte strukturu dat v požadavku na API podle potřeby
2. Upravte zpracování notifikací v klientských aplikacích podle potřeby

### Jak změnit vzhled notifikací?

Vzhled notifikací můžete upravit:

1. V Windows aplikaci v souboru `WebHookNotifier/MainWindow.xaml.cs` v metodě `FormatNotificationMessage`
2. V Android aplikaci v souboru `WebHookNotifierMaui/Views/NotificationPage.xaml`
