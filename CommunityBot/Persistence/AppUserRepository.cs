using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using CommunityBot.Contracts;

namespace CommunityBot.Persistence
{
    public class AppUserRepository : RepositoryBase<AppUser>, IAppUserRepository
    {
        private readonly IMemoryCacheWrapper _memoryCacheWrapper;

        public AppUserRepository(
            SQLiteConnection connection,
            IMemoryCacheWrapperFactory memoryCacheWrapperFactory)
            : base(connection)
        {
            _memoryCacheWrapper = memoryCacheWrapperFactory.CreateWrapper("User_");
        }

        public async Task<bool> IsExisted(long id)
        {
            var user = await GetCached(id);

            return user != null;
        }

        public Task<AppUser?> Get(long id)
        {
            return ById(id);
        }

        public Task<AppUser?> GetByUsername(string username)
        {
            return ByField(nameof(AppUser.Username), username);
        }

        public Task<IEnumerable<AppUser>> GetAll()
        {
            return GetAllInternal();
        }

        public Task Add(AppUser appUser)
        {
            return Insert(appUser);
        }

        public new Task Update(AppUser appUser)
        {
            return base.Update(appUser);
        }

        private async Task<AppUser?> GetCached(long id)
        {
            if (_memoryCacheWrapper.TryGetValue(id.ToString(), out AppUser cachedUser))
            {
                return cachedUser;
            }
            
            var user = await Get(id);

            if (user != null)
            {
                return _memoryCacheWrapper.Set(id.ToString(), user);
            }

            return null;
        }
    }
}
