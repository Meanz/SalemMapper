using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalemMapper
{
    /// <summary>
    /// A high resolution timer used for diagnostics
    /// </summary>
    public class HighResolutionTimer
    {

        /// <summary>
        /// The stopwatch for this timer
        /// </summary>
        private Stopwatch stopwatch;

        /// <summary>
        /// The name for this timer
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The frequency 
        /// </summary>
        private long nanosecPerTick;

        /// <summary>
        /// Get the number of nanoseconds elapsed
        /// </summary>
        public long ElapsedNanoseconds
        {
            get
            {
                return stopwatch.ElapsedTicks * nanosecPerTick;
            }
        }

        /// <summary>
        /// Get the number of milliseconds elapsed
        /// </summary>
        public double ElapsedMilliseconds
        {
            get
            {
                //Relatively safe =)
                return ((double)ElapsedNanoseconds / 1000000.0d);
            }
        }

        /// <summary>
        /// Get the number of elapsed seconds
        /// </summary>
        public double ElapsedSeconds
        {
            get
            {
                return stopwatch.ElapsedMilliseconds / 1000;
            }
        }

        /// <summary>
        /// Construct a new unnamed high resolution timer
        /// </summary>
        public HighResolutionTimer() : this("Unnamed") { }

        /// <summary>
        /// Construct a new named high resolution timer
        /// </summary>
        /// <param name="name"></param>
        public HighResolutionTimer(string name)
        {
            this.Name = name;
            stopwatch = new Stopwatch();
            nanosecPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
        }

        /// <summary>
        /// Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        public HighResolutionTimer Start()
        {
            stopwatch.Start();
            return this;
        }

        /// <summary>
        /// Stop measuring elapsed time for an interval.
        /// </summary>
        public HighResolutionTimer Stop()
        {
            stopwatch.Stop();
            return this;
        }

        /// <summary>
        /// Stops time interval measurement and resets the elapsed time to zero.
        /// </summary>
        public HighResolutionTimer Reset()
        {
            stopwatch.Reset();
            return this;
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring the elapsed time
        /// </summary>
        public HighResolutionTimer Restart()
        {
            stopwatch.Restart();
            return this;
        }
    }
}
