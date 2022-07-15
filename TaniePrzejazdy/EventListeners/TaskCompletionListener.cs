using System;
using Android.Gms.Tasks;
using Exception = Java.Lang.Exception;
using Object = Java.Lang.Object;

namespace TaniePrzejazdy.EventListeners
{
    public class TaskCompletionListener : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {
        public event EventHandler Success;
        public event EventHandler Failure;
        public void OnSuccess(Object result)
        {
            Success?.Invoke(this, new EventArgs());
        }

        public void OnFailure(Exception e)
        {
            Failure?.Invoke(this, new EventArgs());
        }
    }
}