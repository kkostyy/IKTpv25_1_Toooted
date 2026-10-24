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
    public DbSet<PereKorviRida> PereKorv => Set<PereKorviRida>();

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

        // Pere ühine ostukorv: toote kustutamisel kaob ka rida
        modelBuilder.Entity<PereKorviRida>().ToTable("PereKorv");
        modelBuilder.Entity<PereKorviRida>()
            .HasOne(r => r.Toode).WithMany().HasForeignKey(r => r.ToodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PereKorviRida>().HasIndex(r => r.PereKood);

        // Algandmed: kategooriad (Id 1-3 jäävad samaks, et vanad tooted ei kaoks)
        modelBuilder.Entity<Kategooria>().HasData(
            new Kategooria { Id = 1, Kategooria_nimetus = "Piimatooted, munad, juust" },
            new Kategooria { Id = 2, Kategooria_nimetus = "Leivad, saiad, kondiitritooted" },
            new Kategooria { Id = 3, Kategooria_nimetus = "Puuviljad, köögiviljad" },
            new Kategooria { Id = 4, Kategooria_nimetus = "Liha- ja kalatooted" },
            new Kategooria { Id = 5, Kategooria_nimetus = "Joogid" },
            new Kategooria { Id = 6, Kategooria_nimetus = "Maiustused ja snäkid" },
            new Kategooria { Id = 7, Kategooria_nimetus = "Külmutatud toidukaubad" });

        // Tooted tulevad Rimi e-poe kataloogist (Andmed\rimi_kataloog.json.gz, vt RimiKataloog)
    }
}
