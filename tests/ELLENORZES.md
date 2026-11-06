# Elvégzett ellenőrzések

Dátum: 2026. október 24. Környezet: Windows, magyar területi beállítás, .NET SDK 9.0.314, EF Core 9.0.13, SQLite.

- `dotnet build SzervizApp.sln`: sikeres, **0 hiba, 0 figyelmeztetés**.
- `dotnet ef migrations has-pending-model-changes --project SzervizApp.DataContext --startup-project SzervizApp.DataContext`: nincs eltérés a modell és a migráció között.
- `python tests/verify_backend.py`: **144 sikeres ellenőrzés**, valódi HTTP-kérésekkel és az adatmegőrzés vizsgálatával.
- `python tests/verify_demo.py`: **51 sikeres ellenőrzés** a tesztfiókokra, a mintaállományra, a Swagger végpontjaira és a módosított adatok/jelszavak ismételt indítás utáni megőrzésére. Üres és már feltöltött adatbázison is ellenőrizve.
- `python tests/verify_jwt.py`: **31 sikeres ellenőrzés** a Swagger Authorize sémájára, a JWT-aláírásra, lejáratra, kibocsátóra, célközönségre, szerepkörre, kijelentkezéskori visszavonásra és az újraindítás utáni érvényességre.
- `dotnet run --project tests/DataContextCheck/DataContextCheck.csproj`: **7 sikeres ellenőrzés** az adatmodellre, a konkurenciára és a migrációkra.

Lefedett fő esetek: új adatbázis; az eredeti Initial migrációval létrehozott, három ügyféllel, eszközzel és javítással feltöltött pilot frissítése; az eredeti mezők megőrzése; mindhárom lezárási eredmény; munkanapló; alkatrészre várás; költségtételek és véglegesítés; alkatrészár-változás; visszaadás; tulajdonosváltás és előzmények; mind a négy szerepkör; ügyféladatok elkülönítése; munkamenetek visszavonása; újraindítás utáni megőrzés; idegen kulcsok; párhuzamos módosítás ütközése és a kapcsolódó tétel visszagörgetése; migráció visszaállítása és újbóli alkalmazása.

Minden teszt külön ideiglenes adatbázist használt. A tesztelt frissítés az eredeti migráció sémájából indult; a felhasználó saját adatbázisfájlját nem módosította. A tesztek backend-ellenőrzések; felhasználói felület nem része ennek a csomagnak.


## 2026. november 6-i kiegészítés

A fenti, 2026. október 24-én lefuttatott teszteredmények az akkori kódállapotra vonatkoznak. Az új UI-funkcióval együtt bekerült a szerelői önkéntes feladatvállalás szerveroldali végpontja (`POST /api/javitasok/{id}/vallalas`) és a `verify_backend.py` tesztjei: recepciós tiltása, sikeres szerelői vállalás, másik szerelő dupla vállalásának tiltása, valamint manager általi kiosztás/átosztás. Ezeket az új ellenőrzéseket ebben a környezetben nem tudtam lefuttatni, mert nincs telepített .NET SDK. Windows alatt futtasd újra a teljes buildet és a `python tests/verify_backend.py` tesztet.
