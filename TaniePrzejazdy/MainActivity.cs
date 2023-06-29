using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.DrawerLayout.Widget;
using Firebase;
using Firebase.Database;
using Google.Android.Material.BottomSheet;
using Google.Places;
using TaniePrzejazdy.DataModel;
using TaniePrzejazdy.EventListeners;
using TaniePrzejazdy.Fragments;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy
{
    [Activity(Label = "@string/app_name", Theme = "@style/tpTheme")]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        UserProfileEventListener profileEventListener = new UserProfileEventListener();
        CreateRequestEventListener requestListener;

        private FirebaseDatabase database;
        private AndroidX.AppCompat.Widget.Toolbar mainToolbar;
        private DrawerLayout drawerLayout;
        private GoogleMap mainMap;

        private TextView pickupLocationText;
        private TextView destinationText;

        private RadioButton pickupRadio;
        private RadioButton destinationRadio;
        private Button favouritePlacesButton;
        private Button locationSetButton;
        private Button requestDriverButton;

        private ImageView centerMarker;

        private RelativeLayout layoutPickup;
        private RelativeLayout layoutDestination;

        private BottomSheetBehavior tripDetailsBottomSheetBehavior;

        private readonly string[] permissionGroupLocation = {Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation};
        private const int requestLocationId = 0;

        private LocationRequest mLocationRequest;
        private FusedLocationProviderClient locationClient;
        private Android.Locations.Location mLastLocation;
        private LocationCallbackHelper mLocationCallback;

        private static readonly int UPDATE_INTERVAL = 5000; //5 sekund
        private static readonly int FASTEST_INTERVAL = 5;
        private static readonly int DISPLACEMENT = 3; //meters

        private MapFunctionHelper mapFunctionHelper;

        //Trip details
        private LatLng pickupLocationLatLng;
        private LatLng destinationLocationLatLng;
        private string pickupAddress;
        private string destinationAddress;

        // Flags
        private int addressRequest = 1;
        private bool takeAddressFromSearch = false;

        //Fragments
        private RequestDriver requestDriverFragment;

        //DataModel
        private NewTripDetails newTripDetails;

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

            profileEventListener.Create();
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
            //Buttons
            pickupRadio = (RadioButton)FindViewById(Resource.Id.pickupRadio);
            destinationRadio = (RadioButton)FindViewById(Resource.Id.destinationRadio);
            favouritePlacesButton = (Button)FindViewById(Resource.Id.favouritePlacesButton);
            locationSetButton = (Button)FindViewById(Resource.Id.locationSetButton);
            requestDriverButton = (Button)FindViewById(Resource.Id.requestDriverButton);
            pickupRadio.Click += PickupRadio_Click;
            destinationRadio.Click += DestinationRadio_Click;
            favouritePlacesButton.Click += FavouritePlacesButton_Click;
            locationSetButton.Click += LocationSetButton_Click;
            requestDriverButton.Click += RequestDriverButton_Click;

            // Layouts
            layoutPickup = (RelativeLayout)FindViewById(Resource.Id.layoutPickup);
            layoutDestination = (RelativeLayout)FindViewById(Resource.Id.layoutDestination);
            layoutPickup.Click += LayoutPickup_Click;
            layoutDestination.Click += LayoutDestination_Click;

            centerMarker = (ImageView)FindViewById(Resource.Id.centerMarker);

            FrameLayout tripDetailsView = (FrameLayout)FindViewById(Resource.Id.tripdetails_bottomsheet);
            tripDetailsBottomSheetBehavior = BottomSheetBehavior.From(tripDetailsView);
        }


        #region CLICK EVENT HANDLERS
        private void RequestDriverButton_Click(object sender, EventArgs e)
        {
            requestDriverFragment = new RequestDriver(mapFunctionHelper.EstimateFares());
            requestDriverFragment.Cancelable = false;

            var trans = SupportFragmentManager.BeginTransaction();
            requestDriverFragment.Show(trans, "Request");
            requestDriverFragment.CancelRequest += RequestDriverFragment_CancelRequest;

            newTripDetails = new NewTripDetails
            {
                DestinationAddress = destinationAddress,
                PickupAddress = pickupAddress,
                DestinationLat = destinationLocationLatLng.Latitude,
                DestinationLng = destinationLocationLatLng.Longitude,
                DistanceString = mapFunctionHelper.distanceString,
                DistanceValue = mapFunctionHelper.distance,
                DurationString = mapFunctionHelper.durationString,
                DurationValue = mapFunctionHelper.duration,
                EstimateFare = mapFunctionHelper.EstimateFares(),
                PaymentMethod = "cash",
                PickupLat = pickupLocationLatLng.Latitude,
                PickupLng = pickupLocationLatLng.Longitude,
                TimeStamp = DateTime.Now
            };

            requestListener = new CreateRequestEventListener(newTripDetails);
            requestListener.CreateRequest();
        }

        private void RequestDriverFragment_CancelRequest(object sender, EventArgs e)
        {
            // User cancels request before driver accepts it
            if (requestDriverFragment !=null && requestListener != null)
            {
                requestListener.CancelRequest();
                requestListener = null;
                requestDriverFragment.Dismiss();
                requestDriverFragment = null;
            }
        }

        private async void LocationSetButton_Click(object sender, EventArgs e)
        {
            locationSetButton.Text = "Please wait...";
            locationSetButton.Enabled = false;

            var json = await mapFunctionHelper.GetDirectionJson(pickupLocationLatLng, destinationLocationLatLng);
            if (!string.IsNullOrEmpty(json))
            {
                var textFare = (TextView)FindViewById(Resource.Id.tripEstimateFareText);
                var textTime = (TextView)FindViewById(Resource.Id.newTripTimeText);

                mapFunctionHelper.DrawTripOnMap(json);

                textFare.Text = mapFunctionHelper.EstimateFares().ToString() + " PLN";
                textTime.Text = mapFunctionHelper.durationString;

                tripDetailsBottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
                TripDrawnOnMap();
            }
            locationSetButton.Text = "Done";
            locationSetButton.Enabled = true;
        }

        private void FavouritePlacesButton_Click(object sender, EventArgs e)
        {
            
        }

        private void PickupRadio_Click(object sender, EventArgs e)
        {
            addressRequest = 1;
            pickupRadio.Checked = true;
            destinationRadio.Checked = false;
            takeAddressFromSearch = false;
            centerMarker.SetColorFilter(Color.DarkGreen);
        }

        private void DestinationRadio_Click(object sender, EventArgs e)
        {
            addressRequest = 2;
            pickupRadio.Checked = false;
            destinationRadio.Checked = true;
            takeAddressFromSearch = false;
            centerMarker.SetColorFilter(Color.Red);
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
        #endregion        

        #region MAP AND LOCATION SERVICES
        public void OnMapReady(GoogleMap googleMap)
        {
            mainMap = googleMap;
            var mapkey = Resources.GetString(Resource.String.mapkey);
            mapFunctionHelper = new MapFunctionHelper(mapkey, googleMap);

            mainMap.CameraIdle += MainMap_CameraIdle;

        }

        private async void MainMap_CameraIdle(object sender, EventArgs e)
        {
            if (!takeAddressFromSearch)
            {
                if (addressRequest == 1)
                {
                    pickupLocationLatLng = mainMap.CameraPosition.Target;
                    pickupAddress = await mapFunctionHelper.FindCoordinateAddress(pickupLocationLatLng);
                    pickupLocationText.Text = pickupAddress;
                }
                else if (addressRequest == 2)
                {
                    destinationLocationLatLng = mainMap.CameraPosition.Target;
                    destinationAddress = await mapFunctionHelper.FindCoordinateAddress(destinationLocationLatLng);
                    destinationText.Text = destinationAddress;
                    TripLocationsSet();
                }
            }
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
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15));
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
        #endregion

        #region TRIP CONFIGURATIONS
        private void TripLocationsSet()
        {
            favouritePlacesButton.Visibility = ViewStates.Invisible;
            locationSetButton.Visibility = ViewStates.Visible;
        }

        private void TripDrawnOnMap()
        {
            layoutDestination.Clickable = false;
            layoutPickup.Clickable = false;
            pickupRadio.Enabled = false;
            destinationRadio.Enabled = false;
            takeAddressFromSearch = true;
            centerMarker.Visibility = ViewStates.Invisible;
        }
        #endregion

        #region OVERRIDE METHODS
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if(grantResults.Length < 1)
            {
                return;
            }
            if (grantResults[0] == (int)Android.Content.PM.Permission.Granted)
            {
                StartLocationUpdates();
            }          
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if(requestCode == 1)
            {
                if(resultCode == Android.App.Result.Ok)
                {
                    takeAddressFromSearch = true;
                    pickupRadio.Checked = false;
                    destinationRadio.Checked = false;

                    var place = Autocomplete.GetPlaceFromIntent(data);
                    pickupLocationText.Text = place.Name.ToString();
                    pickupLocationLatLng = place.LatLng;
                    pickupAddress = place.Name.ToString(); ;

                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                    centerMarker.SetColorFilter(Color.DarkGreen);
                }
            }
            if (requestCode == 2)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    takeAddressFromSearch = true;
                    pickupRadio.Checked = false;
                    destinationRadio.Checked = false;

                    var place = Autocomplete.GetPlaceFromIntent(data);
                    destinationText.Text = place.Name.ToString();
                    destinationLocationLatLng = place.LatLng;
                    destinationAddress = place.Name.ToString();
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                    centerMarker.SetColorFilter(Color.Red);
                    TripLocationsSet();
                }
            }
        }
        #endregion

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
    }
}