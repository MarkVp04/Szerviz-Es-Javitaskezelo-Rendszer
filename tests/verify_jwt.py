"""JWT és Swagger Authorize ellenőrzés. Futtatás: python tests/verify_jwt.py"""
import base64
from contextlib import closing
import hashlib
import hmac
import json
from pathlib import Path
import sqlite3
import tempfile
import time
import verify_backend as test


def encode(value):
    return base64.urlsafe_b64encode(json.dumps(value, separators=(",", ":")).encode()).decode().rstrip("=")


def decode(value):
    return json.loads(base64.urlsafe_b64decode(value + "=" * (-len(value) % 4)))


settings = json.loads((test.API / "appsettings.Development.json").read_text(encoding="utf-8-sig"))["JwtSettings"]


def signed(payload, header=None):
    # Kizárólag a nyilvános fejlesztői tesztkulccsal készítünk szándékosan érvénytelen teszttokeneket.
    body = encode(header or {"alg": "HS256", "typ": "JWT"}) + "." + encode(payload)
    signature = hmac.new(settings["SecretKey"].encode(), body.encode(), hashlib.sha256).digest()
    return body + "." + base64.urlsafe_b64encode(signature).decode().rstrip("=")


with tempfile.TemporaryDirectory(prefix="szerviz-jwt-") as directory:
    directory = Path(directory)
    path = directory / "jwt.db"
    with test.Server(path, directory) as server:
        anonymous = server.client()
        anonymous.test("JWT nélkül 401", "GET", "/api/auth/en", 401)
        swagger = anonymous.test("OpenAPI séma", "GET", "/swagger/v1/swagger.json")
        definition = swagger["components"]["securitySchemes"]["Bearer"]
        test.check("A régi mintához illeszkedő Swagger Authorize fejléc", definition["type"] == "apiKey"
                   and definition["name"] == "Authorization" and definition["in"] == "header")
        test.check("A védett API Bearer hitelesítést kér", {"Bearer": []} in swagger["security"])
        test.check("A belépéshez nem kell előzetes token", swagger["paths"]["/api/auth/bejelentkezes"]["post"]["security"] == [])
        test.check("A Services DTO-jai látszanak a sémában", "JavitasDto" in swagger["components"]["schemas"]
                   and "LoginResponseDto" in swagger["components"]["schemas"])

        manager = server.client()
        response = manager.login("manager", "Manager123!")
        test.check("Belépéskor JWT és lejárat érkezik", len(response["token"].split(".")) == 3 and response["lejarat"].endswith("Z"))
        test.check("A régi mintához hasonló belépési adatok", response["felhasznaloId"] == 1 and response["szerepkor"] == "Manager")
        test.check("A belépés nem állít be sütit", not any(k.lower() == "set-cookie" for k in manager.last_headers))
        payload = decode(response["token"].split(".")[1])
        test.check("A token nem tartalmaz jelszóhash-t", "jelszo" not in json.dumps(payload).lower())
        manager.test("Bearer tokennel azonosítható a fiók", "GET", "/api/auth/en")
        anonymous.test("Másik kliens nem lép be automatikusan", "GET", "/api/auth/en", 401)

        invalid = server.client()
        invalid.token = "nem-ervenyes-token"
        invalid.test("Hibás formátumú token elutasítása", "GET", "/api/auth/en", 401)
        pieces = response["token"].split(".")
        pieces[2] = ("A" if pieces[2][0] != "A" else "B") + pieces[2][1:]
        invalid.token = ".".join(pieces)
        invalid.test("Módosított aláírás elutasítása", "GET", "/api/auth/en", 401)
        for name, changes in [
            ("Lejárt token", {"exp": int(time.time()) - 300, "nbf": int(time.time()) - 600}),
            ("Még nem érvényes token", {"nbf": int(time.time()) + 300}),
            ("Hibás kibocsátó", {"iss": "masik-rendszer"}),
            ("Hibás célközönség", {"aud": "masik-kliens"}),
            ("Elavult belépési verzió", {"belepesVerzio": "regi-verzio"}),
        ]:
            invalid.token = signed({**payload, **changes})
            invalid.test(name + " elutasítása", "GET", "/api/auth/en", 401)
        invalid.token = signed(payload, {"alg": "none", "typ": "JWT"})
        invalid.test("Nem engedélyezett algoritmus elutasítása", "GET", "/api/auth/en", 401)
        role = next(k for k in payload if k.endswith("/role"))
        invalid.token = signed({**payload, role: "Szerelo"})
        invalid.test("Adatbázissal nem egyező szerepkör elutasítása", "GET", "/api/auth/en", 401)
        invalid.login("manager", "Manager123!")
        invalid.test("Hibás token után a nyilvános belépés működik", "GET", "/api/auth/en")
        test.check("A külön belépések külön tokent kapnak", invalid.token != manager.token)
        previous_token = manager.token
        manager.test("Kijelentkezés JWT-vel", "POST", "/api/auth/kijelentkezes", 204)
        invalid.test("Kijelentkezés visszavonja az előző tokeneket", "GET", "/api/auth/en", 401)
        replay = server.client(); replay.token = previous_token
        replay.test("Visszavont token újbóli elküldése tiltott", "GET", "/api/auth/en", 401)
        response = manager.login("manager", "Manager123!")
        restart_token = manager.token
        with closing(sqlite3.connect(path)) as db:
            test.check("Az átalakítás nem hozott létre új migrációt", db.execute("SELECT count(*) FROM __EFMigrationsHistory").fetchone()[0] == 2)

    with test.Server(path, directory) as server:
        same_user = server.client(); same_user.token = restart_token
        same_user.test("Érvényes JWT újraindítás után is működik", "GET", "/api/auth/en")

print(f"\n{len(test.RESULTS)} JWT-ellenőrzés sikeres.")
