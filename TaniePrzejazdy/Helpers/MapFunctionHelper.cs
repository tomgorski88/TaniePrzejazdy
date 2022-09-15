using Android.Gms.Maps.Model;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace TaniePrzejazdy.Helpers
{
    public class MapFunctionHelper
    {
        private readonly string mapKey;
        public MapFunctionHelper(string mMapKey)
        {
            mapKey = mMapKey;
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
    }

}