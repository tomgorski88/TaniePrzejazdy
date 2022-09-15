using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.DrawerLayout.Widget;
using Firebase;
using Firebase.Database;
using Google.Places;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy
{
    [Activity(Label = "@string/app_name", Theme = "@style/tpTheme")]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        private FirebaseDatabase database;
        private AndroidX.AppCompat.Widget.Toolbar mainToolbar;
        private DrawerLayout drawerLayout;
        private GoogleMap mainMap;

        private TextView pickupLocationText;
        private TextView destinationText;

        private RelativeLayout layoutPickup;
        private RelativeLayout layoutDestination;

        private readonly string[] permissionGroupLocation = {Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation};
        private const int requestLocationId = 0;

        private LocationRequest mLocationRequest;
        private FusedLocationProviderClient locationClient;
        private Android.Locations.Location mLastLocation;
        private LocationCallbackHelper mLocationCallback;

        private static readonly int UPDATE_INTERVAL = 5; //5 sekund
        private static readonly int FASTEST_INTERVAL = 5;
        private static readonly int DISPLACEMENT = 3; //meters

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ConnectControl();

            var mapFragment = (SupportMapFragment) SupportFragmentManager.FindFragmentById(Resource.Id.map).JavaCast<SupportMapFragment>();
            mapFragment.GetMapAsync(this);

            CheckLocationPermissions();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();
            InitializePlaces();
        }

        void ConnectControl()
        {
            // DrawerLayout
            drawerLayout = (DrawerLayout)FindViewById(Resource.Id.drawerLayout);

            //Toolbar
            mainToolbar = (AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.mainToolbar);
            SetSupportActionBar(mainToolbar);
            SupportActionBar.Title = "";
            var actionBar = SupportActionBar;
            actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            actionBar.SetDisplayHomeAsUpEnabled(true);

            //TextView
            pickupLocationText = (TextView)FindViewById(Resource.Id.pickupText);
            destinationText = (TextView)FindViewById(Resource.Id.destinationText);

            // Layouts
            layoutPickup = (RelativeLayout)FindViewById(Resource.Id.layoutPickup);
            layoutDestination = (RelativeLayout)FindViewById(Resource.Id.layoutDestination);
            layoutPickup.Click += LayoutPickup_Click;
            layoutDestination.Click += LayoutDestination_Click;
        }

        private void LayoutPickup_Click(object sender, EventArgs e)
        {
            List<Place.Field> fields = new List<Place.Field>();
            fields.Add(Place.Field.Id);
            fields.Add(Place.Field.Name);
            fields.Add(Place.Field.LatLng);
            fields.Add(Place.Field.Address);

            var intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields).SetCountry("PL").Build(this);
            StartActivityForResult(intent, 1);
        }
        private void LayoutDestination_Click(object sender, EventArgs e)
        {
            List<Place.Field> fields = new List<Place.Field>();
            fields.Add(Place.Field.Id);
            fields.Add(Place.Field.Name);
            fields.Add(Place.Field.LatLng);
            fields.Add(Place.Field.Address);

            var intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields).SetCountry("PL").Build(this);
            StartActivityForResult(intent, 2);
            
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        void InitializeDatabase()
        {
            var app = FirebaseApp.InitializeApp(this);
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("tanieprzejazdy-bfab4")
                    .SetApiKey("AIzaSyAJ3bPs6A49I0g9AcRzx22J-xzGi-PVntM")
                    .SetDatabaseUrl("https://tanieprzejazdy-bfab4-default-rtdb.europe-west1.firebasedatabase.app")
                    .SetStorageBucket("tanieprzejazdy-bfab4.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(this, options);
            }
            database = FirebaseDatabase.GetInstance(app);

            var dbref = database.GetReference("UserSupport");
            dbref.SetValue("Ticket1");

            Toast.MakeText(this, "Completed", ToastLength.Short);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mainMap = googleMap;
        }

        bool CheckLocationPermissions()
        {
            bool permissionGranted;
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted &&
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                permissionGranted = false;
                RequestPermissions(permissionGroupLocation, requestLocationId);
            }
            else
            {
                permissionGranted = true;
            }
            return permissionGranted;
        }

        public void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
            locationClient = LocationServices.GetFusedLocationProviderClient(this);
            mLocationCallback = new LocationCallbackHelper();
            mLocationCallback.MyLocation += MLocationCallback_MyLocation;
        }

        void InitializePlaces()
        {
            string mapkey = Resources.GetString(Resource.String.mapkey);
            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(this, mapkey);
            }
        }

        private void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            var myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 12));
        }

        void StartLocationUpdates()
        {
            if (CheckLocationPermissions())
            {
                locationClient.RequestLocationUpdates(mLocationRequest, mLocationCallback, null);
            }
        }

        void StopLocationUpdates()
        {
            if(locationClient!=null && mLocationCallback != null)
            {
                locationClient.RemoveLocationUpdates(mLocationCallback);
            }
        }

        public async void GetMyLocation()
        {
            if (!CheckLocationPermissions())
            {
                return;
            }
            mLastLocation = await locationClient.GetLastLocationAsync();
            if (mLastLocation != null)
            {
                var myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15));
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (grantResults[0] == (int)Android.Content.PM.Permission.Granted)
            {
                Toast.MakeText(this, "Permission was granted", ToastLength.Short).Show();
            } else
            {
                Toast.MakeText(this, "Permission was denied", ToastLength.Short).Show();
            }            
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if(requestCode == 1)
            {
                if(resultCode == Android.App.Result.Ok)
                {
                    var place = Autocomplete.GetPlaceFromIntent(data);
                    pickupLocationText.Text = place.Name.ToString();
                    
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                }
            }
            if (requestCode == 2)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    var place = Autocomplete.GetPlaceFromIntent(data);
                    destinationText.Text = place.Name.ToString();
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                }
            }
        }
    }
}