using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Domain.Exceptions.Http
{
    public class BadRequestException : Exception
    {
        public BadRequestException() : base("BadRequest")
        {

        }
        public BadRequestException(string message) : base(message)
        {

        }
        public BadRequestException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
