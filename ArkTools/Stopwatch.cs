using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkTools {

    public class Stopwatch {

        private readonly bool enabled;
        private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private readonly List<Measurement> measurements = new List<Measurement>();

        public Stopwatch(bool enabled) {
            this.enabled = enabled;
            if (enabled) {
                stopwatch.Start();
            } else {
                stopwatch.Reset();
            }
        }

        public void Stop(string description) {
            if (!enabled)
                return;
            measurements.Add(new Measurement(description, stopwatch.Elapsed));
            stopwatch.Restart();
        }

        public void Print() {
            if (!enabled) {
                return;
            }
            stopwatch.Reset();

            StringBuilder sb = new StringBuilder();

            foreach (Measurement measurement in measurements) {
                sb.Append(measurement.Description);
                sb.Append(" finished after ");
                sb.Append(measurement.Elapsed.TotalMilliseconds);
                sb.AppendLine(" ms");
            }

            if (measurements.Any()) {
                sb.Append("Total time ");
                sb.Append(measurements.Sum(measurement => measurement.Elapsed.TotalMilliseconds));
                sb.AppendLine(" ms");
            }

            Console.Out.Write(sb.ToString());
        }

        private class Measurement {
            public string Description { get; }
            public TimeSpan Elapsed { get; }

            public Measurement(string description, TimeSpan elapsed) {
                Description = description;
                Elapsed = elapsed;
            }
        }

    }

}
