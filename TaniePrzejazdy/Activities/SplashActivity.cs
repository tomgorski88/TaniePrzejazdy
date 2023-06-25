using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Thread.Sleep(2000);
        }

        protected override void OnResume()
        {
            base.OnResume();
            var currentUser = AppDataHelper.GetCurrentUser();
            if (currentUser == null)
            {
                StartActivity(typeof(LoginActivity));
            }
            else
            {
                StartActivity(typeof(MainActivity));
            }
        }
    }
}