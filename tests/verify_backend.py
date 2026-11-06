"""Valódi HTTP- és SQLite-ellenőrzés, külön ideiglenes adatbázisokkal.
Előtte: dotnet build SzervizApp.sln; dotnet tool restore
Futtatás: python tests/verify_backend.py [--report eredmeny.json]
Nem használ külső Python-csomagot, és nem nyúl a saját szerviz.db fájlhoz.
"""
import argparse
import json
import os
import socket
import sqlite3
import subprocess
import tempfile
import time
from contextlib import closing
from datetime import datetime, timezone, timedelta
from decimal import Decimal
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.request import Request, build_opener

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / "SzervizApp.Api"
RESULTS = []


def check(name, condition):
    RESULTS.append({"name": name, "passed": bool(condition)})
    print(("PASS " if condition else "FAIL ") + name, flush=True)
    if not condition:
        raise AssertionError(name)


class Client:
    def __init__(self, base):
        self.base = base
        self.opener = build_opener()
        self.token = None

    def raw(self, method, path, body=None):
        data = json.dumps(body, ensure_ascii=False).encode("utf-8") if body is not None else None
        headers = {"Content-Type": "application/json"}
        if self.token:
            headers["Authorization"] = "Bearer " + self.token
        req = Request(self.base + path, data=data, method=method, headers=headers)
        try:
            response = self.opener.open(req, timeout=15)
        except HTTPError as error:
            response = error
        with response:
            self.last_headers = dict(response.headers)
            text = response.read().decode("utf-8")
            payload = json.loads(text) if text else None
            if response.status == 200 and isinstance(payload, dict) and "token" in payload:
                self.token = payload["token"]
            if response.status == 204 and path == "/api/auth/kijelentkezes":
                self.token = None
            return response.status, payload

    def test(self, name, method, path, expected=200, body=None):
        status, payload = self.raw(method, path, body)
        if status != expected:
            print(json.dumps(payload, ensure_ascii=True), flush=True)
        check(name, status == expected)
        return payload

    def login(self, name, password="TesztJelszo123!"):
        return self.test("Belépés: " + name, "POST", "/api/auth/bejelentkezes", 200,
                         {"felhasznalonev": name, "jelszo": password})


class Server:
    def __init__(self, db, directory, demo=False):
        self.db = db
        self.directory = directory
        self.demo = demo
        with socket.socket() as sock:
            sock.bind(("127.0.0.1", 0))
            self.base = "http://127.0.0.1:" + str(sock.getsockname()[1])

    def __enter__(self):
        self.log_path = self.directory / (self.db.stem + ".log")
        self.log = self.log_path.open("w", encoding="utf-8")
        env = os.environ.copy()
        env.update({"DemoAdatok__Engedelyezve": str(self.demo).lower(), "DemoAdatok__Jelszo": "Teszt12345!",
                    "Bootstrap__AdminNev": "manager", "Bootstrap__AdminJelszo": "Manager123!"})
        self.proc = subprocess.Popen(["dotnet", str(API / "bin/Debug/net9.0/SzervizApp.Api.dll"),
            "--urls", self.base, "--environment", "Development",
            "--ConnectionStrings:DefaultConnection", "Data Source=" + str(self.db)],
            cwd=API, env=env, stdout=self.log, stderr=subprocess.STDOUT)
        for _ in range(120):
            if self.proc.poll() is not None:
                self.log.close()
                raise RuntimeError(self.log_path.read_text(encoding="utf-8"))
            try:
                if Client(self.base).raw("GET", "/swagger/v1/swagger.json")[0] == 200:
                    return self
            except (URLError, TimeoutError):
                pass
            time.sleep(0.1)
        self.__exit__(None, None, None)
        raise TimeoutError("Az API nem indult el.")

    def __exit__(self, exc_type, exc, tb):
        self.proc.terminate()
        self.proc.wait(timeout=15)
        self.log.close()
        if exc_type:
            print(self.log_path.read_text(encoding="utf-8")[-10000:])

    def client(self):
        return Client(self.base)


