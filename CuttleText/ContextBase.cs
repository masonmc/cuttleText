using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuttleText
{
    public abstract class ContextBase
    {

        // override these to perform your own logging.
        public enum eLogCatetory
        {
            InternalError,
            FatalError,
            Error,
            Warning,
            Info
        }

        protected string _language;

        public virtual void LogMessage(eLogCatetory cat, string file, int line, string message)
        {
        }
    }
}
