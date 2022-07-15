using System;
using Android.App;
using Android.Gms.Tasks;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.CoordinatorLayout.Widget;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Google.Android.Material.Snackbar;
using Google.Android.Material.TextField;
using Java.Lang;
using Java.Util;
using TaniePrzejazdy.EventListeners;

namespace TaniePrzejazdy.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/tpTheme", MainLauncher = false)]
    public class RegisterActivity : AppCompatActivity
    {
        private TextInputLayout fullNameText;
        private TextInputLayout phoneText;
        private TextInputLayout emailText;
        private TextInputLayout passwordText;
        private Button registerButton;
        private CoordinatorLayout rootView;
        private string fullname, phone, email, password;

        private FirebaseAuth mAuth;
        private FirebaseDatabase database;
        private readonly TaskCompletionListener taskCompletionListener = new TaskCompletionListener();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);

            InitializeFirebase();
            mAuth = FirebaseAuth.Instance;
            ConnectControls();
        }

        void ConnectControls()
        {
            fullNameText = (TextInputLayout) FindViewById(Resource.Id.fullNameText);
            phoneText = (TextInputLayout) FindViewById(Resource.Id.phoneText);
            emailText = (TextInputLayout) FindViewById(Resource.Id.emailText);
            passwordText = (TextInputLayout) FindViewById(Resource.Id.passwordText);
            rootView = (CoordinatorLayout) FindViewById(Resource.Id.rootView);
            registerButton = (Button) FindViewById(Resource.Id.registerButton);

            registerButton.Click += RegisterButton_Click;
        }

        private void RegisterButton_Click(object sender, System.EventArgs e)
        {
            fullname = fullNameText.EditText.Text;
            phone = phoneText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if (fullname.Length < 3)
            {
                Snackbar.Make(rootView, "Please enter a valid name.", Snackbar.LengthShort).Show();
                return;
            } else if (phone.Length < 9)
            {
                Snackbar.Make(rootView, "Please enter a valid phone number.", Snackbar.LengthShort).Show();
                return;
            } else if (!email.Contains('@'))
            {
                Snackbar.Make(rootView, "Please enter a valid email.", Snackbar.LengthShort).Show();
                return;
            }
            else if (password.Length < 8)
            {
                Snackbar.Make(rootView, "Please enter a password longer than 7 characters.", Snackbar.LengthShort).Show();
                return;
            }

            RegisterUser(fullname, phone, email, password);
        }

        void RegisterUser(string user, string phone, string email, string password)
        {
            taskCompletionListener.Success += TaskCompletionListener_Success;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;
            mAuth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, taskCompletionListener)
                .AddOnFailureListener(this, taskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User registration failed", Snackbar.LengthShort).Show();
        }
        public void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User registration was successful", Snackbar.LengthShort).Show();
            var userMap = new HashMap();
            userMap.Put("email", email);
            userMap.Put("phone", phone);
            userMap.Put("fullname", fullname);

            var userReference = database.GetReference("users/" + mAuth.CurrentUser.Uid);
            userReference.SetValue(userMap);
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
                database = FirebaseDatabase.GetInstance(app);
            }
    }
}