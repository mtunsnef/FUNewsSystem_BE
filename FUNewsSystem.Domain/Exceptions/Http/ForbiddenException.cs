using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Domain.Exceptions.Http
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base("Forbidden")
        {

        }
        public ForbiddenException(string message) : base(message)
        {

        }
        public ForbiddenException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
