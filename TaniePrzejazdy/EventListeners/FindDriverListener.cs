using Android.Gms.Maps.Model;
using Com.Google.Maps.Android;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TaniePrzejazdy.DataModel;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy.EventListeners
{
    public class FindDriverListener : Java.Lang.Object, IValueEventListener
    {
        //Events
        public class DriverFoundEventArgs: EventArgs
        {
            public List<AvailableDriver> Drivers { get; set; }
        }

        public event EventHandler<DriverFoundEventArgs> DriversFound;
        public event EventHandler DriverNotFound;


        LatLng mPickupLocation;
        string mRideId;

        List<AvailableDriver> availableDrivers;
        public FindDriverListener(LatLng pickupLocation, string rideId)
        {
            mPickupLocation = pickupLocation;
            mRideId = rideId;
            availableDrivers = new List<AvailableDriver>();
        }
        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Value != null)
            {
                var child = snapshot.Children.ToEnumerable<DataSnapshot>();
                availableDrivers.Clear();
                foreach (var data in child)
                {
                    if (data.Child("ride_id").Value != null)
                    {
                        if (data.Child("ride_id").Value.ToString() == "waiting")
                        {
                            var latitude = double.Parse(data.Child("location").Child("latitude").Value.ToString());
                            var longitude = double.Parse(data.Child("location").Child("longitude").Value.ToString());

                            var driverLocation = new LatLng(latitude, longitude);
                            var driver = new AvailableDriver();
                            driver.DistanceFromPickup = SphericalUtil.ComputeDistanceBetween(mPickupLocation, driverLocation);
                            driver.ID = data.Key;

                            availableDrivers.Add(driver);
                        }
                    }
                }

                if (availableDrivers.Count > 0)
                {
                    availableDrivers = availableDrivers.OrderBy(d => d.DistanceFromPickup).ToList();
                    DriversFound?.Invoke(this, new DriverFoundEventArgs { Drivers = availableDrivers });
                }
                else
                {
                    DriverNotFound?.Invoke(this, new EventArgs());
                }
            }
            else
            {
                DriverNotFound?.Invoke(this, new EventArgs());
            }
        }
        
        public void Create()
        {
            var database = AppDataHelper.GetDatabase();
            var findDriverRef = database.GetReference("driversAvailable");
            findDriverRef.AddListenerForSingleValueEvent(this);
        }
    }
}