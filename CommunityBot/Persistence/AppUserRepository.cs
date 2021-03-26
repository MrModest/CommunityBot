using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using CommunityBot.Contracts;
using Microsoft.Extensions.Options;

namespace CommunityBot.Persistence
{
    public class AppUserRepository : RepositoryBase<AppUser>, IAppUserRepository
    {
        private ConcurrentBag<long>? _existedUserIds;

        public AppUserRepository(IOptions<SQLiteConfigurationOptions> options)
            : base(options.Value.DbFilePath)
        {
        }

        public async Task<bool> IsExisted(long id)
        {
            if (_existedUserIds == null)
            {
                var userIds = (await GetAllInternal()).Select(u => u.Id).ToList();

                _existedUserIds ??= new ConcurrentBag<long>(userIds);
            }

            if (_existedUserIds.Contains(id))
            {
                return true;
            }

            var user = await Get(id);

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
            _existedUserIds?.Add(appUser.Id);
            
            return Insert(appUser);
        }

        public new Task Update(AppUser appUser)
        {
            return base.Update(appUser);
        }
    }
}