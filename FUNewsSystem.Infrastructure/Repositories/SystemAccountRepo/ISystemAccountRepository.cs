using FUNewsSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Infrastructure.Repositories.SystemAccountRepo
{
    public interface ISystemAccountRepository : IRepositoryBase<SystemAccount>
    {
        Task<SystemAccount?> GetAccountByEmail(string email);

    }
}
