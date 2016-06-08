using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emp_ferias.lib.Classes
{
    public enum MessageType
    {
        Success,
        Info,
        Warning,
        Error
    }

    public class ExecutionResult
    {
        public MessageType MessageType { get; set; }
        public string Message { get; set; }
    }
}
