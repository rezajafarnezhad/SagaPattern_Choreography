using Microsoft.EntityFrameworkCore;
using Order = OrderService.Dmain.Order;

namespace OrderService.Context;

public class OrderDbContext : DbContext
{

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
}