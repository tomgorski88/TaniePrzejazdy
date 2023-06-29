using Android.Gms.Extensions;
using Firebase.Database;
using Java.Util;
using TaniePrzejazdy.DataModel;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy.EventListeners
{
    public class CreateRequestEventListener : Java.Lang.Object, IValueEventListener
    {
        private NewTripDetails newTrip;
        private FirebaseDatabase database;
        private DatabaseReference newTripRef;

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
    }
}