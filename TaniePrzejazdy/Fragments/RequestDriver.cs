using AndroidX.AppCompat.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;

namespace TaniePrzejazdy.Fragments
{
    public class RequestDriver : DialogFragment
    {
        public event EventHandler CancelRequest;
        private Button cancelRequestButton;
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

            cancelRequestButton = (Button)view.FindViewById(Resource.Id.cancelrequestButton);
            cancelRequestButton.Click += CancelRequest_Click;
            faresTxt = (TextView)view.FindViewById(Resource.Id.faresText);
            faresTxt.Text = mfares.ToString() + " PLN";
            return view;
        }

        private void CancelRequest_Click(object sender, System.EventArgs e)
        {
            CancelRequest?.Invoke(this, new EventArgs());
        }

        public RequestDriver(double fares)
        {
            mfares = fares;
        }
    }
}