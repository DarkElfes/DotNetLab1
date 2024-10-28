using Bank.Api.Accounts;
using Bank.Api.Cards;
using Bank.Api.Transactions.TransactionModels;
using Microsoft.EntityFrameworkCore;

namespace Bank.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<Transaction> Transactions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasOne(a => a.Card)
                  .WithOne(c => c.Owner)
                  .HasForeignKey<Card>("OwnerId")
                  .IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Created).IsRequired();
            entity.Property(t => t.Amount).IsRequired();
            entity.Property(t => t.Balance).IsRequired();
            entity.Property(t => t.Description).IsRequired();
            entity.Property(t => t.Status).IsRequired();
            entity.Property(t => t.Type).IsRequired();

            entity.HasDiscriminator<string>("TransactionType")
                .HasValue<TellerMachineTransaction>("TellerMachine")
                .HasValue<TransferTransaction>("Transfer")
                .HasValue<ServicesPaymentTransaction>("ServicesPayment");
        });

        modelBuilder.Entity<TellerMachineTransaction>(entity =>
        {
            entity.Property(t => t.MachineName).IsRequired();
            entity.HasOne(t => t.Card)
                  .WithMany(c => c.TellerMachineTransactions)
                  .HasForeignKey("CardId")
                  .IsRequired();
        });
        
        modelBuilder.Entity<ServicesPaymentTransaction>(entity =>
        {
            entity.Property(t => t.ServiceName).IsRequired();
            entity.HasOne(t => t.Card)
                  .WithMany(c => c.ServicesPaymentTrasnactions)
                  .HasForeignKey("CardId")
                  .IsRequired();
        });

        modelBuilder.Entity<TransferTransaction>(entity =>
        {
            entity.HasOne(t => t.SenderCard)
                  .WithMany(c => c.SentTransferTransactions)
                  .HasForeignKey("SenderCardId")
                  .IsRequired();
            
            entity.HasOne(t => t.ReceiverCard)
                  .WithMany(c => c.ReceivedTransferTransactions)
                  .HasForeignKey("ReceiverCardId")
                  .IsRequired();
        });


    }
}
