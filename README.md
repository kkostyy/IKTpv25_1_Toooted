# IKTpv25_1_Toooted

**AKT** – WinForms (.NET 8) + EF Core 8: toodete haldus, kassa, e-pood, kliendid ja statistika.

## Käivitamine

1. Ava `IKTpv25_1_Toooted.slnx` Visual Studios. NuGet-paketid taastatakse ise
   (EF Core 8.0.6 + SQLite, PdfSharpCore, BCrypt.Net-Next).
2. Package Manager Console (Default project: `IKTpv25_1_Toooted`):

   ```
   Add-Migration InitialCreate
   ```

3. F5. Andmebaas `AppData\Local\ToodedApp\tooded.db` (SQLite) tekib ise koos
   7 kategooria, 69 toote ja 2 kasutajaga. E-poe toodete pildid laaditakse esimesel avamisel internetist
   ja hoitakse kaustas `AppData\Local\ToodedApp\Images` (`db.Database.Migrate()` failis `Program.cs`).

Kui mudeleid muudetakse: `Add-Migration <UusNimi>` ja F5 (andmed jäävad alles). Kui migratsioon on tegemata, ütleb programm seda käivitamisel.

## Kasutajad

| Kasutaja | Parool | Roll | Näeb |
|---|---|---|---|
| admin | admin123 | Omanik | Tooted + Kassa, Kliendid, Statistika, kasutajate registreerimine |
| myyja | myyja123 | Müüja | ainult Kassa |
| (ise registreeritud) | | Ostja | ainult e-pood |

Sisselogimisaknas „Pole kontot? Registreeru“: **ostja** (klient) või müüja. Ostja sisestab e-posti ja saab
**kohe kliendikaardi**; maksmisel on kaart ja e-post juba täidetud (e-posti saab seal ja Seadetes muuta).
Omaniku konto saab luua ainult omanik (Haldus → **Kasutajad**: eraldi tabel kõigist kontodest – roll, e-post,
kliendikaart, uus parool, kustutamine).

- **Seaded** (Kasutaja → Seaded, nupp päises, poes ⚙ või klõps kasutaja nimel): nimi, e-post, kliendikaart,
  parool, teema, poe värv, disain, keel.
- **Ostunimekiri** (Kasutaja → Ostunimekiri / Ctrl+L, nupp päises ja poes, toote lehel „Lisa ostunimekirja“):
  linnuke = ostetud, filter Kõik / Ostmata / Ostetud, „Lisa ostmata korvi“. Ostu järel märgitakse ostetud
  tooted ise. Andmed tabelis `Ostunimekiri`.
- Kassas „+ Uus klient (kaart kohe)“ – uus klient saab kaardinumbri automaatselt (ka Kliendid → Lisa tühja kaardiga).

- **Boonus**: iga tootele kulutatud 1 € annab 1 sendi (1 %). Boonusega saab maksmisel tasuda („Kasuta boonust“).
- **Kliendikaart** (külgmenüü / Seaded → Ava kaart): avaneb nagu iPhone'i Wallet – kaart tõuseb alt, klõps pöörab
  ümber (taga päris EAN-13 vöötkood), all viimased ostud ja boonuse muutus. Esc sulgeb.
- **Kohaletoimetamine**: kojuvedu 3,99 €, ostukorvist alates 25 € tasuta (poes riba „Lisa veel X € …“);
  „Poest kaasa“ on alati tasuta. Aadress Seadetes (ja jääb esimesel tellimusel meelde).
- **Tšekk**: kitsas 80 mm kassatšekk (PDF) – tooted, soodustused, „Oled säästnud“, makse, boonus, käibemaksu tabel.
  E-kiri HTML-ina: bänner „Sinu ostutšekk“, tervitus, ostu kokkuvõte, PDF manuses. Seadetes „Soovin ostutšekke e-postile“.
- **Ümarad nurgad ja külgmenüü** (Seaded → Välimus): ümar „pill“-menüü paremas servas (peaaken ja e-pood),
  märgid (korvis, ostunimekirjas), all väljumine ja kasutaja ring.

Pärast uuendusi: Package Manager Console → `Add-Migration Ostjad` (kui tegemata) ja `Add-Migration Tarne` → F5.

## Kassa

- **Valin** – lisab tabelis valitud toote ostukorvi (ka topeltklõps tabelis). Laos olevast rohkem ei saa.
- **Pood** – tooted plaatidena koos piltidega, kategooriate filter; klõps plaadil lisab korvi.
- **Ostan** – küsib kliendikaarti (võib jätta tühjaks), pakub boonusega allahindlust,
  vähendab laoseisu, salvestab ostu. Kliendile lisandub 5 % ostusummast boonuseks.
- **Ostan** küsib ka kliendi e-posti (kui kliendikaardil seda pole).
- **Maksta** – makse ja tagasiraha, PDF-tšekk kausta `AppData\Local\ToodedApp\Arved`
  ja **automaatselt kliendi e-postile** (PDF manusena).
- **Saada arve** – sama tšekk uuesti mis tahes aadressile.

### E-post (juhend 4.2)

