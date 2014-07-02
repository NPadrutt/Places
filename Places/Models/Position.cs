using PropertyChanged;
using System;

namespace Places.Models
{
    [ImplementPropertyChanged]
    public class Position
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Accuracy { get; set; }

        public DateTime Timestamp { get; set; }
    }
}