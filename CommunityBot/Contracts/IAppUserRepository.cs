using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommunityBot.Contracts
{
    public interface IAppUserRepository
    {
        Task<bool> IsExisted(long id);
        
        public Task<AppUser?> Get(long id);

        public Task<AppUser?> GetByUsername(string username);

        public Task<IEnumerable<AppUser>> GetAll();

        public Task Add(AppUser appUser);

        public Task Update(AppUser appUser);
    }
}