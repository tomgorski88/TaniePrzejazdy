using Android.App;
using Android.Content;
using Firebase.Database;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy.EventListeners
{
    public class UserProfileEventListener : Java.Lang.Object, IValueEventListener
    {
        private readonly ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        private ISharedPreferencesEditor editor;
        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            var test = snapshot;

            if (snapshot.Value != null)
            {
                var fullName = (snapshot.Child("fullname") != null) ? snapshot.Child("fullname").Value.ToString() : string.Empty;
                var email = snapshot.Child("email") != null ? snapshot.Child("email").Value.ToString() : string.Empty;
                var phone = snapshot.Child("phone") != null ? snapshot.Child("phone").Value.ToString() : string.Empty;

                editor.PutString("fullname", fullName);
                editor.PutString("email", email);
                editor.PutString("phone", phone);
                editor.Apply();
            }
        }

        public void Create()
        {
            editor = preferences.Edit();
            var database = AppDataHelper.GetDatabase();
            var userId = AppDataHelper.GetCurrentUser().Uid;
            var profileReference = database.GetReference("users/" + userId);
            profileReference.AddValueEventListener(this);
        }
    }
}