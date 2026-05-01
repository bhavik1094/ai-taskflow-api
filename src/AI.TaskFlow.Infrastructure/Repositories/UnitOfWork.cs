using AI.TaskFlow.Application.Interfaces;
using AI.TaskFlow.Infrastructure.Data;

namespace AI.TaskFlow.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (_repositories.TryGetValue(typeof(T), out var repository))
        {
            return (IGenericRepository<T>)repository;
        }

        var createdRepository = new GenericRepository<T>(_dbContext);
        _repositories[typeof(T)] = createdRepository;
        return createdRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
