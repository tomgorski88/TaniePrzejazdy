using System;

namespace TaniePrzejazdy.DataModel
{
    public class NewTripDetails
    {
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public string PickupAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string DistanceString { get; set; }
        public double DistanceValue { get; set; }
        public string DurationString { get; set; }
        public double DurationValue { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TimeStamp { get; set; }
        public string RideId { get; set; }
        public string DriverId { get; set; }
        public string DriverPhone { get; set; }
        public double EstimateFare { get; set; }
    }
}