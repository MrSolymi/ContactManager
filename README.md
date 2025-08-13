# McDContactManager – Email alapú kapcsolatkezelő alkalmazás

## Áttekintés
A **McDContactManager** egy asztali alkalmazás, amely Microsoft Graph API-t használva, OAuth2 hitelesítésen keresztül képes emaileket letölteni és azokból kontaktadatokat kinyerni.  
A rendszer kizárólag az adott feladói címről érkező, releváns adatokat tartalmazó leveleket dolgozza fel, és a kontaktokat egy helyi adatbázisban tárolja, duplikációk nélkül.  

Az alkalmazás célja, hogy automatizáltan kezelje a beérkező jelentkezéseket, majd kényelmesen szűrhető és módosítható formában jelenítse meg azokat.

---

## Használat előtti beállítás

1. **Kliens ID megadása**  
   Indításkor az alkalmazás kéri a Microsoft Azure-ban regisztrált alkalmazás **Client ID**-ját.  
   Érvényes Client ID nélkül az alkalmazás nem használható.

2. **Hitelesítés**  
   A bejelentkezés Microsoft fiókkal történik, OAuth2 protokoll segítségével.  
   Sikeres hitelesítés után az alkalmazás automatikusan hozzáfér az engedélyezett email adatokhoz.

---

## Fő funkciók

### 1. Email letöltés és feldolgozás
- A felhasználó beírhat egy konkrét feladói email címet.
- Az alkalmazás csak az ettől a címtől érkezett leveleket vizsgálja.
- Amennyiben a levél tartalmazza a szükséges adatokat (név, telefonszám, email), az bekerül az adatbázisba.
- **Duplikációkezelés:** minden kontakt csak egyszer szerepelhet az adatbázisban.

### 2. Adatvizualizáció
A kontaktok egy **DataGrid**-ben jelennek meg, ahol:
- **Szűrés** lehetséges:
  - név szerint
  - email cím szerint
  - telefonszám szerint
  - bekerülés dátuma alapján (tól–ig intervallum)
  - **Új funkció:** csak a felülvizsgálatlan rekordok szűrése (olyan kontaktok, ahol a *Megjelent* vagy *Felvett* mező még nem lett beállítva)
- **Rendezés** bármely oszlop szerint
- **Többszörös kijelölés** támogatott (`Ctrl + kattintás` vagy `Shift + kattintás`).

### 3. Adatmódosítás
Minden kontakt esetében beállítható:
- **Megjelent** – részt vett-e a tájékoztatón
- **Felvett** – felvételt nyert-e

#### Új módosítási logika
- A státusz mezők (`Megjelent` / `Felvett`) alapértelmezésben üresek (null), ha még nem történt módosítás.
- Tömegműveletek esetén a rendszer csak akkor engedi a módosítást, ha a kijelölt elemek állapota egységes:
  - `null` → állítható `true`-ra vagy `false`-ra.
  - `true` → állítható `false`-ra.
  - `false` → állítható `true`-ra.
  - Vegyes állapot (pl. van `true` és `false` is egyszerre) → a művelet letiltva.
- A **"Csak felülvizsgálatlanok"** szűrő bekapcsolása esetén, ha egy elem státusza módosul, a rendszer automatikusan frissíti a nézetet és eltávolítja a listából a már beállított rekordot.

---

## Technikai jellemzők
- **Microsoft Graph API** integráció
- **OAuth2** alapú hitelesítés
- **SQLite** adatbázis tárolás
- **Valós idejű UI frissítés** – minden módosítás azonnal látszik
- **Reszponzív felület**
- **Live Filtering támogatás** – a szűrők automatikusan frissülnek státuszváltozás esetén

---

## Összegzés
A McDContactManager ideális megoldás azoknak, akik automatizáltan szeretnék kezelni a beérkező jelentkezéseket, kiszűrni a releváns adatokat, és ezeket egy könnyen kezelhető, szűrhető és módosítható felületen szeretnék látni.  
Az új szűrési és tömegműveleti logika segítségével a feldolgozás még gyorsabbá és hibamentesebbé válik.
