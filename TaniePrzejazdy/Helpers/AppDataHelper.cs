using Android.App;
using Android.Content;
using Android.Widget;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace TaniePrzejazdy.Helpers
{
    public static class AppDataHelper
    {
        private static readonly ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        public static FirebaseDatabase GetDatabase()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("tanieprzejazdy-bfab4")
                    .SetApiKey("AIzaSyAJ3bPs6A49I0g9AcRzx22J-xzGi-PVntM")
                    .SetDatabaseUrl("https://tanieprzejazdy-bfab4-default-rtdb.europe-west1.firebasedatabase.app")
                    .SetStorageBucket("tanieprzejazdy-bfab4.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options);
            }
            var database = FirebaseDatabase.GetInstance(app);

            return database;
        }

        public static FirebaseAuth GetFirebaseAuth()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("tanieprzejazdy-bfab4")
                    .SetApiKey("AIzaSyAJ3bPs6A49I0g9AcRzx22J-xzGi-PVntM")
                    .SetDatabaseUrl("https://tanieprzejazdy-bfab4-default-rtdb.europe-west1.firebasedatabase.app")
                    .SetStorageBucket("tanieprzejazdy-bfab4.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options);
            }
            var mAuth = FirebaseAuth.Instance;
            return mAuth;
        }

        public static FirebaseUser GetCurrentUser()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("tanieprzejazdy-bfab4")
                    .SetApiKey("AIzaSyAJ3bPs6A49I0g9AcRzx22J-xzGi-PVntM")
                    .SetDatabaseUrl("https://tanieprzejazdy-bfab4-default-rtdb.europe-west1.firebasedatabase.app")
                    .SetStorageBucket("tanieprzejazdy-bfab4.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(Application.Context, options);
            }

            var mAuth = FirebaseAuth.Instance;
            var mUser = mAuth.CurrentUser;
            return mUser;
        }

        public static string GetFullName()
        {
            return preferences.GetString("fullname", string.Empty);
        }
        public static string GetEmail()
        {
            return preferences.GetString("email", string.Empty);
        }
        public static string GetPhone()
        {
            return preferences.GetString("phone", string.Empty);
        }
    }
}