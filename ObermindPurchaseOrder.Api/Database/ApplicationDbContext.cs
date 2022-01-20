using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ObermindPurchaseOrder.Api.Database.Models;

namespace ObermindPurchaseOrder.Api.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options
           
        ) : base(options)
        {
           
        }
    }
}