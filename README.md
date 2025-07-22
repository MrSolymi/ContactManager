# McDContactManager - Email Export és Feldolgozó Alkalmazás

## Előkészületek

### 1. Firebird telepítése
Az alkalmazás használatához szükséges a **Firebird** levelezőkliens telepítése.  
[Letöltés itt](https://www.mozilla.org/en-US/thunderbird/) (ha még nincs telepítve).

### 2. Bejelentkezés
Jelentkezz be a Firebirdbe az email fiókoddal.

### 3. Plugin telepítése
A levelek kimentéséhez szükséges a **ImportExportTools NG** nevű plugin.  
Ez telepíthető a Firebird menüjében:

- **Kiegészítők és témák**
- Keresés: `ImportExportTools NG`
- Telepítés

### 4. Plugin beállítása

- A plugin telepítése után bal felül megjelenik egy saját gombja.
- Kattints rá -> **Opciók**
- Az új ablakban:
  - **Vegyes** fülön ellenőrizd, hogy a *Szöveg és CSV formátum karakterkódolása exportáláskor* `UTF-8` legyen.
  - A **Mappák exportálása** fülön állítsd be azt a mappát, ahová az emaileket szeretnéd exportálni.

### 5. Email exportálása

- Jobbklikk a bal oldali mappalistában az email fiókon a kívánt mappára
- Válaszd:  
  `ImportExportTools NG -> Az összes üzenet exportálása a mappába -> Egyszerű szöveges formátum -> Üzenetek egyetlen fájlként`
- A felugró ablakokkal nem kell foglalkozni

Ez a lépés létrehoz egy `.txt` fájlt az előzőleg megadott mappában, amely tartalmazza az emailjeidet.

> ⚠️ A plugin telepítését csak egyszer kell elvégezni!

---

## Az alkalmazás használata

### 1. Indítás

Indítsd el az alkalmazást.

### 2. Funkciók

- **Upload** – A kimentett `.txt` fájl feltöltése
- **Load** – Az adatok betöltése (általában automatikusan megtörténik)
- **Update** – Jelenleg nincs funkciója
- **Mark as Published** – Az adott bejegyzés megjelölése megjelentként
- **Mark as Hired** – Az adott bejegyzés megjelölése felvettként
- **Mark as NOT Published** – Megjelölés nem megjelentként
- **Mark as NOT Hired** – Megjelölés nem felvettként

### 3. Szűrési lehetőségek

A felhasználó az alábbi mezők szerint tud szűrni:

- **Name** – név alapján
- **Phone** – telefonszám alapján
- **Email** – email alapján
- **Date from / Date to** – dátumintervallum alapján (az adatbázisba kerülés dátumát nézi)

### 4. Rendezés

Bármelyik oszlop rendezhető növekvő vagy csökkenő sorrendbe.

---

## Összegzés

Ezzel a folyamattal egyszerűen exportálhatod az emailjeidet szöveges fájlba, majd az alkalmazás segítségével feldolgozhatod és adminisztrálhatod őket.
