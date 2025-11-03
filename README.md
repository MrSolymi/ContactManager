# ContactManager (Kontakt Kezelő) – Email-alapú kapcsolatkezelő alkalmazás

## Áttekintés
A **ContactManager** egy WPF asztali alkalmazás, amely Gmail-hozzáférésen keresztül (OAuth2) képes bejövő emailek **.eml csatolmányait** feldolgozni, és azokból **kontaktadatokat** (név, telefonszám, email, jelentkezés ideje) kinyerni.  
A kinyert rekordok **SQLite** adatbázisba kerülnek, **duplikációk nélkül**, majd egy szűrhető-rendezhető táblázatban jelennek meg, ahol az állapotuk (Megjelent/Felvett) tömegesen is módosítható.

---

## Aktiválás (első indítás)
Működés előtt egyszeri aktiválás szükséges az adott eszközön:
1. Add meg a **Client ID** és **Client Secret** értékeket (Google Cloud Console-ban regisztrált app adatai).
2. Sikeres aktiválás után az alkalmazás **megőrzi** ezt a beállítást, **azon az eszközön többet nem kell ismételni**.

> **Megjegyzés:** A bejelentkezési munkamenet (OAuth) csak az alkalmazás futása alatt él; a program **nem tárol** helyben hozzáférési tokent/jelszót.

---

## Bejelentkezés és jogosultságok
- Jelentkezz be a **Google (Gmail)** fiókoddal az alkalmazáson belül.
- A bejelentkezés után **engedélyezned kell**, hogy az app olvashassa a postafiókodat és csatolmányait.  
- A sikeres engedélyezés után a ContactManager eléri az emaileket és a **.eml** csatolmányokat feldolgozás céljából.

---

## Működés – feldolgozási folyamat
1. **Feladó megadása:** az alkalmazásban meg kell adni a feldolgozandó levelek **feladójának email címét**.
2. **Keresés és letöltés:** az alkalmazás a megadott feladó alapján **megkeresi a leveleket**, és ha talál bennük **.eml csatolmányt**, azt **letölti** egy ideiglenes helyre.
3. **Kinyerés:** minden .eml fájlból **1 kontakt** adata kerül kinyerésre:  
   - **Név**, **Telefonszám**, **Email cím**, **Jelentkezés ideje**.
4. **Mentés:** az új kontakt **duplikáció-ellenőrzés** után bekerül az **SQLite** adatbázisba.
5. **Tisztítás:** a sikeresen feldolgozott **.eml fájlokat törli** a rendszer (nem foglalnak helyet feleslegesen).
6. **UI frissítés:** a táblázat automatikusan **teljesen frissül**.

---

## Fő funkciók

### Adatbázis frissítése
- **Frissítés** gomb: beolvasás a Gmailből, .eml letöltés → kinyerés → mentés → táblázat frissítése.
- A gombok **állapotfüggőek**: csak akkor aktívak, amikor az adott művelet elvégezhető.
- A frissítés közben az UI **vizuálisan jelzi**, ha épp folyamatban van az adatok beolvasása és feldolgozása:
  - A „Frissítés” gomb ilyenkor **letiltásra kerül**.
  - A képernyőn egy **„Frissítés folyamatban...”** üzenet jelenik meg.

### Szűrés és rendezés
- **Szűrés**: név, telefonszám, email szerint.
- **Dátumszűrő**: jelentkezés ideje **tól–ig** intervallummal.
- **Hiányos kontaktok** megjelenítése: kapcsoló, amely csak azokat mutatja, ahol a **Megjelent** vagy **Felvett** mező még **nincs beállítva**.
- **Rendezés**: bármely oszlop szerint.

### Állapotkezelés (Megjelent / Felvett)
- Kezdetben a státuszok **üres** (null) értékűek.
- **Tömeges módosítás** több kijelölt rekordon:
  - Egységes állapot esetén váltható:  
    - `null` → `true` / `false`  
    - `true` → `false`  
    - `false` → `true`
  - **Vegyes** (`true` és `false` együtt) esetén a művelet **tiltott**.
- Ha a **„Hiányos kontaktok”** szűrő aktív, a módosított rekord **azonnal eltűnik** a listából (mert már nem hiányos).

### Tömeges email-cím másolás
- **„E-mailek másolása vágólapra”** gomb: a kijelölt kontaktok email címeit olyan formában másolja, hogy **Outlookba** beillesztve (**Ctrl+V**) **egy lépésben** címezhető legyen mindenkinek.

---

## Adatmodell (röviden)
- **Név** | **Telefonszám** | **Email** | **Jelentkezés ideje**  
- **Megjelent** (null/true/false) | **Felvett** (null/true/false)  
- **Dátumok**: Jelentkezés ideje; Adatbázisba kerülés ideje

