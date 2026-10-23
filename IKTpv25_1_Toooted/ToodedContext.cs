using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

public class ToodedContext : DbContext
{
    public DbSet<Toode> Toodetabel => Set<Toode>();
    public DbSet<Kategooria> Kategooriatabel => Set<Kategooria>();
    public DbSet<Kasutaja> Kasutajad => Set<Kasutaja>();
    public DbSet<Klient> Kliendid => Set<Klient>();
    public DbSet<Ost> Ostud => Set<Ost>();
    public DbSet<OstuRida> OstuRead => Set<OstuRida>();
    public DbSet<OstunimekirjaRida> Ostunimekiri => Set<OstunimekirjaRida>();
    public DbSet<Kampaania> Kampaaniad => Set<Kampaania>();
    public DbSet<Saavutus> Saavutused => Set<Saavutus>();
    public DbSet<Partii> Partiid => Set<Partii>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // SQLite: andmebaas on fail AppData\Local\ToodedApp\tooded.db (LocalDB-d pole teises arvutis vaja)
        Directory.CreateDirectory(Kaustad.Juur);
        optionsBuilder.UseSqlite($"Data Source={Kaustad.AndmebaasiFail}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kategooria>().ToTable("Kategooriatabel");
        modelBuilder.Entity<Toode>().ToTable("Toodetabel");

        // Kategooriat, milles on tooteid, ei saa kustutada (muidu kustuksid ka tooted)
        modelBuilder.Entity<Toode>()
            .HasOne(t => t.Kategooria)
            .WithMany(k => k.Tooded)
            .HasForeignKey(t => t.KategooriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kliendi kustutamisel jäävad tema ostud alles (ilma kliendita)
        modelBuilder.Entity<Ost>()
            .HasOne(o => o.Klient)
            .WithMany(k => k.Ostud)
            .HasForeignKey(o => o.KlientId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ostu kustutamisel kustuvad ka selle read
        modelBuilder.Entity<OstuRida>()
            .HasOne(r => r.Ost)
            .WithMany(o => o.Read)
            .HasForeignKey(r => r.OstId)
            .OnDelete(DeleteBehavior.Cascade);

        // Müügirida viitab tootele (tagastamiseks). Toote kustutamisel jääb rida alles.
        modelBuilder.Entity<OstuRida>()
            .HasOne<Toode>()
            .WithMany()
            .HasForeignKey(r => r.ToodeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Kasutaja kliendikaart: kliendi kustutamisel jääb kasutaja alles (ilma kaardita)
        modelBuilder.Entity<Kasutaja>()
            .HasOne(k => k.Klient)
            .WithMany()
            .HasForeignKey(k => k.KlientId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ostunimekiri: kasutaja kustutamisel kustub ka tema nimekiri
        modelBuilder.Entity<OstunimekirjaRida>().ToTable("Ostunimekiri");
        modelBuilder.Entity<OstunimekirjaRida>()
            .HasOne<Kasutaja>()
            .WithMany()
            .HasForeignKey(r => r.KasutajaId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OstunimekirjaRida>()
            .HasOne<Toode>()
            .WithMany()
            .HasForeignKey(r => r.ToodeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Kampaania ja partii kustuvad koos tootega
        modelBuilder.Entity<Kampaania>()
            .HasOne(k => k.Toode)
            .WithMany()
            .HasForeignKey(k => k.ToodeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Partii>()
            .HasOne(p => p.Toode)
            .WithMany()
            .HasForeignKey(p => p.ToodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Saavutus kustub koos kliendiga; sama koodi saab klient ainult ühe korra
        modelBuilder.Entity<Saavutus>()
            .HasOne(s => s.Klient)
            .WithMany()
            .HasForeignKey(s => s.KlientId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Saavutus>()
            .HasIndex(s => new { s.KlientId, s.Kood })
            .IsUnique();

        // Algandmed: kategooriad (Id 1-3 jäävad samaks, et vanad tooted ei kaoks)
        modelBuilder.Entity<Kategooria>().HasData(
            new Kategooria { Id = 1, Kategooria_nimetus = "Piimatooted, munad, juust" },
            new Kategooria { Id = 2, Kategooria_nimetus = "Leivad, saiad, kondiitritooted" },
            new Kategooria { Id = 3, Kategooria_nimetus = "Puuviljad, köögiviljad" },
            new Kategooria { Id = 4, Kategooria_nimetus = "Liha- ja kalatooted" },
            new Kategooria { Id = 5, Kategooria_nimetus = "Joogid" },
            new Kategooria { Id = 6, Kategooria_nimetus = "Maiustused ja snäkid" },
            new Kategooria { Id = 7, Kategooria_nimetus = "Külmutatud toidukaubad" });

        // ... ja tooted koos piltide ja toote andmetega (pildid kaustas Pildid)
        modelBuilder.Entity<Toode>().HasData(
            new Toode
            {
                Id = 1, Toodenimetus = "Juust Atleet Eesti viilutatud Valio 500g", Kogus = 40,
                Hind = 2.99m, VanaHind = 6.19m, Pilt = "atleet.png", KategooriaId = 1,
                Kaubamark = "Valio", Tootja = "Valio Eesti AS", Paritolumaa = "Eesti",
                Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m,
                Kirjeldus = "Eesti juustuklassika aastast 1984. Viilutatud juust taassuletavas pakendis – laktoosivaba, kaltsiumi- ja valgurikas.",
                Koostisosad = "Pastöriseeritud PIIM, sool, piimhappebakterid, laap, värvaine annatto.",
                Lisainfo = "Laktoosivaba (laktoosi alla 0,01 g/100 g).\nMin säilitustemperatuur: 2° C\nMax säilitustemperatuur: 6° C",
                Toitevaartus = "energiasisaldus;1490 kJ / 360 kcal\nrasvad;27 g\n-millest küllastunud rasvhapped;17 g\nsüsivesikud;0 g\n-millest suhkrud;0 g\nvalgud;28 g\nsool;1.3 g"
            },
            new Toode
            {
                Id = 2, Toodenimetus = "Õrrekanade munad M Kulli Farm 10tk", Kogus = 60,
                Hind = 2.89m, Pilt = "munad.png", KategooriaId = 1,
                Kaubamark = "Kulli Farm", Tootja = "DAVA Foods Estonia AS", Paritolumaa = "Eesti",
                Pakend = "10 tk", Uhik = "tk", UhikuKogus = 10m,
                Kirjeldus = "Eesti õrrekanade munad, suurus M, A-klass.",
                Koostisosad = "Kanamunad.",
                Lisainfo = "Min säilitustemperatuur: 5° C\nMax säilitustemperatuur: 18° C",
                Toitevaartus = "energiasisaldus;600 kJ / 144 kcal\nrasvad;10 g\n-millest küllastunud rasvhapped;2.8 g\nsüsivesikud;0.6 g\n-millest suhkrud;0.6 g\nvalgud;12.5 g\nsool;0.3 g"
            },
            new Toode
            {
                Id = 3, Toodenimetus = "Kodujuust murakamoosiga Alma 200g", Kogus = 35,
                Hind = 1.39m, VanaHind = 1.79m, Pilt = "kodujuust.png", KategooriaId = 1,
                Kaubamark = "Alma", Tootja = "Valio Eesti AS", Paritolumaa = "Eesti",
                Pakend = "0.2 kg", Uhik = "kg", UhikuKogus = 0.2m,
                Kirjeldus = "Valgurikas kodujuust murakamoosiga.",
                Koostisosad = "Kodujuust (PIIM, sool, piimhappebakterid), murakamoos (suhkur, murakad, vesi, paksendaja pektiin).",
                Lisainfo = "Min säilitustemperatuur: 2° C\nMax säilitustemperatuur: 6° C",
                Toitevaartus = "energiasisaldus;540 kJ / 129 kcal\nrasvad;3.5 g\n-millest küllastunud rasvhapped;2.2 g\nsüsivesikud;12 g\n-millest suhkrud;11 g\nvalgud;11 g\nsool;0.5 g"
            },
            new Toode
            {
                Id = 4, Toodenimetus = "Täispiim 3,8-4,2% Farmi 2L", Kogus = 50,
                Hind = 2.19m, Pilt = "taispiim.png", KategooriaId = 1,
                Kaubamark = "Farmi", Tootja = "Farmi Piimatööstus AS", Paritolumaa = "Eesti",
                Pakend = "2 l", Uhik = "l", UhikuKogus = 2m,
                Kirjeldus = "Eesti farmide pastöriseeritud täispiim.",
                Koostisosad = "Pastöriseeritud täisPIIM (rasvasisaldus 3,8-4,2%).",
                Lisainfo = "Pärast avamist tarvitada 4 päeva jooksul.\nMin säilitustemperatuur: 2° C\nMax säilitustemperatuur: 6° C",
                Toitevaartus = "energiasisaldus;270 kJ / 65 kcal\nrasvad;3.9 g\n-millest küllastunud rasvhapped;2.5 g\nsüsivesikud;4.7 g\n-millest suhkrud;4.7 g\nvalgud;3.2 g\nsool;0.1 g"
            },
            new Toode
            {
                Id = 5, Toodenimetus = "Rukkileib Rehe Eesti Pagar 450g", Kogus = 45,
                Hind = 1.69m, Pilt = "rehe.png", KategooriaId = 2,
                Kaubamark = "Eesti Pagar", Tootja = "Eesti Pagar AS", Paritolumaa = "Eesti",
                Pakend = "0.45 kg", Uhik = "kg", UhikuKogus = 0.45m,
                Kirjeldus = "Tõsine rukkileib 100% Eesti rukkist, E-ainete vaba.",
                Koostisosad = "RUKKIjahu, vesi, rukkilinnased, suhkur, sool, köömned.",
                Lisainfo = "Võib sisaldada NISU jälgi.\nMin säilitustemperatuur: 2° C\nMax säilitustemperatuur: 30° C",
                Toitevaartus = "energiasisaldus;960 kJ / 228 kcal\nrasvad;1.2 g\n-millest küllastunud rasvhapped;0.2 g\nsüsivesikud;45 g\n-millest suhkrud;6 g\nvalgud;6 g\nsool;1.1 g"
            },
            new Toode
            {
                Id = 6, Toodenimetus = "Sai Perenaise suur Eesti Pagar 500g", Kogus = 30,
                Hind = 1.25m, Pilt = "perenaise.png", KategooriaId = 2,
                Kaubamark = "Eesti Pagar", Tootja = "Eesti Pagar AS", Paritolumaa = "Eesti",
                Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m,
                Kirjeldus = "Klassikaline lauasai. Suurem päts - rohkem rõõmu!",
                Koostisosad = "NISUjahu, vesi, rapsiõli, suhkur, sool 1,2%, antioksüdant: E300",
                Lisainfo = "võib sisaldada piima jääke\nMin säilitustemperatuur: 2° C\nMax säilitustemperatuur: 30° C",
                Toitevaartus = "energiasisaldus;1141 kJ / 270 kcal\nrasvad;2.9 g\n-millest küllastunud rasvhapped;0.1 g\nsüsivesikud;51.8 g\n-millest suhkrud;2.6 g\nvalgud;7.7 g\nsool;1.2 g"
            },

            // Tooted e-poest (hinnad 23.10.2026). Pildid laaditakse esimesel avamisel internetist
            // ja hoitakse kaustas AppData\Local\ToodedApp\Images (vt PildiVahemalu).
            new Toode { Id = 7, Toodenimetus = "Toorjuust laktoosivaba Piimameister Otto 400g", Kogus = 32, Hind = 2.79m, VanaHind = 3.59m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_4012516_PCE_EE", KategooriaId = 1, Kaubamark = "Piimameister Otto", Paritolumaa = "Eesti", Pakend = "0.4 kg", Uhik = "kg", UhikuKogus = 0.4m },
            new Toode { Id = 8, Toodenimetus = "Vabapidamisel kanade munad Kodutalu 10tk", Kogus = 40, Hind = 2.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_978403_PCE_EE", KategooriaId = 1, Kaubamark = "Kodutalu", Paritolumaa = "Eesti", Pakend = "10 tk", Uhik = "tk", UhikuKogus = 10m },
            new Toode { Id = 9, Toodenimetus = "Piim Alma 2,5% 1,5l", Kogus = 70, Hind = 1.39m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_200280_PCE_EE", KategooriaId = 1, Kaubamark = "Alma", Paritolumaa = "Eesti", Pakend = "1.5 l", Uhik = "l", UhikuKogus = 1.5m },
            new Toode { Id = 10, Toodenimetus = "Hapukoor Alma 20% 500g", Kogus = 36, Hind = 1.89m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_200279_PCE_EE", KategooriaId = 1, Kaubamark = "Alma", Paritolumaa = "Eesti", Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m },
            new Toode { Id = 11, Toodenimetus = "Köögikoor Farmi 15% 400ml", Kogus = 28, Hind = 1.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_962520_PCE_EE", KategooriaId = 1, Kaubamark = "Farmi", Paritolumaa = "Eesti", Pakend = "0.4 l", Uhik = "l", UhikuKogus = 0.4m },
            new Toode { Id = 12, Toodenimetus = "Riivjuust Eesti Estover 25,2% 200g", Kogus = 22, Hind = 2.39m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_266460_PCE_EE", KategooriaId = 1, Kaubamark = "Estover", Paritolumaa = "Eesti", Pakend = "0.2 kg", Uhik = "kg", UhikuKogus = 0.2m },
            new Toode { Id = 13, Toodenimetus = "Või MO Saaremaa 82% 200g", Kogus = 30, Hind = 2.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_291032_PCE_EE", KategooriaId = 1, Kaubamark = "Saaremaa", Paritolumaa = "Eesti", Pakend = "0.2 kg", Uhik = "kg", UhikuKogus = 0.2m },
            new Toode { Id = 14, Toodenimetus = "Juustupulgad Pik-Nik 40% 280g", Kogus = 18, Hind = 3.79m, VanaHind = 5.59m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_249144_PCE_EE", KategooriaId = 1, Kaubamark = "Pik-Nik", Paritolumaa = "Eesti", Pakend = "0.28 kg", Uhik = "kg", UhikuKogus = 0.28m },
            new Toode { Id = 15, Toodenimetus = "Keefir Valio Gefilus 1kg", Kogus = 45, Hind = 1.29m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_200277_PCE_EE", KategooriaId = 1, Kaubamark = "Valio", Paritolumaa = "Eesti", Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 16, Toodenimetus = "Koorene jogurt must kirss Farmi 400g", Kogus = 26, Hind = 0.99m, VanaHind = 1.79m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_272890_PCE_EE", KategooriaId = 1, Kaubamark = "Farmi", Paritolumaa = "Eesti", Pakend = "0.4 kg", Uhik = "kg", UhikuKogus = 0.4m },
            new Toode { Id = 17, Toodenimetus = "Kreeka jogurt maitsestamata Tere 350g", Kogus = 7, Hind = 1.29m, VanaHind = 1.65m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_952654_PCE_EE", KategooriaId = 1, Kaubamark = "Tere", Paritolumaa = "Eesti", Pakend = "0.35 kg", Uhik = "kg", UhikuKogus = 0.35m },
            new Toode { Id = 18, Toodenimetus = "Vahukoor Alma 35% 400ml", Kogus = 24, Hind = 1.99m, VanaHind = 2.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_205760_PCE_EE", KategooriaId = 1, Kaubamark = "Alma", Paritolumaa = "Eesti", Pakend = "0.4 l", Uhik = "l", UhikuKogus = 0.4m },
            new Toode { Id = 19, Toodenimetus = "Kohuke mustsõstra-laimi kakaogl. Alma 40g", Kogus = 90, Hind = 0.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_4012328_PCE_EE", KategooriaId = 1, Kaubamark = "Alma", Paritolumaa = "Eesti", Pakend = "0.04 kg", Uhik = "kg", UhikuKogus = 0.04m },
            new Toode { Id = 20, Toodenimetus = "Leib rukkipala Leibur 330g", Kogus = 40, Hind = 0.98m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_205052_PCE_EE", KategooriaId = 2, Kaubamark = "Leibur", Paritolumaa = "Eesti", Pakend = "0.33 kg", Uhik = "kg", UhikuKogus = 0.33m },
            new Toode { Id = 21, Toodenimetus = "Röstsai Tosta Eesti Pagar 500g", Kogus = 35, Hind = 1.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_274018_PCE_EE", KategooriaId = 2, Kaubamark = "Eesti Pagar", Paritolumaa = "Eesti", Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m },
            new Toode { Id = 22, Toodenimetus = "Röst Mitmevilja Leibur 470g", Kogus = 20, Hind = 1.75m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_290851_PCE_EE", KategooriaId = 2, Kaubamark = "Leibur", Paritolumaa = "Eesti", Pakend = "0.47 kg", Uhik = "kg", UhikuKogus = 0.47m },
            new Toode { Id = 23, Toodenimetus = "Koorikleib Kodukandi Eesti Pagar 300g", Kogus = 25, Hind = 1.05m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_966927_PCE_EE", KategooriaId = 2, Kaubamark = "Eesti Pagar", Paritolumaa = "Eesti", Pakend = "0.3 kg", Uhik = "kg", UhikuKogus = 0.3m },
            new Toode { Id = 24, Toodenimetus = "Leib Peremehe pärmivaba Eesti Pagar 600g", Kogus = 30, Hind = 1.1m, VanaHind = 1.39m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_975422_PCE_EE", KategooriaId = 2, Kaubamark = "Eesti Pagar", Paritolumaa = "Eesti", Pakend = "0.6 kg", Uhik = "kg", UhikuKogus = 0.6m },
            new Toode { Id = 25, Toodenimetus = "Ruks Leibur 390g", Kogus = 4, Hind = 1.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_279166_PCE_EE", KategooriaId = 2, Kaubamark = "Leibur", Paritolumaa = "Eesti", Pakend = "0.39 kg", Uhik = "kg", UhikuKogus = 0.39m },
            new Toode { Id = 26, Toodenimetus = "Rukkisepik Eesti Pagar 300g", Kogus = 28, Hind = 1.09m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_298347_PCE_EE", KategooriaId = 2, Kaubamark = "Eesti Pagar", Paritolumaa = "Eesti", Pakend = "0.3 kg", Uhik = "kg", UhikuKogus = 0.3m },
            new Toode { Id = 27, Toodenimetus = "Rukkiröst Tosta Eesti Pagar 390g", Kogus = 22, Hind = 1.15m, VanaHind = 1.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_286883_PCE_EE", KategooriaId = 2, Kaubamark = "Eesti Pagar", Paritolumaa = "Eesti", Pakend = "0.39 kg", Uhik = "kg", UhikuKogus = 0.39m },
            new Toode { Id = 28, Toodenimetus = "Must Tume leib juuretisega Fazer 500g", Kogus = 18, Hind = 1.39m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_4008650_PCE_EE", KategooriaId = 2, Kaubamark = "Fazer", Paritolumaa = "Eesti", Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m },
            new Toode { Id = 29, Toodenimetus = "Keefirisai kaltsiumiga Fazer 350g", Kogus = 16, Hind = 1.67m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_920924_PCE_EE", KategooriaId = 2, Kaubamark = "Fazer", Paritolumaa = "Eesti", Pakend = "0.35 kg", Uhik = "kg", UhikuKogus = 0.35m },
            new Toode { Id = 30, Toodenimetus = "Jaapani kõrvits praadimiseks, kg", Kogus = 25, Hind = 1.49m, VanaHind = 2.39m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_968932_KGM_EE", KategooriaId = 3, Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 31, Toodenimetus = "Banaan Cavendish 1kl, kg", Kogus = 80, Hind = 1.29m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_270939_KGM_EE", KategooriaId = 3, Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 32, Toodenimetus = "Paprika punane 1kl, kg", Kogus = 30, Hind = 2.69m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_269167_KGM_EE", KategooriaId = 3, Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 33, Toodenimetus = "Kurk Luunja kg", Kogus = 20, Hind = 4.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_275290_KGM_EE", KategooriaId = 3, Kaubamark = "Luunja", Paritolumaa = "Eesti", Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 34, Toodenimetus = "Porgand pestud, kg Eesti", Kogus = 60, Hind = 0.46m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_276076_KGM_EE", KategooriaId = 3, Paritolumaa = "Eesti", Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 35, Toodenimetus = "Sidrun Eureka 1kl kg", Kogus = 35, Hind = 2.09m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_969226_KGM_EE", KategooriaId = 3, Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 36, Toodenimetus = "Ploomtomat punane mini 1kl, 250g", Kogus = 0, Hind = 2.29m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_296781_PCE_EE", KategooriaId = 3, Pakend = "0.25 kg", Uhik = "kg", UhikuKogus = 0.25m },
            new Toode { Id = 37, Toodenimetus = "Viinamari hele s.-ta Sultan 1kl kg", Kogus = 24, Hind = 1.49m, VanaHind = 3.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_269179_KGM_EE", KategooriaId = 3, Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 38, Toodenimetus = "Beebispinat VEG 1kl, 100g", Kogus = 15, Hind = 1.59m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_947057_PCE_EE", KategooriaId = 3, Pakend = "0.1 kg", Uhik = "kg", UhikuKogus = 0.1m },
            new Toode { Id = 39, Toodenimetus = "Mandariin Nadorcott 1kl kg", Kogus = 40, Hind = 2.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_211248_KGM_EE", KategooriaId = 3, Pakend = "1 kg", Uhik = "kg", UhikuKogus = 1m },
            new Toode { Id = 40, Toodenimetus = "Verivorstid maasuitsusingiga Rannarootsi 500g", Kogus = 30, Hind = 3.59m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_941450_PCE_EE", KategooriaId = 4, Kaubamark = "Rannarootsi", Paritolumaa = "Eesti", Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m },
            new Toode { Id = 41, Toodenimetus = "Verivorstid peened Rakvere 350g", Kogus = 26, Hind = 3.09m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_4009220_PCE_EE", KategooriaId = 4, Kaubamark = "Rakvere", Paritolumaa = "Eesti", Pakend = "0.35 kg", Uhik = "kg", UhikuKogus = 0.35m },
            new Toode { Id = 42, Toodenimetus = "Verivorst peekoniga Pühade Rakvere 500g", Kogus = 20, Hind = 3.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_270141_PCE_EE", KategooriaId = 4, Kaubamark = "Rakvere", Paritolumaa = "Eesti", Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m },
            new Toode { Id = 43, Toodenimetus = "Tanguvorstid suitsupeekoniga Rannarootsi 500g", Kogus = 18, Hind = 3.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_4005458_PCE_EE", KategooriaId = 4, Kaubamark = "Rannarootsi", Paritolumaa = "Eesti", Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m },
            new Toode { Id = 44, Toodenimetus = "Verivorst E-vaba Pöialpoiss Oskar 500g", Kogus = 6, Hind = 3.29m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_270147_PCE_EE", KategooriaId = 4, Kaubamark = "Oskar", Paritolumaa = "Eesti", Pakend = "0.5 kg", Uhik = "kg", UhikuKogus = 0.5m },
            new Toode { Id = 45, Toodenimetus = "Grillribi röst. küüslaugu eelküpsetatud Rannarootsi 900g", Kogus = 12, Hind = 10.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_4012753_PCE_EE", KategooriaId = 4, Kaubamark = "Rannarootsi", Paritolumaa = "Eesti", Pakend = "0.9 kg", Uhik = "kg", UhikuKogus = 0.9m },
            new Toode { Id = 46, Toodenimetus = "Alkoholivaba glögi õuna-mustsõstra Aura 1l", Kogus = 40, Hind = 1.95m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_173914_PCE_EE", KategooriaId = 5, Kaubamark = "Aura", Paritolumaa = "Eesti", Pakend = "1 l", Uhik = "l", UhikuKogus = 1m },
            new Toode { Id = 47, Toodenimetus = "Vitamiinivesi Vitamineral Pro Hydration 0,5l", Kogus = 48, Hind = 1.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1376489_PCE_EE", KategooriaId = 5, Pant = 0.10m, Kaubamark = "Vitamineral", Pakend = "0.5 l", Uhik = "l", UhikuKogus = 0.5m },
            new Toode { Id = 48, Toodenimetus = "Tee assortii Magic Winter Teekanne 72,5g", Kogus = 14, Hind = 5.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1025270_PCE_EE", KategooriaId = 5, Kaubamark = "Teekanne", Pakend = "0.0725 kg", Uhik = "kg", UhikuKogus = 0.0725m },
            new Toode { Id = 49, Toodenimetus = "Karastusjook Cherry & Lime Living Things 0,33l", Kogus = 24, Hind = 2.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1377211_PCE_EE", KategooriaId = 5, Pant = 0.10m, Kaubamark = "Living Things", Pakend = "0.33 l", Uhik = "l", UhikuKogus = 0.33m },
            new Toode { Id = 50, Toodenimetus = "Karastusjook Rasp & Pomegr. Living Things 0,33l", Kogus = 24, Hind = 2.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1377207_PCE_EE", KategooriaId = 5, Pant = 0.10m, Kaubamark = "Living Things", Pakend = "0.33 l", Uhik = "l", UhikuKogus = 0.33m },
            new Toode { Id = 51, Toodenimetus = "Glögi Pumpkin Spice 0,5% Blossa 0,75l", Kogus = 10, Hind = 7.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1377222_PCE_EE", KategooriaId = 5, Kaubamark = "Blossa", Pakend = "0.75 l", Uhik = "l", UhikuKogus = 0.75m },
            new Toode { Id = 52, Toodenimetus = "Vesi sidrunimaitseline, gas. Saaremaa 0,5l", Kogus = 96, Hind = 0.59m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1353600_PCE_EE", KategooriaId = 5, Pant = 0.10m, Kaubamark = "Saaremaa", Paritolumaa = "Eesti", Pakend = "0.5 l", Uhik = "l", UhikuKogus = 0.5m },
            new Toode { Id = 53, Toodenimetus = "Vesi sidrunimaitseline, gas. Saaremaa 1,5l", Kogus = 60, Hind = 0.89m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1353601_PCE_EE", KategooriaId = 5, Pant = 0.10m, Kaubamark = "Saaremaa", Paritolumaa = "Eesti", Pakend = "1.5 l", Uhik = "l", UhikuKogus = 1.5m },
            new Toode { Id = 54, Toodenimetus = "Kohvi advendikalender L'Or 24x5,2g", Kogus = 8, Hind = 14.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1025236_PCE_EE", KategooriaId = 5, Kaubamark = "L'Or", Pakend = "0.125 kg", Uhik = "kg", UhikuKogus = 0.1248m },
            new Toode { Id = 55, Toodenimetus = "Küpsised Digestive suhkruvabad Gullon 400g", Kogus = 30, Hind = 3.29m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_168879_PCE_EE", KategooriaId = 6, Kaubamark = "Gullon", Pakend = "0.4 kg", Uhik = "kg", UhikuKogus = 0.4m },
            new Toode { Id = 56, Toodenimetus = "Küpsised šokolaadi tükkidega Gullon 125g", Kogus = 25, Hind = 2.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_168225_PCE_EE", KategooriaId = 6, Kaubamark = "Gullon", Pakend = "0.125 kg", Uhik = "kg", UhikuKogus = 0.125m },
            new Toode { Id = 57, Toodenimetus = "Pralineekompvekid Favourites Fazer 430g", Kogus = 15, Hind = 10.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1024530_PCE_EE", KategooriaId = 6, Kaubamark = "Fazer", Pakend = "0.43 kg", Uhik = "kg", UhikuKogus = 0.43m },
            new Toode { Id = 58, Toodenimetus = "Piimašokolaad Lindt Teddy 40g", Kogus = 20, Hind = 4.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1010603_PCE_EE", KategooriaId = 6, Kaubamark = "Lindt", Pakend = "0.04 kg", Uhik = "kg", UhikuKogus = 0.04m },
            new Toode { Id = 59, Toodenimetus = "Šokolaadikommid Grand Ferrero Rocher 125g", Kogus = 12, Hind = 11.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1010761_PCE_EE", KategooriaId = 6, Kaubamark = "Ferrero", Pakend = "0.125 kg", Uhik = "kg", UhikuKogus = 0.125m },
            new Toode { Id = 60, Toodenimetus = "Šokolaadi jõulumaius Maltesers 29g", Kogus = 50, Hind = 1.29m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1015234_PCE_EE", KategooriaId = 6, Kaubamark = "Maltesers", Pakend = "0.029 kg", Uhik = "kg", UhikuKogus = 0.029m },
            new Toode { Id = 61, Toodenimetus = "Piimašokolaad Milka Santa 45g", Kogus = 40, Hind = 1.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1010441_PCE_EE", KategooriaId = 6, Kaubamark = "Milka", Pakend = "0.045 kg", Uhik = "kg", UhikuKogus = 0.045m },
            new Toode { Id = 62, Toodenimetus = "Piimašokolaad sarapuupähklitega Lacmi 90g", Kogus = 30, Hind = 2.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_1022015_PCE_EE", KategooriaId = 6, Kaubamark = "Lacmi", Pakend = "0.09 kg", Uhik = "kg", UhikuKogus = 0.09m },
            new Toode { Id = 63, Toodenimetus = "Advendikalender Haribo 300g", Kogus = 9, Hind = 15.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_196075_PCE_EE", KategooriaId = 6, Kaubamark = "Haribo", Pakend = "0.3 kg", Uhik = "kg", UhikuKogus = 0.3m },
            new Toode { Id = 64, Toodenimetus = "Jäätis vanilli šokolaadiglasuuriga Vanilla Ninja 80g", Kogus = 60, Hind = 1.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_206273_PCE_EE", KategooriaId = 7, Kaubamark = "Vanilla Ninja", Paritolumaa = "Eesti", Pakend = "0.08 kg", Uhik = "kg", UhikuKogus = 0.08m },
            new Toode { Id = 65, Toodenimetus = "Jäätis šokolaadi šokolaadiglasuuriga Vanilla Ninja 80g", Kogus = 55, Hind = 1.19m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_910463_PCE_EE", KategooriaId = 7, Kaubamark = "Vanilla Ninja", Paritolumaa = "Eesti", Pakend = "0.08 kg", Uhik = "kg", UhikuKogus = 0.08m },
            new Toode { Id = 66, Toodenimetus = "Pelmeenid Hiina Uvic 700g", Kogus = 22, Hind = 4.29m, VanaHind = 5.49m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_273639_PCE_EE", KategooriaId = 7, Kaubamark = "Uvic", Paritolumaa = "Eesti", Pakend = "0.7 kg", Uhik = "kg", UhikuKogus = 0.7m },
            new Toode { Id = 67, Toodenimetus = "Pelmeenid Rakvere 900g", Kogus = 18, Hind = 4.59m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_908407_PCE_EE", KategooriaId = 7, Kaubamark = "Rakvere", Paritolumaa = "Eesti", Pakend = "0.9 kg", Uhik = "kg", UhikuKogus = 0.9m },
            new Toode { Id = 68, Toodenimetus = "Kaneelisaiake Eesti Pagar 8x40g", Kogus = 14, Hind = 2.89m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_296476_PCE_EE", KategooriaId = 7, Kaubamark = "Eesti Pagar", Paritolumaa = "Eesti", Pakend = "0.32 kg", Uhik = "kg", UhikuKogus = 0.32m },
            new Toode { Id = 69, Toodenimetus = "Friikartul Kullakesed sakiline Härmavili 750g", Kogus = 20, Hind = 2.99m, Pilt = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_202677_PCE_EE", KategooriaId = 7, Kaubamark = "Härmavili", Paritolumaa = "Eesti", Pakend = "0.75 kg", Uhik = "kg", UhikuKogus = 0.75m }
);
    }
}
