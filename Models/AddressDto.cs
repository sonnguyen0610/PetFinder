using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PetFinderAPI.Models
{
    public class AddressDto
    {
        public string Address { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }

        private class ApiAddressLatLon
        {
            public double lon { get; set; }
            public double lat { get; set; }
        }

        private ApiAddressLatLon callGeocodingAPI(string address)
        {
            ApiAddressLatLon dataObjects = new ApiAddressLatLon()
            {
                lat = 0,
                lon = 0,
            };

            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/search");

            //// Add an Accept header for JSON format.
            //client.DefaultRequestHeaders.Accept.Add(
            //new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("User-Agent", "C# App");

            //// List data response.
            //HttpResponseMessage response = client.GetAsync("?q=" + address + "&format=jsonv2").Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            //if (response.IsSuccessStatusCode)
            //{
            //    // Parse the response body.
            //    var results = response.Content.ReadAsAsync<List<ApiAddressLatLon>>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
            //    if (results != null && results.Count != 0)
            //    {
            //        dataObjects = results[0];
            //    }

            //}
            //else
            //{
            //    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            //}

            //// Make any other calls using HttpClient here.

            //// Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            //client.Dispose();

            return dataObjects;
        }

        public AddressDto(string address)
        {
            Address = address;
            ApiAddressLatLon apiAddressLatLon = callGeocodingAPI(address);
            Long = apiAddressLatLon.lon;
            Lat = apiAddressLatLon.lat;
        }

        public AddressDto()
        {

        }
    }
}
