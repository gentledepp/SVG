using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface ILogger
    {
        void Debug(string txt);
        void Info(string txt);
        void Warn(string txt);
        void Error(string txt);
        void Fatal(string txt);
    }
}
