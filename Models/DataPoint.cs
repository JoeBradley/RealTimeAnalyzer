using Newtonsoft.Json;
using System;

namespace cc.RealTimeAnalyzer.Models
{
    public class TimestampPoint
    {
        public DateTime Timestamp { get; set; }
    }

    public class GeoPoint: TimestampPoint
    {
        // Geospatial
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    
    public class DataPoint : GeoPoint
    {
        private static readonly string[] Names = { "Chris", "John", "Annette", "Leigh", "Scott", "Jarrod", "Marnie" };
        private static readonly string[] Types = { "Click", "Move", "DoubleClick", "Drag", "Drop", "Hover" };
        private static readonly string[] Actions = { "Buy", "Sell", "Reserve", "Decline", "Reject" };

        private static Random rnd = new Random();

        public string UserName { get; set; }
        public string Type { get; set; }
        public string Action { get; set; }
        
        // Screen position
        public double PositionX { get; set; }
        public double PositionY { get; set; }

        // Stock values
        public double High { get; set; }
        public double Low { get; set; }

        public DataPoint()
        {
            this.Timestamp = DateTime.UtcNow;
            this.UserName = Names[rnd.Next(Names.Length)];
            this.Type = Types[rnd.Next(Types.Length)];
            this.Action = Actions[rnd.Next(Actions.Length)];
            this.Latitude = -90 + rnd.NextDouble() * 180;
            this.Longitude = -180 + rnd.NextDouble() * 360;
            this.PositionX = rnd.NextDouble() * 1200;
            this.PositionY = rnd.NextDouble() * 800;
            this.High = 583 + rnd.NextDouble() * 20;
            this.Low = 583 + rnd.NextDouble() * 20;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
