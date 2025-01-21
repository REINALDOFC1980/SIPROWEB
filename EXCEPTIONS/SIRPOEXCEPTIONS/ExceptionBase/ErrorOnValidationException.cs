﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIRPOEXCEPTIONS.ExceptionBase
{
    public class ErrorOnValidationException : SiproException
    {

        public IList<string>? ErrosMessages { get; set; }

        public ErrorOnValidationException(IList<string> errosMessages) 
        { 
         ErrosMessages = errosMessages;
        
        }
    }
}
