using System;
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

            var mapFragment = (SupportMapFragment) SupportFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            CheckLocationPermissions();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();
        }

        void ConnectControl()
        {
            drawerLayout = (DrawerLayout)FindViewById(Resource.Id.drawerLayout);
            mainToolbar = (AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.mainToolbar);
            SetSupportActionBar(mainToolbar);
            SupportActionBar.Title = "";
            var actionBar = SupportActionBar;
            actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            actionBar.SetDisplayHomeAsUpEnabled(true);
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
                mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17));
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
    }
}