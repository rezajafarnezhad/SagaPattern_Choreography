using CustomerService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace CustomerService.Context;

public class CustomerDbContext : DbContext
{
    private IDbContextTransaction _currentTransaction;
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>().HasData(

            new Customer { Id = Guid.NewGuid(), Name = "Jac", Credit = 2000 }
        );
    }


    public async Task<T> ExecuteTransactionalAsync<T>(Func<Task<T>> action, IsolationLevel isolationLevel = IsolationLevel.Serializable) where T : new()
    {
        var isCreateTran = await BeginTransactionAsync(isolationLevel);

        if (!isCreateTran)
            return await action();

        try
        {
            var res = await action();

            await _currentTransaction.CommitAsync();

            return res;
        }
        catch
        {
            await _currentTransaction.RollbackAsync();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task<T> ExecuteTransactionalWithFailureAsync<T>(Func<Task<T>> action, IsolationLevel isolationLevel = IsolationLevel.Serializable) where T : new()
    {

        var isCreateTran = await BeginTransactionAsync(isolationLevel);

        if (!isCreateTran)
            return await action();

        try
        {
            var res = await action();

            await _currentTransaction.CommitAsync();

            return res;
        }
        catch
        {
            await _currentTransaction.RollbackAsync();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task<bool> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Serializable, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return false;

        _currentTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return true;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}