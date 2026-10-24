using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Demoandmed esitluseks: 3 klienti ja 60 ostu viimase 30 päeva jooksul.
// Kui poes on Rimi kataloog, ostetakse kataloogi tooteid (uusi tooteid ei looda);
// tühja poe korral lisatakse ka 50 demotoodet. Kõik on märgitud Demo = true ja need saab ühe nupuga eemaldada.
public static class DemoAndmed
{
    public const int TooteArv = 50, OstuArv = 60;

    // Suur kataloog (Rimi) on olemas → demo kasutab selle tooteid
    public static bool KasutaKataloogi(ToodedContext db) => db.Toodetabel.Count(t => !t.Demo) >= 200;

    // (kategooria, nimi, hind, Eesti, märgised, pant, toiteväärtus 100 g kohta: kcal/rasv/süsiv/valk, pakend)
    private static readonly (int Kat, string Nimi, decimal Hind, bool Eesti, string? Margid, decimal Pant, string? Toit, string Pakend)[] Tooted =
    {
        (1, "Kefiir 2,5% Tere 1l", 1.19m, true, null, 0, "56;2.5;4;3.2", "1 l"),
        (1, "Hapupiim Alma 2,5% 1l", 1.09m, true, null, 0, "52;2.5;4;3", "1 l"),
        (1, "Juust Eesti 26% Estover 400g", 3.99m, true, "laktoosivaba", 0, "330;26;0;24", "0.4 kg"),
        (1, "Kohupiim 5% Farmi 250g", 1.29m, true, null, 0, "120;5;3;17", "0.25 kg"),
        (1, "Jogurt maasika Valio Profeel 180g", 0.99m, true, null, 0, "70;0.2;9;9", "0.18 kg"),
        (1, "Või soolata Farmi 82% 180g", 2.69m, true, null, 0, "740;82;0.6;0.6", "0.18 kg"),
        (1, "Kaerajook Oddlygood 1l", 2.19m, false, "laktoosivaba;vegan", 0, "46;1.5;7;0.6", "1 l"),
        (2, "Saiake kaneeliga Leibur 4x60g", 1.99m, true, null, 0, "340;12;50;7", "0.24 kg"),
        (2, "Leib Hiiu rukkileib Hiiu Pagar 500g", 1.89m, true, "vegan", 0, "230;1.5;44;6.5", "0.5 kg"),
        (2, "Sai täistera Eesti Pagar 400g", 1.39m, true, "vegan", 0, "250;3;45;9", "0.4 kg"),
        (2, "Croissant võiga Fazer 4x50g", 2.49m, false, null, 0, "410;22;45;8", "0.2 kg"),
        (2, "Pirukas kapsaga Eesti Pagar 85g", 0.79m, true, null, 0, "260;10;35;6", "0.085 kg"),
        (2, "Maasikakook Kalev 400g", 4.49m, true, null, 0, "290;14;37;4", "0.4 kg"),
        (2, "Gluteenivaba leib Leibur 300g", 2.99m, true, "gluteenivaba;vegan", 0, "220;5;38;4", "0.3 kg"),
        (3, "Õun Eesti Liivika, kg", 1.49m, true, null, 0, "52;0.2;14;0.3", "1 kg"),
        (3, "Kartul Eesti, kg", 0.69m, true, null, 0, "77;0.1;17;2", "1 kg"),
        (3, "Tomat kobar, kg", 3.29m, false, null, 0, "18;0.2;3.9;0.9", "1 kg"),
        (3, "Kapsas valge Eesti, kg", 0.59m, true, null, 0, "25;0.1;6;1.3", "1 kg"),
        (3, "Pirn Conference, kg", 2.19m, false, null, 0, "57;0.1;15;0.4", "1 kg"),
        (3, "Sibul kollane Eesti, kg", 0.89m, true, null, 0, "40;0.1;9;1.1", "1 kg"),
        (3, "Avokaado Hass 2tk", 2.49m, false, null, 0, null, "2 tk"),
        (4, "Hakkliha sea-veise Rakvere 400g", 3.49m, true, null, 0, "250;20;0;17", "0.4 kg"),
        (4, "Kanafilee Tallegg 500g", 5.99m, true, null, 0, "110;1.5;0;23", "0.5 kg"),
        (4, "Lõhefilee külmutatud 300g", 6.49m, false, null, 0, "200;13;0;20", "0.3 kg"),
        (4, "Sink suitsu Nõo 150g", 2.29m, true, null, 0, "120;4;1;20", "0.15 kg"),
        (4, "Viinerid Rannarootsi 400g", 2.99m, true, null, 0, "260;22;2;12", "0.4 kg"),
        (4, "Räimefilee marinaadis Vettel 250g", 2.59m, true, null, 0, "190;12;6;14", "0.25 kg"),
        (4, "Seakarbonaad Rakvere, kg", 7.99m, true, null, 0, "150;7;0;21", "1 kg"),
        (5, "Mineraalvesi Värska Originaal 1,5l", 1.09m, true, "vegan", 0.10m, "0;0;0;0", "1.5 l"),
        (5, "Õunamahl Aura 1l", 1.79m, true, "vegan", 0.10m, "45;0;11;0", "1 l"),
        (5, "Kali Tartu Pruulikoda 0,5l", 1.29m, true, "vegan", 0.10m, "36;0;9;0", "0.5 l"),
        (5, "Coca-Cola 0,5l", 1.49m, false, "vegan", 0.10m, "42;0;10.6;0", "0.5 l"),
        (5, "Apelsinimehu Granini 1l", 2.29m, false, "vegan", 0.10m, "44;0;10;0.7", "1 l"),
        (5, "Kohv jahvatatud Löfbergs 450g", 6.99m, false, "vegan", 0, null, "0.45 kg"),
        (5, "Energiajook Battery 0,33l", 1.39m, false, "vegan", 0.10m, "47;0;11;0", "0.33 l"),
        (6, "Šokolaad Kalev Mesikäpp 100g", 1.79m, true, null, 0, "550;34;52;7", "0.1 kg"),
        (6, "Kommid Kalev Kirju koer 200g", 2.49m, true, null, 0, "480;25;60;5", "0.2 kg"),
        (6, "Krõpsud Taffel soolased 175g", 2.59m, false, "vegan", 0, "540;33;52;6", "0.175 kg"),
        (6, "Küpsised Selga Kalev 180g", 1.69m, true, null, 0, "460;18;68;7", "0.18 kg"),
        (6, "Pähklisegu Nutline 150g", 3.29m, false, "vegan;gluteenivaba", 0, "600;50;14;20", "0.15 kg"),
        (6, "Vahvlid Kalev 300g", 2.19m, true, null, 0, "500;25;63;6", "0.3 kg"),
        (6, "Marmelaad Haribo Goldbears 200g", 1.99m, false, "gluteenivaba", 0, "343;0.5;77;6.9", "0.2 kg"),
        (7, "Jäätis Eskimo vanilli Premia 120ml", 1.29m, true, null, 0, "230;14;23;3.5", "0.12 l"),
        (7, "Külmutatud herned Põltsamaa 400g", 1.59m, true, "vegan;gluteenivaba", 0, "80;0.4;13;5", "0.4 kg"),
        (7, "Pitsa Margherita Dr. Oetker 350g", 3.49m, false, null, 0, "230;9;27;10", "0.35 kg"),
        (7, "Kalapulgad Iglo 250g", 2.99m, false, null, 0, "200;9;17;12", "0.25 kg"),
        (7, "Marjasegu külmutatud Põltsamaa 300g", 2.49m, true, "vegan;gluteenivaba", 0, "45;0.4;8;1", "0.3 kg"),
        (7, "Pelmeenid Maiasmokk 500g", 3.19m, true, null, 0, "230;10;25;10", "0.5 kg"),
        (7, "Jäätis Tuutu šokolaadi Premia 500ml", 3.69m, true, null, 0, "220;11;27;4", "0.5 l"),
        (7, "Friikartulid Farm Frites 1kg", 2.79m, false, "vegan", 0, "140;4;24;2.5", "1 kg"),
    };

