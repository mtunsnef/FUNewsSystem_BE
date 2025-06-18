using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Service.Services.ConfigService
{
    public interface IConfigService
    {
        int GetInt(string key);
        string GetString(string key);
    }
}
