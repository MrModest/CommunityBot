using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface IRepositoryBase<TEntity> where TEntity : EntityBase
    {
        Task<TEntity?> GetById(long chatId);
        Task Add(TEntity entity);
        Task Update(TEntity entity);
        Task Remove(long id);
        Task AddOrUpdate(TEntity entity);
    }
}