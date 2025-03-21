using Microsoft.EntityFrameworkCore;

namespace Zuhid.Base;

public abstract class BaseRepository<TContext, TModel, TEntity>(TContext context)
  where TContext : IDbContext
  where TModel : BaseModel
  where TEntity : BaseEntity
{
    protected readonly TContext context = context;

    public abstract IQueryable<TModel> Query { get; }

    public async Task<List<TModel>> Get(Guid id) => await Query.Where(n => n.Id.Equals(id)).ToListAsync();

    public async Task<SaveRespose> Add(TEntity entity)
    {
        try
        {
            entity.Updated = DateTime.UtcNow;
            context.Set<TEntity>().Add(entity);
            _ = await context.SaveChangesAsync();
            return new SaveRespose { Updated = entity.Updated };
        }
        catch (Exception ex)
        {
            ex.Data.Add("entity", entity);
            throw;
        }
    }

    public async Task<SaveRespose> Update(TEntity entity)
    {
        try
        {
            // the updated value is set to make sure no one else modifed the record since the last read
            context.Entry(entity).Property(p => p.Updated).OriginalValue = context.Entry(entity).Property(p => p.Updated).CurrentValue;
            entity.Updated = DateTime.UtcNow; // Update the record with to have the current utc value
            context.Set<TEntity>().Update(entity);
            _ = await context.SaveChangesAsync();
            return new SaveRespose { Updated = entity.Updated };
        }
        catch (Exception ex)
        {
            ex.Data.Add("entity", entity);
            throw;
        }
    }

    public async Task<int> Delete(Guid id)
    {
        return await context.Set<TEntity>().Where(e => e.Id.Equals(id)).ExecuteDeleteAsync();
    }
}
