using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.CoordinatorLayout.Widget;
using Firebase;
using Firebase.Auth;
using Google.Android.Material.Snackbar;
using Google.Android.Material.TextField;
using System;
using TaniePrzejazdy.EventListeners;

namespace TaniePrzejazdy.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/tpTheme", MainLauncher = false)]
    public class LoginActivity : AppCompatActivity
    {
        private TextInputLayout emailText;
        private TextInputLayout passwordText;
        private Button loginButton;
        private CoordinatorLayout rootView;
        private TextView clickToRegisterText;
        private FirebaseAuth mAuth;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);

            emailText = (TextInputLayout)FindViewById(Resource.Id.emailText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordText);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            loginButton = (Button)FindViewById(Resource.Id.loginButton);
            clickToRegisterText = (TextView)FindViewById(Resource.Id.clickToRegisterText);
            clickToRegisterText.Click += ClickToRegisterText_Click;
            loginButton.Click += LoginButton_Click;

            InitializeFirebase();
        }

        private void ClickToRegisterText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
            Finish();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            var email = emailText.EditText.Text;
            var password = passwordText.EditText.Text;

            if (!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please provide a valid email", Snackbar.LengthShort).Show();
                return;
            }
            if (password.Length < 8)
            {
                Snackbar.Make(rootView, "Please provide a valid password", Snackbar.LengthShort).Show();
                return;
            }

            var taskCompletionListener = new TaskCompletionListener();
            taskCompletionListener.Success += TaskCompletionListener_Success;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;

            mAuth.SignInWithEmailAndPassword(email, password)
                .AddOnSuccessListener(taskCompletionListener)
                .AddOnFailureListener(taskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "Login failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        void InitializeFirebase()
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
            mAuth = FirebaseAuth.Instance;
        }
    }
}