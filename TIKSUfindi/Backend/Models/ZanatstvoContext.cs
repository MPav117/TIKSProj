using Models;

namespace WebTemplate.Models;

public class ZanatstvoContext : DbContext
{
    public DbSet<Identitet> Identiteti { get; set; }
    public DbSet<Korisnik> Korisnici { get; set; }
    public DbSet<Poslodavac> Poslodavci { get; set; }
    public DbSet<Majstor> Majstori { get; set; }
    public DbSet<Oglas> Oglasi { get; set; }
    public DbSet<Ugovor> Ugovori { get; set; }
    public DbSet<Recenzija> Recenzije { get; set; }
    public DbSet<ZahtevZaPosao> ZahteviZaPosao { get; set; }
    public DbSet<ZahtevZaGrupu> ZahteviZaGrupu { get; set; }
    public DbSet<Kalendar> Kalendari { get; set; }
    public DbSet<Grad> Gradovi { get; set; }
    public DbSet<MajstorOglas> MajstoriOglasi { get; set; }
    //public DbSet<ChatHistory> Chats { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }

    public ZanatstvoContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ZahtevZaGrupu>()
            .HasOne(z => z.MajstorPrimalac)
            .WithMany(m => m.ZahteviGrupaPrimljeni)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ZahtevZaGrupu>()
            .HasOne(z => z.MajstorPosiljalac)
            .WithMany(m => m.ZahteviGrupaPoslati)
            .OnDelete(DeleteBehavior.NoAction);

        //modelBuilder.Entity<Majstor>()
        //   .HasMany(m => m.Oglasi)
        //  .WithMany(o => o.PrijavljeniMajstori);
        modelBuilder.Entity<MajstorOglas>()
      .HasOne(mo => mo.Oglas)
      .WithMany(o => o.OglasiMajstor)
      .HasForeignKey(mo => mo.OglasID)
      .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MajstorOglas>()
            .HasOne(mo => mo.Majstor)
            .WithMany(m => m.MajstoriOglas)
            .HasForeignKey(mo => mo.MajstorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ZahtevZaPosao>()
        .HasOne(z => z.Poslodavac)
        .WithMany(p => p.Zahtevi)
        //.HasForeignKey(z => z.ID)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Ugovor>()
         .HasOne(u => u.Poslodavac)
         .WithMany(p => p.Ugovori)
         //.HasForeignKey(u => u.PoslodavacID)
         .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Ugovor>()
        .HasOne(u => u.ZahtevZaPosao)
        .WithMany()
        //.HasForeignKey<Ugovor>(u => u.ZahtevZaPosaoID)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Recenzija>()
        .HasOne(r => r.Primalac)
        .WithMany(k => k.PrimljeneRecenzije)
        //.HasForeignKey(r => r.PrimalacID)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Recenzija>()
        .HasOne(r => r.Davalac)
        .WithMany(k => k.PoslateRecenzije)
        //.HasForeignKey(r => r.DavalacID)
        .OnDelete(DeleteBehavior.NoAction);

        /*modelBuilder.Entity<ChatHistory>()
        .HasOne(c => c.KorisnikPosiljalac)
        .WithMany(k => k.ChatPoslate)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ChatHistory>()
        .HasOne(c => c.KorisnikPrimalac)
        .WithMany(k => k.ChatPrimljene)
        .OnDelete(DeleteBehavior.NoAction);*/

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Sender)
            .WithMany(k => k.ChatPoslate)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Receiver)
            .WithMany(k => k.ChatPrimljene)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}