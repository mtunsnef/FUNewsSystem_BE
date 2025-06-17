using FUNewsSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.HttpContextService
{
    public interface IHttpContextService
    {
        Task<SystemAccount?> GetSystemAccount();
        Task<SystemAccount> GetSystemAccountAndThrow();
        string GetIpAddress();
    }
}
