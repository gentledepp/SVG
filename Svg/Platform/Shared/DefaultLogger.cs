using System;
using System.Diagnostics;
using System.Net;
using Svg.Interfaces;

namespace Svg
{
    public class DefaultLogger : ILogger
    {
        public void Debug(string txt)
        {
            //System.Diagnostics.Debug.WriteLine(txt);
        }

        public void Info(string txt)
        {
            System.Diagnostics.Debug.WriteLine(txt);
        }

        public void Warn(string txt)
        {
            System.Diagnostics.Debug.WriteLine(txt);
        }

        public void Error(string txt)
        {
            System.Diagnostics.Debug.WriteLine(txt);
        }

        public void Fatal(string txt)
        {
            System.Diagnostics.Debug.WriteLine(txt);
        }
    }
}