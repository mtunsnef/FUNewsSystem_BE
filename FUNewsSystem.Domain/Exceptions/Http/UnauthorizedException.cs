using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Domain.Exceptions.Http
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("Unauthorized")
        {

        }
        public UnauthorizedException(string message) : base(message)
        {

        }
        public UnauthorizedException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
