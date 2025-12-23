using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace WpfApp1.classes_bd
{
    public partial class Model_R : DbContext
    {
        public Model_R()
            : base("name=Model_R")
        {
        }

        public virtual DbSet<Admin> Admin { get; set; }
        public virtual DbSet<Booking> Booking { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<Cook> Cook { get; set; }
        public virtual DbSet<Delivery> Delivery { get; set; }
        public virtual DbSet<Discount> Discount { get; set; }
        public virtual DbSet<Dish> Dish { get; set; }
        public virtual DbSet<Dish_prod> Dish_prod { get; set; }
        public virtual DbSet<Ord_dish> Ord_dish { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Table> Table { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<User_type> User_type { get; set; }
        public virtual DbSet<Waiter> Waiter { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>()
                .Property(e => e.Code)
                .IsFixedLength();

            modelBuilder.Entity<Booking>()
                .Property(e => e.Count)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Client>()
                .Property(e => e.Count)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Discount>()
                .HasMany(e => e.Ord_dish)
                .WithOptional(e => e.Discount)
                .HasForeignKey(e => e.Discounttype);

            modelBuilder.Entity<Dish>()
                .Property(e => e.Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Ord_dish>()
                .Property(e => e.Cost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Order>()
                .Property(e => e.Count)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Product>()
                .Property(e => e.Cost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Table>()
                .Property(e => e.Cost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Table>()
                .HasMany(e => e.Booking)
                .WithRequired(e => e.Table)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Phone)
                .IsFixedLength();

            modelBuilder.Entity<User_type>()
                .HasMany(e => e.User)
                .WithRequired(e => e.User_type)
                .HasForeignKey(e => e.Type)
                .WillCascadeOnDelete(false);
        }
    }
}
