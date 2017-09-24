using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cc.RealTimeAnalyzer.Models
{
    public class AggregateData
    {
        public DateTime Timestamp { get; set; }
        public int Count { get; set; }
        public AggregateDataPoint High { get; set; }
        public AggregateDataPoint Low { get; set; }
        public List<DataPoint> Data { get; set; }

        [JsonConstructor]
        public AggregateData() { }

        public AggregateData(DateTime timestamp, List<DataPoint> data)
        {
            Timestamp = timestamp;
            Data = data;
            Count = data.Count;
            High = new AggregateDataPoint(data.Select(x => x.High));
            Low = new AggregateDataPoint(data.Select(x => x.Low));
        }

        public AggregateData(List<DataPoint> data, double lastAvg, double lastMin, double lastMax)
        {
            Data = data;
            Count = data.Count;
            High = new AggregateDataPoint(data.Select(x => x.High), lastAvg, lastMin, lastMax);
        }
    }

    public class AggregateDataPoint
    {
        public double Avg { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        public double DAvg { get; set; }
        public double DMin { get; set; }
        public double DMax { get; set; }

        [JsonConstructor]
        public AggregateDataPoint() { }

        public AggregateDataPoint(IEnumerable<double> values) {
            Avg = values.Average();
            Min = values.Min();
            Max = values.Max();
        }

        public AggregateDataPoint(IEnumerable<double> values, double lastAvg, double lastMin, double lastMax) : this(values)
        {
            DAvg = Avg - lastAvg;
            DMin = Min - lastMin;
            DMax = Max - lastMax;
        }
    }
}
