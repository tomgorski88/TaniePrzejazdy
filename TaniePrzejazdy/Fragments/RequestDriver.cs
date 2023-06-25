using AndroidX.AppCompat.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

namespace TaniePrzejazdy.Fragments
{
    public class RequestDriver : DialogFragment
    {
        private Button cancelRequest;
        private TextView faresTxt;
        private double mfares;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.request_driver, container, false);

            cancelRequest = (Button)view.FindViewById(Resource.Id.cancelrequestButton);
            faresTxt = (TextView)view.FindViewById(Resource.Id.faresText);
            faresTxt.Text = mfares.ToString() + " PLN";
            return view;
        }

        public RequestDriver(double fares)
        {
            mfares = fares;
        }
    }
}