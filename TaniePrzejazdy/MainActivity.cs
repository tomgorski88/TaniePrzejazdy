using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.DrawerLayout.Widget;
using Firebase;
using Firebase.Database;

namespace TaniePrzejazdy
{
    [Activity(Label = "@string/app_name", Theme = "@style/tpTheme")]
    public class MainActivity : AppCompatActivity
    {
        private FirebaseDatabase database;
        private AndroidX.AppCompat.Widget.Toolbar mainToolbar;
        private DrawerLayout drawerLayout;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ConnectControl();
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
    }
}