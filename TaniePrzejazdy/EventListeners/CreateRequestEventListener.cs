using Android.Gms.Extensions;
using Firebase.Database;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TaniePrzejazdy.DataModel;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy.EventListeners
{
    public class CreateRequestEventListener : Java.Lang.Object, IValueEventListener
    {
        private NewTripDetails newTrip;
        private FirebaseDatabase database;
        private DatabaseReference newTripRef;
        private DatabaseReference notifyDriverRef;

        public List<AvailableDriver> mAvailableDrivers;
        private AvailableDriver selectedDriver;

        private System.Timers.Timer RequestTimer = new System.Timers.Timer();
        private int TimerCounter = 0;

        private bool hasDriverAccepted;

        public event EventHandler NoDriverAcceptedRequest;


        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {

        }

        public CreateRequestEventListener(NewTripDetails mNewTrip)
        {
            newTrip = mNewTrip;
            database = AppDataHelper.GetDatabase();

            RequestTimer.Interval = 1000;
            RequestTimer.Elapsed += RequestTimer_Elapsed;
        }

        public void CreateRequest()
        {
            newTripRef = database.GetReference("rideRequest").Push();

            var location = new HashMap();
            location.Put("latitude", newTrip.PickupLat);
            location.Put("longitude", newTrip.PickupLng);

            var destination = new HashMap();
            destination.Put("latitude", newTrip.DestinationLat);
            destination.Put("longitude", newTrip.DestinationLng);

            var myTrip = new HashMap();

            newTrip.RideId = newTripRef.Key;
            myTrip.Put("location", location);
            myTrip.Put("destination", destination);
            myTrip.Put("destination_address", newTrip.DestinationAddress);
            myTrip.Put("pickup_address", newTrip.PickupAddress);
            myTrip.Put("rider_id", AppDataHelper.GetCurrentUser().Uid);
            myTrip.Put("payment_method", newTrip.PaymentMethod);
            myTrip.Put("created_at", newTrip.TimeStamp.ToString());
            myTrip.Put("driver_id", "waiting");
            myTrip.Put("rider_name", AppDataHelper.GetFullName());
            myTrip.Put("rider_phone", AppDataHelper.GetPhone());

            newTripRef.AddValueEventListener(this);
            newTripRef.SetValue(myTrip);
        }

        public void CancelRequest()
        {
            if (selectedDriver != null)
            {
                var cancelDriverRef = database.GetReference("driversAvailable/" + selectedDriver.ID + "/ride_id");
                cancelDriverRef.SetValue("cancelled");
            }
            newTripRef.RemoveEventListener(this);
            newTripRef.RemoveValue();
        }

        public void CancelRequestOnTimeout()
        {
            newTripRef.RemoveEventListener(this);
            newTripRef.RemoveValue();
        }

        public void NotifyDriver(List<AvailableDriver> availableDrivers)
        {
            mAvailableDrivers = availableDrivers;

            if (availableDrivers.Count > 0 && availableDrivers != null)
            {
                selectedDriver = mAvailableDrivers.First();
                notifyDriverRef = database.GetReference("driversAvailable/" + selectedDriver.ID + "/ride_id");
                notifyDriverRef.SetValue(newTrip.RideId);

                if (mAvailableDrivers.Count > 1)
                {
                    mAvailableDrivers.RemoveAt(0);
                }
                else if (mAvailableDrivers.Count == 1)
                {
                    mAvailableDrivers = null;
                }

                RequestTimer.Enabled = true;
            }
            else
            {

                // no driver accepted
                RequestTimer.Enabled = false;
                NoDriverAcceptedRequest?.Invoke(this, new EventArgs());
            }
        }
        private void RequestTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimerCounter++;

            if (TimerCounter == 10)
            {
                if (!hasDriverAccepted)
                {
                    var cancelDriverRef = database.GetReference("driversAvailable/" + selectedDriver.ID + "/ride_id");
                    cancelDriverRef.SetValue("timeout");
                    TimerCounter = 0;

                    if (mAvailableDrivers != null)
                    {
                        NotifyDriver(mAvailableDrivers);
                    }
                    else
                    {
                        RequestTimer.Enabled = false;
                        NoDriverAcceptedRequest?.Invoke(this, new EventArgs());
                    }
                }

            }
        }
    }
}