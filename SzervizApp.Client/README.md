# SzervizApp.Client

Windows Forms kliens a szervizkezelő rendszerhez.

## Szerelői feladatkezelés

A szerelő a Javítások képernyőn két listát lát:
- **Saját feladataim**: a hozzá rendelt munkalapok.
- **Szabad feladatok**: a még felelős nélkül nyitott javítások.

A szerelő a szabad feladatok sorában a **Feladat vállalása** gombbal saját magához rendelheti a munkát. A manager ettől függetlenül a javítás részleteinél továbbra is kijelölhet vagy módosíthat felelőst. A vállalás szerveroldalon, a javítás verziószámával védetten történik, így két szerelő nem vállalhatja sikeresen ugyanazt a munkát.

## Ügyféladatok és ügyfélfiók

Az ügyfél adatlapjának rögzítéséhez nincs szükség jelszóra. A bejelentkezési fiók külön, manager által létrehozható.