    public static bool OnOlemas(ToodedContext db) => db.Toodetabel.Any(t => t.Demo) || db.Ostud.Any(o => o.Demo);

    // Demopartiide tunnus (Partii-l pole Demo välja): lisamise aeg 01.01.2000
    private static readonly DateTime DemoPartii = new(2000, 1, 1);

    // Mis luuakse (demo akna plaatide jaoks)
    public static int KliendiArv => 3;
    public static int PandiArv => Tooted.Count(t => t.Pant > 0);
    public static int ToiduArv => Tooted.Count(t => t.Toit != null);
    public static int KategooriaArv => Tooted.Select(t => t.Kat).Distinct().Count();

    public static (int Tooteid, int Oste) Arvud(ToodedContext db) => (db.Toodetabel.Count(t => t.Demo), db.Ostud.Count(o => o.Demo));

    // samm(osa) – edenemine 0..1 (edenemisriba jaoks)
    public static string Lisa(ToodedContext db, Action<double>? samm = null)
    {
        if (OnOlemas(db)) return Keel.T("Demoandmed on juba olemas.");
        var juhus = new Random(2026);   // sama tulemus igal korral

        // 1) Tooted (ainult tühja poe korral)
        var tooted = new List<Toode>();
        bool kataloog = KasutaKataloogi(db);
        foreach (var d in Tooted.Where(_ => !kataloog))
        {
            var (uhik, kogus) = Uhik(d.Pakend);
            var t = new Toode
            {
                Toodenimetus = d.Nimi,
                Hind = d.Hind,
                VanaHind = juhus.Next(5) == 0 ? Math.Round(d.Hind * 1.25m, 2) : null,
                Kogus = juhus.Next(0, 41),
                KategooriaId = d.Kat,
                Paritolumaa = d.Eesti ? "Eesti" : "EL",
                Pakend = d.Pakend,
                Uhik = uhik,
                UhikuKogus = kogus,
                Margised = d.Margid,
                Pant = d.Pant,
                Ostuhind = Math.Round(d.Hind * (0.55m + (decimal)juhus.NextDouble() * 0.2m), 2),
                Toitevaartus = d.Toit is string tv ? ToiteTekst(tv) : null,
                Kirjeldus = Keel.T("Demotoode esitluseks."),
                Demo = true
            };
            tooted.Add(t);
        }
        db.Toodetabel.AddRange(tooted);
        db.SaveChanges();
        samm?.Invoke(0.4);

        // 2) Kliendid
        var kliendid = new[] { ("Mari Maasikas", "mari@demo.ee"), ("Jaan Tamm", "jaan@demo.ee"), ("Liis Kask", "liis@demo.ee") }
            .Select(k => { var kl = Kliendikaart.Loo(db, k.Item1, k.Item2); kl.Demo = true; db.SaveChanges(); return kl; }).ToList();   // iga kaart saab oma numbri
        samm?.Invoke(0.55);

        // 3) Ostud: eri päevad ja kellaajad (soojuskaardi jaoks), 2 tagastatud
        var nyyd = DateTime.Now;
        // Kataloogist ~300 toiduainet (kategooriad 1–11), et samad tooted korduksid (prognoos, "ostetakse koos")
        var koikTooted = kataloog
            ? db.Toodetabel.AsNoTracking().Where(t => t.KategooriaId <= 11 && t.Kogus > 0).ToList().OrderBy(_ => juhus.Next()).Take(300).ToList()
            : db.Toodetabel.AsNoTracking().ToList();
        for (int i = 0; i < OstuArv; i++)
        {
            var aeg = nyyd.Date.AddDays(-juhus.Next(0, 30)).AddHours(8 + juhus.Next(0, 14)).AddMinutes(juhus.Next(0, 60));
            if (aeg > nyyd) aeg = nyyd.AddMinutes(-juhus.Next(5, 300));
            var ost = new Ost
            {
                Aeg = aeg,
                Kasutaja = "demo",
                Makstud = true,
                Makseviis = juhus.Next(3) == 0 ? "Sularaha" : "Kaart",
                Tarneviis = juhus.Next(4) == 0 ? Tarne.Kojuvedu : Tarne.Kaasa,
                Klient = juhus.Next(4) == 0 ? null : kliendid[juhus.Next(kliendid.Count)],
                Tagastatud = i == 5 || i == 13,
                Demo = true
            };
            int ridu = juhus.Next(2, 7);
            foreach (var t in koikTooted.OrderBy(_ => juhus.Next()).Take(ridu))
            {
                int kogus = juhus.Next(1, 4);
                ost.Read.Add(new OstuRida { ToodeId = t.Id, Toodenimetus = t.Toodenimetus, Kogus = kogus, Hind = t.Hind, TavaHind = t.VanaHind, Pant = t.Pant });
            }
            ost.Vahesumma = ost.Read.Sum(r => r.Summa + r.PantKokku);
            ost.TarneHind = ost.Tarneviis == Tarne.Kojuvedu ? Tarne.Arvuta(ost.Read.Sum(r => r.Summa)) : 0;
            ost.Summa = ost.Vahesumma + ost.TarneHind;
            ost.MakstudSumma = ost.Summa;
            if (ost.Klient != null && !ost.Tagastatud)
            {
                ost.BoonusLisatud = Tarne.Boonus(ost.Read.Sum(r => r.Summa), 0.01m);
                ost.Klient.Boonus += ost.BoonusLisatud;
            }
            db.Ostud.Add(ost);
            samm?.Invoke(0.55 + 0.45 * (i + 1) / OstuArv);
        }
        db.SaveChanges();

        // 4) "Ilma raiskamiseta" vitriini jaoks: 10 toidukaupa, mille partii aegub täna või homme (−30 %)
        var aeguvad = db.Toodetabel.AsNoTracking().Where(t => (t.KategooriaId == 1 || t.KategooriaId == 2 || t.KategooriaId == 4 || t.KategooriaId == 9) && t.Kogus > 0)
            .Select(t => t.Id).ToList().OrderBy(_ => juhus.Next()).Take(10).ToList();
        foreach (int id in aeguvad)
        {
            var p = Aegumine.LisaPartii(db, id, juhus.Next(2, 9), DateTime.Today.AddDays(juhus.Next(0, 2)));
            p.Lisatud = DemoPartii;
        }
        db.SaveChanges();
        return Keel.F("Lisati {0} toodet, {1} klienti ja {2} ostu.", tooted.Count, kliendid.Count, OstuArv);
    }

