using Android.App;
using Android.OS;

namespace TaniePrzejazdy.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/tpTheme", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);
        }
    }
}