---

## Biztonság és adatvédelem
- A bejelentkezési munkamenet **csak futásidőben él**, az alkalmazás **nem tárol** helyben hozzáférési adatokat.
- A feldolgozott **.eml csatolmányok törlésre kerülnek** a kinyerés után.
- Az adatbázis **lokálisan**, **SQLite**-ban tárolódik.

---

## Használati lépések (gyorsstart)
1. **Első indítás:** add meg a **Client ID** és **Client Secret** értékeket → **Aktiválás**.
2. **Bejelentkezés** Gmail fiókba → add meg a szükséges **engedélyeket**.
3. **Feladó email címének megadása** – az alkalmazás csak az ettől a címről érkező leveleket fogja vizsgálni.
4. **Frissítés** gomb → új kontaktok beolvasása a .eml csatolmányokból.
5. **Szűrés** és **rendezés** beállítása a rács fölötti sávban.
6. **Tömeges módosítás**: jelöld ki a sorokat → állapotgombok (Megjelent/Felvett).
7. **Email-címek másolása**: kijelölt sorok → „E-mailek másolása vágólapra” → beillesztés Outlookba.

---

## Követelmények
- Windows 10/11
- Internetkapcsolat (Gmail eléréséhez)
- Google Cloud Console-ban regisztrált alkalmazás (**Client ID** + **Client Secret**)

---

## Hibaelhárítás (gyakori esetek)
- **Nem aktív a Frissítés gomb:** előbb jelentkezz be Gmaillel, majd add meg a **feladó email címét**.
- **Nem kerül be kontakt:** ellenőrizd, hogy a .eml csatolmány **érvényes** adatokat tartalmaz-e (név, telefonszám, email).
- **Duplikáció gyanú:** a rendszer deduplikál; ha egy rekord nem látszik újként, valószínűleg már szerepel az adatbázisban.
- **Üres lista szűréskor:** kapcsold ki ideiglenesen a „Hiányos kontaktok” szűrőt, vagy ellenőrizd a dátumintervallumot.

---

## UI áttekintés (fő elemek)
- **Bejelentkezés / Frissítés** gombok  
- **Feladó** – ez alapján szűr a rendszer, csak az ettől a címtől érkezett emailekben keres csatolmányokat  
- **Állapotjelző** (bejelentkezve/nem)  
- **Email-címek másolása** gomb  
- **Állapotgombok**: Megjelent, Nem jelent meg, Felvett, Visszautasítva  
- **Szűrősáv**: Név, Telefonszám, Email, Dátum -tól/-ig, „Hiányos kontaktok”  
- **DataGrid**: rendezhető oszlopok, többszörös kijelölés  

---

## Újdonságok és fejlesztések

### Telefonszám-normalizálás (+36 → 06)
Az **EmlProcessorService** `NormPhone` metódusa továbbfejlesztésre került:  
- megtartja a számjegyeket és a `+` jelet,  
- a **+36-tal kezdődő** telefonszámokat automatikusan **06-ra alakítja**,  
- így az adatbázisban minden telefonszám **egységes formátumban** kerül tárolásra.  

> Példa:  
> `+36201234567` → `06201234567`

Ez segíti a későbbi keresést, duplikációkezelést és szűrést.

---

### Frissítés állapotának megjelenítése az UI-n
A felhasználói élmény javítása érdekében bevezetésre került az **„IsRefreshing”** állapot:
- Amíg a Gmailből történik a levelek és csatolmányok feldolgozása,  
  az alkalmazás kijelzi: *„Frissítés folyamatban...”*
- A **Frissítés gomb** ilyenkor **automatikusan letiltódik**, így elkerülhetők a párhuzamos műveletek.
- A megvalósításhoz két új **WPF konverter** került bevezetésre:
  - `BoolToVisibilityConverter`  
  - `InverseBoolConverter`
- Ezek lehetővé teszik, hogy a UI dinamikusan reagáljon a `IsRefreshing` állapot változásaira.

---

## Összegzés
A **ContactManager** gyors és megbízható módon automatizálja a Gmailbe érkező, **.eml** csatolmányokban található jelentkezések feldolgozását.  
A deduplikált adatbázis, az állapotkezelés, a rugalmas szűrés/rendezés és a **tömeges email-cím másolás** mind azt szolgálja, hogy a kiválasztás és kommunikáció **gyorsabb**, **átláthatóbb** és **hibamentesebb** legyen.

A legutóbbi frissítésekkel az alkalmazás:
- **konzisztens telefonszám-formátumot** használ,  
- és **valós idejű visszajelzést ad** a felhasználónak, ha éppen frissítést végez.
