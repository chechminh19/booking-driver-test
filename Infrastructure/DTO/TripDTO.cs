using Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class TripDTO
    {
        public long Id { get; set; }
        public long? CustomerId { get; set; }
        public long? DriverId { get; set; }
        public TripStatus Status { get; set; }
        public double? SourceLatitude { get; set; }
        public double? SourceLongitude { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
    }
}
