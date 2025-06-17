using FUNewsSystem.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo
{
    public class SystemAccountRepository : RepositoryBase<SystemAccount>, ISystemAccountRepository
    {
        public SystemAccountRepository(FunewsSystemApiDbContext context) : base(context)
        {
        }

        public async Task<SystemAccount?> GetAccountByEmail(string email)
        {
            var appUser = await _dbSet.FirstOrDefaultAsync(u => u.AccountEmail == email);

            return appUser;
        }
    }
}
