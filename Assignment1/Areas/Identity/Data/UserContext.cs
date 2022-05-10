using Assignment1.Areas.Identity.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.Data;

public class UserContext : IdentityDbContext<Assignment1User>
{
    public UserContext(DbContextOptions<UserContext> options)
        : base(options)
    {
    }
    public DbSet<Store> Store { get; set; }

    public DbSet<Book> Book { get; set; }

    public DbSet<CartItem> CartItem { get; set; }

    public DbSet<Order> Order { get; set; }

    public DbSet<OrderDetail> OrderDetail { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);            

        builder.Entity<CartItem>(builder =>
        {
            builder.HasKey(c => new { c.BookIsbn, c.UserID, });

            builder.HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserID);

            builder.HasOne<Book>(od => od.Book)
                .WithMany(b => b.CartItems)
                .HasForeignKey(od => od.BookIsbn)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<OrderDetail>(builder =>
        {
            builder.HasKey(od => new { od.OrderId, od.BookIsbn });

            builder.HasOne(od => od.Order)
                .WithMany(or => or.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(od => od.Book)
                .WithMany(b => b.OrderDetails)
                .HasForeignKey(od => od.BookIsbn)
                .OnDelete(DeleteBehavior.NoAction);
        });


        //By conventions, Index of UserID is created by default. Hence it's not necessary to include the following.
        //builder.Entity<CartItem>()
        //    .HasIndex(c => c.UserID);

        //builder.Entity<Assignment1User>()
        //    .HasOne<Store>(au => au.Store)
        //    .WithOne(st => st.User)
        //    .HasForeignKey<Store>(st => st.UserId);
        //builder.Entity<Book>()
        //    .HasOne<Store>(b => b.Store)
        //    .WithMany(st => st.Books)
        //    .HasForeignKey(st => st.StoreId);
        //builder.Entity<Order>()
        //    .HasOne<Assignment1User>(o => o.User)
        //    .WithMany(ap => ap.Orders)
        //    .HasForeignKey(o => o.UserId);
    }
}