def ef_update(db, target):
    result = subprocess.run(["dotnet", "ef", "database", "update", target, "--no-build",
        "--project", "SzervizApp.DataContext", "--startup-project", "SzervizApp.DataContext",
        "--connection", "Data Source=" + str(db)], cwd=ROOT, capture_output=True, text=True, encoding="utf-8")
    if result.returncode:
        raise RuntimeError(result.stdout + result.stderr)


def utc(value=None):
    return (value or datetime.now(timezone.utc)).isoformat()


def scenario(directory):
    fresh = directory / "fresh.db"
    with Server(fresh, directory) as server:
        anon = server.client()
        anon.test("Névtelen kérés tiltása", "GET", "/api/javitasok", 401)
        manager = server.client()
        manager.login("manager", "Manager123!")
        check("Új adatbázisban nincsenek kényszerített tesztügyfelek",
              manager.test("Üres ügyféllista", "GET", "/api/ugyfelek") == [])
        with closing(sqlite3.connect(fresh)) as conn:
            count = conn.execute("SELECT count(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%'").fetchone()[0]
            check("12 alkalmazástábla és 2 migráció", count == 12 and conn.execute('SELECT count(*) FROM __EFMigrationsHistory').fetchone()[0] == 2)
            check("Jelszó helyett hash tárolódik", conn.execute('SELECT JelszoHash FROM Felhasznalok').fetchone()[0] != "Manager123!")

    legacy = directory / "legacy.db"
    ef_update(legacy, "Initial")
    with closing(sqlite3.connect(legacy)) as conn:
        for i in range(1, 4):
            conn.execute('INSERT INTO Ugyfelek (Id,Nev,Telefon,Email) VALUES (?,?,?,?)', (i, f"Régi ügyfél {i}", "1234567", None))
            conn.execute('INSERT INTO Eszkozok (Id,TulajdonosId,EszkozTipus,Gyarto,Sorozatszam) VALUES (?,?,?,?,?)', (i, i, "Laptop", "Régi gyártó", f"REGI-{i}"))
            conn.execute('INSERT INTO Javitasok (Id,UgyfelId,EszkozId,BejelentettHiba,AtvetelDatuma,Allapot,LezarasDatuma,Eredmeny,LezarasIndoklasa) VALUES (?,?,?,?,?,?,?,?,?)',
                (i, i, i, "Régi pilot hiba", "2026-10-01 10:00:00", 1 if i == 1 else 2,
                 None if i == 1 else "2026-10-02 10:00:00", None if i == 1 else i - 1, None if i < 3 else "Nem gazdaságos"))
        before = {t: conn.execute("SELECT * FROM " + t).fetchall() for t in ("Ugyfelek", "Eszkozok", "Javitasok")}
        conn.commit()

    with Server(legacy, directory) as server:
        manager = server.client(); manager.login("MANAGER", "Manager123!")
        with closing(sqlite3.connect(legacy)) as conn:
            columns = {"Ugyfelek": "Id,Nev,Telefon,Email", "Eszkozok": "Id,TulajdonosId,EszkozTipus,Gyarto,Sorozatszam",
                       "Javitasok": "Id,UgyfelId,EszkozId,BejelentettHiba,AtvetelDatuma,Allapot,LezarasDatuma,Eredmeny,LezarasIndoklasa"}
            check("Pilot frissítése: minden eredeti sor és mező megmaradt", all(conn.execute("SELECT " + columns[t] + " FROM " + t).fetchall() == before[t] for t in before))
            check("Régi ügyfelek aktívak, a régi felelős ismeretlen", conn.execute('SELECT count(*) FROM Ugyfelek WHERE Aktiv=1').fetchone()[0] == 3
                  and conn.execute('SELECT count(*) FROM Javitasok WHERE FelelosFelhasznaloId IS NULL AND Verzio=1').fetchone()[0] == 3)
        manager.test("Szerepkörlista", "GET", "/api/jogosultsagok")
        manager.test("Utolsó manager védelme", "PUT", "/api/felhasznalok/1", 409,
                     {"nev": "Manager", "jogosultsagId": 3, "aktiv": False})

        def user(name, role, customer=None):
            return manager.test("Fiók létrehozása: " + name, "POST", "/api/felhasznalok", 201,
                {"felhasznalonev": name, "nev": name, "jogosultsagId": role, "ugyfelId": customer,
                 "ujJelszo": "TesztJelszo123!"})

        rec_user = user("recepcios", 1)
        tech_user = user("szerelo", 2)
        other_user = user("masikszerelo", 2)
        rec = server.client(); rec.login("recepcios")
        tech = server.client(); tech.login("szerelo")
        other = server.client(); other.login("masikszerelo")
        anon = server.client()
        anon.test("Hibás jelszó", "POST", "/api/auth/bejelentkezes", 401, {"felhasznalonev": "manager", "jelszo": "rossz"})
        rec.test("Recepciós nem kezel fiókokat", "GET", "/api/felhasznalok", 403)
        tech.test("Szerelő nem vesz fel ügyfelet", "POST", "/api/ugyfelek", 403, {"nev": "Tiltott", "telefon": "1"})
        rec.test("Kötelező telefon", "POST", "/api/ugyfelek", 400, {"nev": "Ügyfél"})
        rec.test("Email ellenőrzése", "POST", "/api/ugyfelek", 400, {"nev": "Ügyfél", "telefon": "1", "email": "hibas"})
        ca = rec.test("Ügyfél A felvétele", "POST", "/api/ugyfelek", 201, {"nev": "  Teszt Anna  ", "telefon": "12345"})
        cb = rec.test("Ügyfél B felvétele", "POST", "/api/ugyfelek", 201, {"nev": "Teszt Béla", "telefon": "67890"})
        check("Szövegek tisztítása", ca["nev"] == "Teszt Anna")
        da = rec.test("Eszköz A felvétele", "POST", "/api/eszkozok", 201, {"tulajdonosId": ca["id"], "eszkozTipus": "Laptop", "sorozatszam": "TESZT-001"})
        db = rec.test("Eszköz B felvétele", "POST", "/api/eszkozok", 201, {"tulajdonosId": cb["id"], "eszkozTipus": "Telefon"})
        customer_user = user("ugyfelA", 4, ca["id"])
        user("ugyfelB", 4, cb["id"])
        a = server.client(); a.login("ugyfelA")
        b = server.client(); b.login("ugyfelB")
        userlist = manager.test("Felhasználók lekérése", "GET", "/api/felhasznalok")
        check("Jelszóhash és belépési verzió nem kerül válaszba", "jelszoHash" not in json.dumps(userlist) and "belepesVerzio" not in json.dumps(userlist))
        manager.test("Felhasználónév egyedi kis/nagybetűtől függetlenül", "POST", "/api/felhasznalok", 409,
            {"felhasznalonev": "SZERELO", "nev": "Más", "jogosultsagId": 2, "ujJelszo": "TesztJelszo123!"})
        create = {"ugyfelId": ca["id"], "eszkozId": da["id"], "bejelentettHiba": "Nem kapcsol be"}
        rec.test("Más tulajdonos eszköze nem párosítható", "POST", "/api/javitasok", 400, {**create, "eszkozId": db["id"]})
        rec.test("Üres hibaleírás tiltása", "POST", "/api/javitasok", 400, {**create, "bejelentettHiba": "   "})
        j = rec.test("Javítás felvétele", "POST", "/api/javitasok", 201, create)
        jid = j["id"]; p = f"/api/javitasok/{jid}"
        check("Átvételkor helyes alapadatok", j["allapot"] == "Atvett" and j["felvevoFelhasznaloId"] == rec_user["id"] and j["atvetelDatuma"].endswith("Z"))
        rec.test("Visszaadás lezárás előtt tiltott", "POST", p + "/visszaadas", 409, {"atvevoNev": "Anna"})
        a.test("Ügyfél nem vehet fel javítást", "POST", "/api/javitasok", 403, create)
        b.test("Más ügyfél javítása rejtett", "GET", p, 404)
        own = a.test("Saját javítások listája", "GET", "/api/javitasok")
        check("Ügyfél csak saját javításokat listáz", len(own) == 1 and own[0]["id"] == jid)
        b.test("Más ügyfél költsége rejtett", "GET", p + "/koltsegek", 404)
        a.test("Ügyfél saját eszközei", "GET", "/api/eszkozok")
        b.test("Más ügyfél törzsadata rejtett", "GET", f'/api/ugyfelek/{ca["id"]}', 404)
        rec.test("Felelős kijelölése csak managernek", "PUT", p + "/felelos", 403, {"felhasznaloId": tech_user["id"]})
        manager.test("Recepciós nem lehet szerelő felelős", "PUT", p + "/felelos", 400, {"felhasznaloId": rec_user["id"]})
        manager.test("Szerelő hozzárendelése", "PUT", p + "/felelos", 200, {"felhasznaloId": tech_user["id"], "tervezettBefejezes": utc(datetime.now(timezone.utc) + timedelta(days=1))})
        manager.test("Nyitott javítással szerelő nem deaktiválható", "PUT", f'/api/felhasznalok/{tech_user["id"]}', 409,
            {"nev": "Szerelő", "jogosultsagId": 2, "aktiv": False})
        other.test("Más szerelő nem módosíthat", "POST", p + "/bevizsgalas", 403, {"eredmeny": "Tiltott"})
        tech.test("Bevizsgálás kihagyása tiltott", "PUT", p + "/allapot", 409, {"allapot": "JavitasAlatt"})
        tech.test("Érvénytelen enum tiltása", "PUT", p + "/allapot", 400, {"allapot": 999})
        tech.test("Bevizsgálás rögzítése", "POST", p + "/bevizsgalas", 201, {"eredmeny": "Hibás csatlakozó", "becsultKoltseg": 12000})
        tech.test("Dupla bevizsgálás tiltása", "POST", p + "/bevizsgalas", 409, {"eredmeny": "Dupla"})
        tech.test("Bevizsgálás módosítása", "PUT", p + "/bevizsgalas", 200, {"eredmeny": "Csatlakozó cserélendő", "becsultKoltseg": 11000})
        tech.test("Javítás elindítása", "PUT", p + "/allapot", 200, {"allapot": "JavitasAlatt"})
        start = utc()
        work = {"leiras": "Csatlakozó cseréje", "kezdes": start, "oradij": 6000}
        tech.test("Negatív óradíj tiltása", "POST", p + "/munkanaplo", 400, {**work, "oradij": -1})
        tech.test("Jövőbeli munka tiltása", "POST", p + "/munkanaplo", 400, {**work, "kezdes": utc(datetime.now(timezone.utc) + timedelta(days=1))})
        m = tech.test("Munka megkezdése", "POST", p + "/munkanaplo", 201, work)
        tech.test("Folyamatban lévő munka nem véglegesíthető", "PUT", p + "/koltsegek/veglegesites", 409)
        m = tech.test("Munka befejezése", "PUT", p + f'/munkanaplo/{m["id"]}', 200, {**work, "befejezes": utc()})
        part_body = {"cikkszam": "CS-001", "nev": "Csatlakozó", "egyseg": "db", "aktualisEgysegar": 125.55}
        part = manager.test("Alkatrész törzsadat", "POST", "/api/alkatreszek", 201, part_body)
        manager.test("Egyedi cikkszám", "POST", "/api/alkatreszek", 409, part_body)
        tech.test("Negatív mennyiség tiltása", "POST", p + "/alkatreszek", 400, {"alkatreszId": part["id"], "mennyiseg": -2})
        usage = tech.test("Alkatrészfelhasználás", "POST", p + "/alkatreszek", 201, {"alkatreszId": part["id"], "mennyiseg": 2})
        manager.test("Aktuális alkatrészár változtatása", "PUT", f'/api/alkatreszek/{part["id"]}', 200, {**part_body, "aktualisEgysegar": 999})
        manager.test("Korábbi tételek mértékegysége védett", "PUT", f'/api/alkatreszek/{part["id"]}', 409, {**part_body, "egyseg": "csomag"})
        check("Régi felhasználás megőrzi az eredeti árat", tech.test("Korábbi alkatrésztételek", "GET", p + "/alkatreszek")[0]["egysegar"] == 125.55)
        extra = {"megnevezes": "Szállítás", "tipus": "Szallitas", "osszeg": 10.20}
        tech.test("Negatív költség tiltása", "POST", p + "/koltsegek", 400, {**extra, "osszeg": -1})
        cost = tech.test("Egyéb költség felvétele", "POST", p + "/koltsegek", 201, extra)
        summary = tech.test("Költségösszesítés", "GET", p + "/koltsegek")
        expected = Decimal(str(m["munkadij"])) + Decimal("251.10") + Decimal("10.20")
        check("Munkadíj + alkatrész + egyéb költség pontos összege", Decimal(str(summary["vegosszeg"])) == expected)
        tech.test("Lezárás véglegesítés nélkül tiltott", "PUT", p + "/lezaras", 409, {"eredmeny": "Javitva", "lezarasIndoklasa": "Kész"})
        tech.test("Költségek véglegesítése", "PUT", p + "/koltsegek/veglegesites")
        tech.test("Költségtétel javítása", "PUT", p + f'/koltsegek/{cost["id"]}', 200, {**extra, "osszeg": 20})
        check("Módosítás után új véglegesítés kell", not tech.test("Véglegesítés állapota", "GET", p)["koltsegekVeglegesitve"])
        tech.test("Ismételt véglegesítés", "PUT", p + "/koltsegek/veglegesites")
        tech.test("Sikeres lezáráshoz is kötelező indoklás", "PUT", p + "/lezaras", 400, {"eredmeny": "Javitva"})
        tech.test("Javítás lezárása", "PUT", p + "/lezaras", 200, {"eredmeny": "Javitva", "lezarasIndoklasa": "Csatlakozó cserélve, működik"})
        tech.test("Ismételt lezárás tiltott", "PUT", p + "/lezaras", 409, {"eredmeny": "Javitva", "lezarasIndoklasa": "Kész"})
        tech.test("Lezárt javítás költsége védett", "DELETE", p + f'/koltsegek/{cost["id"]}', 409)
        tech.test("Lezárt javítás alkatrésze védett", "DELETE", p + f'/alkatreszek/{usage["id"]}', 409)
        tech.test("Szerelő nem adhat vissza eszközt", "POST", p + "/visszaadas", 403, {"atvevoNev": "Anna"})
        rec.test("Eszköz visszaadása", "POST", p + "/visszaadas", 201, {"atvevoNev": "Teszt Anna", "megjegyzes": "Átvette"})
        rec.test("Dupla visszaadás tiltott", "POST", p + "/visszaadas", 409, {"atvevoNev": "Anna"})
        check("Visszaadva állapot", a.test("Saját státusz követése", "GET", p)["allapot"] == "Visszaadva")
        a.test("Visszaadás adatai", "GET", p + "/visszaadas")
        a.test("Saját eszköz javítási előzménye", "GET", f'/api/eszkozok/{da["id"]}/javitasok')
        a.test("Ügyfél kapcsolatfelvétele", "POST", "/api/uzenetek", 201, {"javitasId": jid, "tartalom": "Köszönöm!"})
        rec.test("Ügyfélszolgálati válasz", "POST", "/api/uzenetek", 201, {"ugyfelId": ca["id"], "javitasId": jid, "tartalom": "Szívesen."})
        check("Más ügyfél nem lát üzenetet", b.test("Üzenetek elkülönítése", "GET", "/api/uzenetek") == [])
        b.test("Üzenet más javításhoz tiltott", "POST", "/api/uzenetek", 404, {"javitasId": jid, "tartalom": "Más ügye"})
        a.test("Ügyfélazonosító hamisítása tiltott", "POST", "/api/uzenetek", 403, {"ugyfelId": cb["id"], "tartalom": "Hamis"})
        tech.test("Szerelő nem ügyfélszolgálatos", "GET", "/api/uzenetek", 403)

        # Ugyanaz az eszköz ismét javítható, tulajdonosváltás után is megmaradnak az elkülönített előzmények.
        rec.test("Tulajdonosváltás visszaadás után", "PUT", f'/api/eszkozok/{da["id"]}', 200,
                 {"tulajdonosId": cb["id"], "eszkozTipus": "Laptop", "sorozatszam": "TESZT-001"})
        j2 = rec.test("Ugyanazon eszköz új javítása", "POST", "/api/javitasok", 201, {**create, "ugyfelId": cb["id"]})
        p2 = f'/api/javitasok/{j2["id"]}'
        check("Új tulajdonos nem látja a régi ügyfél javítását", [x["id"] for x in b.test("Előzmények tulajdonosváltás után", "GET", f'/api/eszkozok/{da["id"]}/javitasok')] == [j2["id"]])
        check("Régi tulajdonos csak saját előzményét látja", [x["id"] for x in a.test("Régi saját előzmény", "GET", f'/api/eszkozok/{da["id"]}/javitasok')] == [jid])
        rec.test("Recepciós nem vállalhat szerelői feladatot", "POST", p2 + "/vallalas", 403)
        claimed = tech.test("Szerelő szabad javítás vállalása", "POST", p2 + "/vallalas", 200)
        check("A vállalás a bejelentkezett szerelőt teszi felelőssé", claimed["felelosFelhasznaloId"] == tech_user["id"])
        other.test("Másik szerelő nem vállalhatja el a már kiosztott javítást", "POST", p2 + "/vallalas", 409)
        reassigned = manager.test("Manager át tudja osztani a vállalt javítást", "PUT", p2 + "/felelos", 200,
                                  {"felhasznaloId": other_user["id"]})
        check("A manager kijelölése felülírja a szerelői vállalást", reassigned["felelosFelhasznaloId"] == other_user["id"])
        manager.test("Manager vissza tudja osztani a javítást", "PUT", p2 + "/felelos", 200,
                     {"felhasznaloId": tech_user["id"]})
        tech.test("Nulla költség véglegesítése", "PUT", p2 + "/koltsegek/veglegesites")
        tech.test("Sikeres eredmény munka nélkül tiltott", "PUT", p2 + "/lezaras", 409, {"eredmeny": "Javitva", "lezarasIndoklasa": "Nem volt munka"})
        tech.test("Javítás nélküli lezárás bevizsgálás nélkül", "PUT", p2 + "/lezaras", 200, {"eredmeny": "JavitasNelkulLezarva", "lezarasIndoklasa": "Az ügyfél visszakérte"})
        rec.test("Visszaadás javítás nélküli eredménnyel", "POST", p2 + "/visszaadas", 201, {"atvevoNev": "Béla"})
        rec.test("Régi sikeres pilotjavítás átadható", "POST", "/api/javitasok/2/visszaadas", 201, {"atvevoNev": "Régi ügyfél 2"})
        # A harmadik lezárási eredmény és az alkatrészre várás ága.
        j3 = rec.test("Nem javítható eszköz felvétele", "POST", "/api/javitasok", 201,
            {"ugyfelId": cb["id"], "eszkozId": db["id"], "bejelentettHiba": "Sérült alaplap"})
        p3 = f'/api/javitasok/{j3["id"]}'
        manager.test("Hibás eszköz felelőse", "PUT", p3 + "/felelos", 200, {"felhasznaloId": tech_user["id"]})
        tech.test("Nem javítható eredményhez bevizsgálás kell", "PUT", p3 + "/koltsegek/veglegesites")
        tech.test("Bevizsgálás nélküli nem javítható lezárás tiltott", "PUT", p3 + "/lezaras", 409,
            {"eredmeny": "NemJavithato", "lezarasIndoklasa": "Sérült alaplap"})
        tech.test("Hibás alaplap bevizsgálása", "POST", p3 + "/bevizsgalas", 201, {"eredmeny": "Alaplap sérült"})
        tech.test("Alkatrészre várás", "PUT", p3 + "/allapot", 200, {"allapot": "AlkatreszreVar"})
        tech.test("Várakozásból javításba visszatérés", "PUT", p3 + "/allapot", 200, {"allapot": "JavitasAlatt"})
        disposable = tech.test("Téves egyéb költség felvétele", "POST", p3 + "/koltsegek", 201, extra)
        tech.test("Téves költség törölhető nyitott javításból", "DELETE", p3 + f'/koltsegek/{disposable["id"]}', 204)
        unused = tech.test("Téves alkatrésztétel felvétele", "POST", p3 + "/alkatreszek", 201, {"alkatreszId": part["id"], "mennyiseg": 1})
        tech.test("Téves alkatrésztétel törlése", "DELETE", p3 + f'/alkatreszek/{unused["id"]}', 204)
        tech.test("Hibás eszköz nulla költségének véglegesítése", "PUT", p3 + "/koltsegek/veglegesites")
        tech.test("Nem javítható eredmény rögzítése", "PUT", p3 + "/lezaras", 200,
            {"eredmeny": "NemJavithato", "lezarasIndoklasa": "Az alkatrész nem beszerezhető"})
        rec.test("Nem javítható eszköz visszaadása", "POST", p3 + "/visszaadas", 201, {"atvevoNev": "Béla"})
        tech.test("Saját aktuális feladatok", "GET", "/api/javitasok/feladatok?sajat=true")
        check("Szöveges keresés", len(rec.test("Keresés sorozatszám szerint", "GET", "/api/javitasok?kereses=TESZT-001")) == 2)

        # Meglévő munkamenetek visszavonása jelszó- vagy szerepkörváltáskor.
        a2 = server.client(); a2.login("ugyfelA")
        a.test("Saját jelszó megváltoztatása és új token", "PUT", "/api/auth/jelszo", 200, {"regiJelszo": "TesztJelszo123!", "ujJelszo": "UjTesztJelszo123!"})
        a.test("Jelszóváltó munkamenet megmarad", "GET", "/api/auth/en")
        a2.test("Másik régi munkamenet visszavonva", "GET", "/api/auth/en", 401)
        manager.test("Másik szerelő szerepkörének módosítása", "PUT", f'/api/felhasznalok/{other_user["id"]}', 200,
            {"nev": "Másik szerelő", "jogosultsagId": 1, "aktiv": True})
        other.test("Régi szerepkörrel munkamenet érvénytelen", "GET", "/api/auth/en", 401)
        manager.test("Ügyfélfiók kikapcsolása", "PUT", f'/api/felhasznalok/{customer_user["id"]}', 200,
            {"nev": "Anna", "jogosultsagId": 4, "ugyfelId": ca["id"], "aktiv": False})
        a.test("Kikapcsolt fiók munkamenete visszavonva", "GET", "/api/auth/en", 401)
        rec.test("Kijelentkezés", "POST", "/api/auth/kijelentkezes", 204)
        rec.test("Kijelentkezés után védelem", "GET", "/api/javitasok", 401)
        with closing(sqlite3.connect(legacy)) as conn:
            check("Minden idegen kulcs érvényes", conn.execute("PRAGMA foreign_key_check").fetchall() == [])
            conn.execute("PRAGMA foreign_keys=ON")
            try:
                conn.execute("DELETE FROM Ugyfelek WHERE Id=?", (ca["id"],))
                restricted = False
            except sqlite3.IntegrityError:
                restricted = True
            check("Előzményeket védő idegen kulcsok", restricted)
            conn.rollback()
            rows = conn.execute("SELECT count(*) FROM Javitasok").fetchone()[0]
            users = conn.execute("SELECT count(*) FROM Felhasznalok").fetchone()[0]

    with Server(legacy, directory) as server:
        manager = server.client(); manager.login("manager", "Manager123!")
        check("Újraindítás után megmaradó javítások", len(manager.test("Perzisztencia újraindítás után", "GET", "/api/javitasok")) == rows)
        with closing(sqlite3.connect(legacy)) as conn:
            check("Újraindítás nem duplázza a fiókokat", conn.execute("SELECT count(*) FROM Felhasznalok").fetchone()[0] == users)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    try:
        with tempfile.TemporaryDirectory(prefix="szerviz-backend-") as temp:
            scenario(Path(temp))
    finally:
        if args.report:
            args.report.parent.mkdir(parents=True, exist_ok=True)
            args.report.write_text(json.dumps(RESULTS, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"\n{len(RESULTS)} ellenőrzés sikeres.")
