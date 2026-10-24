"""Fejlesztői tesztadatok ellenőrzése. Futtatás: python tests/verify_demo.py"""
from contextlib import closing
from pathlib import Path
import sqlite3
import tempfile
import verify_backend as test


def snapshot(path):
    with closing(sqlite3.connect(path)) as db:
        tables = [r[0] for r in db.execute("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%'")]
        return {t: db.execute('SELECT * FROM "' + t + '" ORDER BY Id').fetchall() for t in tables}


with tempfile.TemporaryDirectory(prefix="szerviz-demo-") as directory:
    directory = Path(directory)
    fresh = directory / "demo.db"
    with test.Server(fresh, directory, demo=True) as server:
        manager = server.client(); manager.login("manager", "Manager123!")
        schema = manager.test("Swagger séma elérhető", "GET", "/swagger/v1/swagger.json")
        test.check("Nincs időjárás végpont a Swaggerben", not any("weather" in p.lower() for p in schema["paths"]))
        accounts = manager.test("Tesztfiókok listája", "GET", "/api/felhasznalok")
        test.check("Manager és 6 tesztfiók", len(accounts) == 7)
        jobs = manager.test("Mintajavítások", "GET", "/api/javitasok")
        test.check("Mind a 6 állapotra van példa", len(jobs) == 6 and {j["allapot"] for j in jobs} == {
            "Atvett", "BevizsgalasAlatt", "JavitasAlatt", "AlkatreszreVar", "Lezart", "Visszaadva"})
        test.check("3 mintaügyfél", len(manager.test("Ügyfelek", "GET", "/api/ugyfelek")) == 3)
        test.check("5 mintaeszköz", len(manager.test("Eszközök", "GET", "/api/eszkozok")) == 5)
        test.check("3 alkatrész", len(manager.test("Alkatrészek", "GET", "/api/alkatreszek")) == 3)
        roles = {"demo.recepcios": "Recepcios", "demo.szerelo1": "Szerelo", "demo.szerelo2": "Szerelo",
                 "demo.ugyfel1": "Ugyfel", "demo.ugyfel2": "Ugyfel", "demo.ugyfel3": "Ugyfel"}
        for name, role in roles.items():
            c = server.client()
            account = c.login(name, "Teszt12345!")
            test.check("Megfelelő szerepkör: " + name, account["szerepkor"] == role)
            if role == "Ugyfel":
                own = c.test("Saját tesztjavítások: " + name, "GET", "/api/javitasok")
                test.check("Csak saját ügyféltörténet: " + name, len(own) > 0 and all(j["ugyfelId"] == account["ugyfelId"] for j in own))
        old = next(j for j in jobs if j["allapot"] == "Visszaadva")
        total = manager.test("Lezárt javítás költségei", "GET", f'/api/javitasok/{old["id"]}/koltsegek')
        test.check("Példaösszeg: 6000 + 18990 + 1500", total["vegosszeg"] == 26490 and total["koltsegekVeglegesitve"])
        current = next(j for j in jobs if j["allapot"] == "JavitasAlatt")
        worker = server.client(); worker.login("demo.szerelo1", "Teszt12345!")
        worker.test("Megkezdett mintamunka védi a véglegesítést", "PUT", f'/api/javitasok/{current["id"]}/koltsegek/veglegesites', 409)
        receptionist = server.client(); receptionist.login("demo.recepcios", "Teszt12345!")
        closed = next(j for j in jobs if j["allapot"] == "Lezart")
        receptionist.test("Visszaadásra váró tesztjavítás átadható", "POST", f'/api/javitasok/{closed["id"]}/visszaadas', 201, {"atvevoNev": "Minta Péter"})
        worker.test("Tesztfiók saját jelszava módosítható", "PUT", "/api/auth/jelszo", 200,
                    {"regiJelszo": "Teszt12345!", "ujJelszo": "Modositott123!"})
        manager.test("Manager saját jelszava módosítható", "PUT", "/api/auth/jelszo", 200,
                     {"regiJelszo": "Manager123!", "ujJelszo": "UjManager123!"})
        before = snapshot(fresh)
    with test.Server(fresh, directory, demo=True) as server:
        test.check("Újraindítás nem dupláz és nem állít vissza adatot", snapshot(fresh) == before)
        server.client().login("manager", "UjManager123!")
        server.client().login("demo.szerelo1", "Modositott123!")

    populated = directory / "existing.db"
    with test.Server(populated, directory) as server:
        manager = server.client(); manager.login("manager", "Manager123!")
        existing = manager.test("Meglévő saját ügyfél", "POST", "/api/ugyfelek", 201,
                                {"nev": "Meglévő saját ügyfél", "telefon": "123456789"})
    with test.Server(populated, directory, demo=True) as server:
        manager = server.client(); manager.login("manager", "Manager123!")
        clients = manager.test("Tesztadatok meglévő adatbázisban", "GET", "/api/ugyfelek")
        test.check("Nem üres adatbázisba is bekerülnek a tesztadatok", len(clients) == 4)
        test.check("A meglévő ügyfél változatlan", next(c for c in clients if c["id"] == existing["id"]) == existing)
        test.check("Meglévő manager mellé is létrejön minden tesztfiók", len(manager.test("Bővített fióklista", "GET", "/api/felhasznalok")) == 7)
        with closing(sqlite3.connect(populated)) as db:
            test.check("Minden tesztadat-kapcsolat érvényes", db.execute("PRAGMA foreign_key_check").fetchall() == [])

print(f"\n{len(test.RESULTS)} tesztadat-ellenőrzés sikeres.")
