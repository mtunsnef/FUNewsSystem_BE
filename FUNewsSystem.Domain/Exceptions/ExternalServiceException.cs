﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Domain.Exceptions
{
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException() : base("BadRequest")
        {

        }
        public ExternalServiceException(string message) : base(message)
        {

        }
        public ExternalServiceException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