    public static string Eemalda(ToodedContext db)
    {
        var ostud = db.Ostud.Where(o => o.Demo).ToList();
        db.Ostud.RemoveRange(ostud);   // read kustuvad kaasa
        db.SaveChanges();
        var kliendid = db.Kliendid.Where(k => k.Demo).ToList();
        db.Kliendid.RemoveRange(kliendid);
        var partiid = db.Partiid.Where(p => p.Lisatud == DemoPartii).ToList();
        db.Partiid.RemoveRange(partiid);
        db.SaveChanges();
        Aegumine.SobitaLaoseis(db, partiid.Select(p => p.ToodeId).Distinct());
        Aegumine.Uuenda(db, DateTime.Now);
        var tooted = db.Toodetabel.Where(t => t.Demo).ToList();
        db.Toodetabel.RemoveRange(tooted);
        db.SaveChanges();
        return Keel.F("Eemaldati {0} toodet, {1} klienti ja {2} ostu.", tooted.Count, kliendid.Count, ostud.Count);
    }

    private static (string Uhik, decimal Kogus) Uhik(string pakend)
    {
        var osad = pakend.Split(' ');
        decimal arv = decimal.Parse(osad[0], System.Globalization.CultureInfo.InvariantCulture);
        return osad[1] switch { "kg" => ("kg", arv), "l" => ("l", arv), _ => ("tk", arv) };
    }

    private static string ToiteTekst(string lyh)
    {
        var v = lyh.Split(';');
        return $"energiasisaldus;{v[0]} kcal\nrasvad;{v[1]} g\nsüsivesikud;{v[2]} g\nvalgud;{v[3]} g";
    }
}
