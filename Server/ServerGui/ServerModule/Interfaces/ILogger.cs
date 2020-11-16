using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerModule.Logger
{
    public interface ILogger
    {
        void writeLineLogMessage(Message msg);
        void writeLineLogMessageAsync(Message msg);
    }
}
