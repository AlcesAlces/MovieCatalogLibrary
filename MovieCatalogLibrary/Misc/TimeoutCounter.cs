using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.Misc
{
    public class TimeoutCounter : Stopwatch
    {
        public bool timeout = false;
        public bool timerExit = false;

        /// <summary>
        /// Timeout is a timespan in ms
        /// </summary>
        /// <param name="timeout"></param>
        public bool? start(int timespan)
        {
            Start();
            while (Elapsed.TotalMilliseconds < timespan) ;
            Stop();
            timeout = true;
            return true;
        }
    }
}