Omanik: menüü **Haldus → E-posti seaded** → Gmaili aadress + 16-märgiline **rakenduse parool**
→ Salvesta → Saada testkiri. Parool hoitakse krüpteeritult failis
`AppData\Local\ToodedApp\email.json`, mitte koodis, seega GitHubi see ei satu.

## Kasutajaliides

- **Ctrl+Shift+D** (igas aknas) või **Vaade → E-poe disain / Klassikaline disain** – vahetab disaini:
  e-poe (Rimi) stiil ↔ esimene, klassikaline (sinine, Wordi-laadne) kujundus. Avatud pood vahetub kohe.

- **Keel: Eesti / Русский / English** – lingid lilla riba paremas servas (peaaken, pood, sisselogimine)
  või **Vaade → Keel**. Tõlgitakse menüüd, nupud, tabelite päised, teated, PDF-tšekk ja e-kiri;
  valik jääb meelde (`AppData\Local\ToodedApp\keel.json`). Toodete nimed jäävad andmebaasi järgi.
- **Vaade → Hele / Tume teema** ja **Poe värv** (roheline, sinine, oranž, lilla); valik jääb meelde
  (`AppData\Local\ToodedApp\teema.json`). Teema saab vahetada ka sisselogimisaknas.
- **Pood** e-poe veebilehe stiilis: lilla riba, päis (logo, otsing, kasutaja, ostukorvi nupp), menüüriba
  (Tooted, Kampaaniad, Tõesti odav!, Minu lemmikud), avalehel riiulid („Parimad pakkumised“, kategooriad,
  „Näita kõiki“, kerimisnool), salvestatud ostukorvid, lemmikud ☆.
  Vasakul kategooriate ikoonid, tootekaardid
  (suur hind, soodushind läbikriipsutatud vana hinnaga, €/kg, nupp „Lisa ostukorvi“ → korvis „− N tk +“),
  paremal „Sinu ostukorv“ (Tooteid, Kokku, Maksma, Tühjenda ostukorv).
- Klõps tootel avab **toote lehe**: pilt, hind, „Lisa ostukorvi“, „Veel tooteid kaubamärgilt …“ ja
  „Toote andmed“ (Tooteinfo: päritolumaa, kaubamärk, tootja, kogus, kirjeldus, koostisosad, lisainfo,
  toitumisalane teave; Soovitatud tooted).
- Tooted vahelehel nupp **Tooteinfo…** – toote lisaandmete muutmine.
- **Vormista ost** – üks aken: kliendikaart + boonus, e-post tšeki jaoks, sularaha/kaart, tagasiraha.
- Teated programmi stiilis, väikesed hüpikteated („Lisatud korvi“, „Ost makstud“).

## Oma idee

- Väikese laoseisuga (alla 10 tk) tooted on tabelis punased, ostu järel tuleb hoiatus.
- Statistika: müük täna / kokku, ostude arv, väike laoseis, enim müüdud tooted.
- Boonuskaardi soodustus.
- Toodete otsing nime ja kategooria järgi.
- Poes saab valida koguse.
- Ostude ajalugu: tšeki korduv printimine, tagastus (kaup tagasi lattu, boonus taastatakse).
- Müügigraafik 14 päeva kohta, CSV eksport (tooted, müük), parooli muutmine.

## Installer (.msi) – juhend osa 7

Andmebaas on SQLite (juhend 7.1 variant B), seega teises arvutis pole LocalDB-d vaja.
Publish-profiil on valmis: `Properties/PublishProfiles/FolderProfile.pubxml`
(Release, win-x64, **Self-contained** – .NET 8 on kaasas, eeldusi pole vaja).

1. Extensions → Manage Extensions → **Microsoft Visual Studio Installer Projects 2022** → Install,
   sulge ja ava Visual Studio uuesti.
2. Paremklõps Solutionil → Add → New Project → **Setup Project** → nimi `ToodedInstaller`.
   Properties: Manufacturer = `TTHK`, ProductName = `Tooded App`.
3. File System → Application Folder → paremklõps → Add → **Project Output…** →
   Project: `IKTpv25_1_Toooted` → **Publish Items** → OK.
   Vali „Publish Items“ → Properties → **PublishProfilePath** → `Properties\PublishProfiles\FolderProfile.pubxml`.
4. Application Folder → paremklõps → Add → File… → `IKTpv25_1_Toooted\Ikoon.ico`.
5. User's Programs Menu → Add → Folder `Tooded App` → selle sees Add → Shortcut →
   Application Folder → Publish Items from IKTpv25_1_Toooted. Otsetee Properties: Name `Tooded App`, Icon → `Ikoon.ico`.
6. User's Desktop → Add → Shortcut → sama sihtmärk ja ikoon.
7. Ülal Configuration **Release** → paremklõps `ToodedInstaller` → **Build**.
   Kaustas `ToodedInstaller\Release` on `ToodedInstaller.msi` ja `setup.exe`.

Testi teises arvutis või virtuaalmasinas: otseteest käivitub sisselogimisaken, esimesel käivitamisel
tekib andmebaas koos kategooriate, toodete ja kahe kasutajaga.
