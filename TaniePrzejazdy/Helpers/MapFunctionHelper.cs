using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Com.Google.Maps.Android;
using Java.Util;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using TaniePrzejazdy.Helpers;

namespace TaniePrzejazdy.Helpers
{
    public class MapFunctionHelper
    {
        private readonly string mapKey;
        private readonly GoogleMap map;
        public MapFunctionHelper(string mMapKey, GoogleMap mmap)
        {
            mapKey = mMapKey;
            map = mmap;
        }
        public string GetGeoCodeUrl(double lat, double lng)
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&key={mapKey}";
            return url;
        }
        public async Task<string> GetGeoJsonAsync(string url)
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            var result = await client.GetStringAsync(url);
            return result;
        }

        public async Task<string> FindCoordinateAddress(LatLng position)
        {
            CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
            var url = GetGeoCodeUrl(position.Latitude, position.Longitude);
            var placeAddress = string.Empty;

            //check for internet connection 
            var json = await GetGeoJsonAsync(url);
            if (!string.IsNullOrEmpty(json))
            {
                var geoCodeData = JsonConvert.DeserializeObject<GeocodingParser>(json);
                if (!geoCodeData.status.Contains("ZERO"))
                {
                    if(geoCodeData.results[0] != null)
                    {
                        placeAddress = geoCodeData.results[0].formatted_address;
                    }
                }
            }
            return placeAddress;
        }

        public async Task<string> GetDirectionJson(LatLng location, LatLng destination)
        {
            CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
            var str_origin = $"origin={location.Latitude},{location.Longitude}";
            var str_destination = $"destination={destination.Latitude},{destination.Longitude}";
            var mode = "mode=driving";

            var parameters = $"{str_origin}&{str_destination}&&{mode}";
            var url = $"https://maps.googleapis.com/maps/api/directions/json?{parameters}&key={mapKey}";
            var json = await GetGeoJsonAsync(url);
            return json;
        }

        public void DrawTripOnMap(string json)
        {
            var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            var points = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(points);

            var routeList = new ArrayList();
            foreach(var item in line)
            {
                routeList.Add(item);
            }
            var polylineOptions = new PolylineOptions().AddAll(routeList).InvokeWidth(10).InvokeColor(Color.Teal).InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap()).InvokeJointType(JointType.Round).Geodesic(true);

            var mPolyline = map.AddPolyline(polylineOptions);
            var firstpoint = line[0];
            var lastpoint = line[line.Count - 1];

            var pickupMarkerOptions = new MarkerOptions();
            pickupMarkerOptions.SetPosition(firstpoint);
            pickupMarkerOptions.SetTitle("Pickup Location");
            pickupMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));

            var destinationMarkerOptions = new MarkerOptions();
            destinationMarkerOptions.SetPosition(lastpoint);
            destinationMarkerOptions.SetTitle("Destination");
            destinationMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));

            var pickupMarker = map.AddMarker(pickupMarkerOptions);
            var destinationMarker = map.AddMarker(destinationMarkerOptions);

            var southlng = directionData.routes[0].bounds.southwest.lng;
            var southlat = directionData.routes[0].bounds.southwest.lat;
            var northlng = directionData.routes[0].bounds.northeast.lng;
            var northlat = directionData.routes[0].bounds.northeast.lat;

            var southwest = new LatLng(southlat, southlng);
            var northeast = new LatLng(northlat, northlng);
            var tripBound = new LatLngBounds(southwest, northeast);

            map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(tripBound, 470));
            map.SetPadding(40, 70, 40, 70);
            pickupMarker.ShowInfoWindow();
        }
    }

}