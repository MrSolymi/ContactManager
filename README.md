# ContactManager (Kontakt Kezelő) 

[🇬🇧 English version](README.en.md)

A **ContactManager** egy WPF asztali alkalmazás, amely Gmail-fiókból származó pályázói emailek alapján automatikusan kinyeri a jelöltek adatait, és egy lokális SQLite adatbázisban kezeli őket. Az app célja, hogy megkönnyítse a HR / toborzó csapat munkáját: egy helyen látható, szűrhető és státuszozható minden jelentkező.

## Fő funkciók

- **Gmail bejelentkezés (OAuth)**  
  - Google-fiókkal történő bejelentkezés.
  - A bejelentkezett fiók email címe megjelenik a fejlécben.

- **.eml csatolmányok letöltése és feldolgozása**
  - A megadott *feladó* email címről érkező levelek keresése.
  - Csak a csatolmányos emaileket nézi, és azokból menti le a `.eml` fájlokat.
  - A `.eml` fájlokból (HTML / szöveges body) automatikusan kinyeri:
    - nevet
    - telefonszámot
    - email címet
    - a jelentkezés dátumát

- **Kontaktok mentése SQLite adatbázisba**
  - EF Core alapú `contacts.db` SQLite adatbázis.
  - Telefon / email / név normalizálás (whitespace, kisbetűs email, +36 → 06 stb.).
  - Duplikátum-kezelés: azonos Név + Telefon + Email kombináció csak egyszer kerül be.
  - Automatikus adatbázis-migráció az új `IsForeign` oszlophoz.

- **Adatkezelés a felületen**
  - Kontaktok listázása DataGridben.
  - Többszörös kijelölés támogatása (custom `SelectedItemsBehavior`).
  - **Email címek másolása vágólapra** egy gombnyomással (kijelölt sorok alapján).
  - **Külföldi jelölés**:
    - Egy kijelölt rekordnál kapcsolható az `IsForeign` mező.
    - A táblázatban "Külföldi" oszlop: ha `true`, akkor "külföldi", ha `false`, üres.
    - Szűrés: "Csak külföldiek" checkbox.

- **Státuszkezelés (HR workflow)**
  - `Megjelent` / `Nem jelent meg` (Published)
  - `Felvett` / `Visszautasítva` (Hired)
  - A gombok az aktuális kijelölés állapotához igazodnak (nem engedi ellentmondásos tömegműveleteket).
  - "Hiányos kontaktok" szűrő: azok jelennek meg, ahol még nincs eldöntve, hogy megjelent / felvett-e.

- **Szűrés és keresés**
  - Név szerinti szűrés
  - Telefonszám szerinti szűrés
  - Email cím szerinti szűrés
  - Jelentkezés dátuma szerinti intervallum (Dátum -tól / -ig)
  - Kombinálható a státusz és "külföldi" szűrőkkel.

- **Kijelölt rekordok törlése**
  - A kijelölt kontakt(ok) törlése előtt megerősítést kérő párbeszédablak.

## Technológiai stack

- **.NET / C#**
  - .NET 8
  - WPF (Windows Presentation Foundation)

- **Adatbázis**
  - SQLite
  - Entity Framework Core

- **Email kezelés**
  - Gmail API (Google.Apis.Gmail.v1)
  - OAuth 2.0 alapú bejelentkezés
  - Csatolmánykezelés, `.eml` fájlok letöltése
  - MimeKit a `.eml` fájlok feldolgozásához

- **UI / MVVM**
  - MVVM mintára épülő architektúra
  - `MainWindowViewModel` a fő nézethez
  - `RelayCommand` saját implementáció parancsokhoz
  - `SelectedItemsBehavior` a DataGrid többszörös kijelölésének ViewModel-hez kötéséhez
  - Egyszerű, letisztult, "kártyás" filter layout

## Használat

1. **Alkalmazás indítása**
   - Futtasd a `ContactManager.exe` fájlt

2. **Bejelentkezés Gmail fiókkal**
   - Kattints a **Bejelentkezés** gombra.
   - Válaszd ki a kívánt Google-fiókot és engedélyezd a gmail hozzáférést.
   - Sikeres bejelentkezésnél a fejlécben megjelenik: `Bejelentkezve: <email cím>`.

3. **Feladó megadása**
   - A **Feladó** mezőben add meg azt az email címet, ahonnan a pályázói levelek érkeznek
     (pl. karrier oldal vagy álláshirdetés feladója).
   - A mező értéke automatikusan elmentésre kerül, legközelebb innen tölti vissza az app.

4. **Frissítés (emailek beolvasása)**
   - Kattints a **Frissítés** gombra.
   - Az alkalmazás:
     - megkeresi a feladótól érkező, csatolmányos leveleket,
     - letölti a `.eml` csatolmányokat egy ideiglenes mappába,
     - feldolgozza ezeket, és új kontaktokat szúr be az adatbázisba.

5. **Kontakok böngészése, szűrése**
   - A táblázatban megjelenő sorokat a felső szűrőrészen (név, telefon, email, dátum intervallum) tudod szűrni.
   - A "Hiányos kontaktok" checkbox csak azokat mutatja, akiknél még nincs Published/Hired státusz.
   - A "Csak külföldiek" checkbox csak az `IsForeign = true` rekordokat mutatja.

6. **Státusz és jelölések**
   - **Külföldi jelölés**: jelölj ki egy sort, majd kattints a *Külföldi jelölés* gombra.
   - **Státusz gombok**:
     - *Megjelent / Nem jelent meg* → Published mező
     - *Felvett / Visszautasítva* → Hired mező
   - A gombok engedélyezése dinamikus: csak akkor aktívak, ha a kijelölés állapota értelmezhető az adott műveletre.

7. **Email címek másolása**
   - Jelöld ki a kívánt sorokat a táblázatban.
   - Kattints az **E-mailek másolása vágólapra** gombra.
   - A kijelölt kontaktok email címei soronként kerülnek a vágólapra.

8. **Kijelölt rekordok törlése**
   - Jelöld ki a törölni kívánt kontaktokat.
   - Kattints a **Kijelölt törlése** gombra.
   - A felugró megerősítő ablakban erősítsd meg a törlést.

## Ismert korlátozások / ötletek a jövőre

- Jelenleg csak Gmail integrációt támogat.
- A HTML parszolás konkrét sablonokra van belőve – más felépítésű jelentkezési emaileknél módosításra szorulhat.
- Nincs még fejlett jogosultságkezelés (csak lokális futásra tervezve).
- Lehetséges fejlesztési irányok:
  - több feladó / több kampány kezelése,
  - export (CSV / Excel),
  - statisztikák (hányan jelentkeztek, mennyi lett felvéve stb.),
  - fejlettebb logolás és hibajelentés.
