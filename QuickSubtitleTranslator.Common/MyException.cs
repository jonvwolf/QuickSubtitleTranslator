using System;
using System.Collections.Generic;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class MyException : Exception
    {
        public MyException(string message) : base(message)
        {
        }
    }
}
