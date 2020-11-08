/*
 * Jarod Guerrero
 * CSS FourThreeSix A - Cloud Computing
 * 10/19/19
 * REST
 * 
 * Takes as input the name of a city and outputs licensed medical
 * doctor Vincent-Denache's (Dr. D for short) recommended daily Vitamin D
 * dosaged based on the current weather conditions and cloudiness. 
 */

using System;
using System.Net;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Program2Weather
{
    public class Rootobject
    {
        public Coord coord { get; set; }
        public Weather[] weather { get; set; }
        public string _base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Rain rain { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }
    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }
    public class Main
    {
        public float temp { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
    }
    public class Wind
    {
        public float speed { get; set; }
        public int deg { get; set; }
        public float gust { get; set; }
    }
    public class Rain
    {
        public float _1h { get; set; }
    }
    public class Clouds
    {
        public int all { get; set; }
    }
    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public float message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }
    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    class Program2Weather
    {
        /*
         * Returns a non-medical prescription for a recommended dosage
         * of Vitamin D for the day based on an inputted weather and cloudiness level.
         * 
         * Weather Code corresponds to the OpenWeatherMap codes found at:
         * https://openweathermap.org/weather-conditions
         * 
         * Cloudiness ranges from 0-100
         */
        static int ComputeVDScore(int weatherID, int cloudiness)
        {
            double VDScore = 0.0;
            // Thunderstorms, drizzle, rain, or snow
            if (weatherID < 300) // 2xx Thunderstorms
            {
                VDScore += 2000;
            }
            else if (weatherID < 400) // 3xx drizzle
            {
                VDScore += 1500;
            }
            else if (weatherID < 600) // 5xx Rain 
            {
                VDScore += 1800;
            }
            else if (weatherID < 700) // 6xx Snow
            {
                VDScore += 2000;
            }
            else if (weatherID < 800) // Atmospheric disruption: mist, fog, tornado, ash, etc
            {
                VDScore += 2000;
            }
            else if (weatherID > 800) // 80x Cloudiness
            {
                VDScore += 800;
            }
            // Factor in cloudiness multiplier.
            // Calculated based on 50+ years of rigourous supplement research
            VDScore *= (cloudiness * .01 + .27);
            return Convert.ToInt32(VDScore);
        }

        /*
         * Looks up a city name's city ID corresponding to OpenWeatherMap's
         * city list. Refers to an included city.list.json file
         * Returns values:  City ID if single result found
         *                  -1 if city not found
         *                  Number of matching results found, outputted to the list parameter
         * Adds all matching results to the List<int> cities parameter
         */
        private static int LookupCity(string cityName, ref List<int> cities, ref JsonTextReader reader)
        {
            int retVal = -1, cityID = 0, cityCount = 0;
            while (reader.Read())
            {
                //Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                //if (reader.TokenType.Equals("Property");
                //  reader.Value == cityName.)

                // Temporarily store the city ID

                if (reader.TokenType.ToString().Equals("PropertyName") && reader.Value.ToString().Equals("id"))
                {
                    reader.Read();
                    int.TryParse(reader.Value.ToString(), out cityID);
                    reader.Read();
                    reader.Read();
                    if (reader.Value.ToString().Equals(cityName, StringComparison.OrdinalIgnoreCase))
                    {
                        retVal = cityID;
                        cities.Add(retVal);
                        cityCount++;
                    }
                }
            }
            // Return city count if more than 1 matching city was found,
            // otherwise return the single cityID.
            return cityCount > 1 ? cityCount : retVal;
        }

        /*
         * Takes as input the name of a city and outputs licensed medical
         * doctor Vincent-Denache's (Dr. D for short) recommended daily Vitamin D
         * dosaged based on the current weather conditions and cloudiness.
         * 
         * Returns 0 and outputs a recommendation if input was valid.
         * Returns -1 if there was an error with the API key or city name.
         */
        static int Main(string[] args)
        {
            using (var client = new HttpClient(new HttpClientHandler
            { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                if(args.Length == 0)
                {
                    Console.Error.WriteLine("Error: Provide a city name");
                    return -1;
                }
                string cityName = args[0];
                for(int i=1; i < args.Length; i++)
                {

                    cityName += (" " + args[i]);
                }

                client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
                string weather = "weather?";
                string cityList = "";

                try
                {
                    cityList = File.ReadAllText(@"city.list.json");

                }
                catch
                {
                    Console.Error.WriteLine("Error: city.list.json not found");
                    return -1;
                }

                JsonTextReader reader = new JsonTextReader(new StringReader(cityList));

                List<int> cities = new List<int>(); // Currently not used, but holds cities with the same name

                // Perform a lookup in the MASSIVE json list of cities.
                int cityCode = LookupCity(cityName, ref cities, ref reader);

                if (cityCode < 100) // City not found in list or multiple possible results
                {
                    // Instead, query OpenWeatherMap directly
                    weather += ("q=" + cityName);
                }
                else // Single city code found
                {
                    weather += ("id=" + cityCode.ToString());
                }
                weather += "&APPID=";
                // Call OpenWeatherMap Current Weather API
                HttpResponseMessage response = client.GetAsync(weather).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Result JSON:\n" + result + "\n");

                // ------------------------------
                // Parse error status codes
                if ((int)response.StatusCode == 404)
                {
                    Console.WriteLine("Dr. D: I can't prescribe anything if you're from\n" +
                        "Nowheresville. Please give me an actual city name.");
                    return -1;
                } else if((int)response.StatusCode == 401)
                {
                    Console.WriteLine("Dr. D: Something's wrong with my API key. Come back later.");
                    return -1;
                }

                Rootobject weatherDetails = JsonConvert.DeserializeObject<Rootobject>(result);

                // ------------------------------
                // Output Dr. D's Recommendation
                int VDScore = ComputeVDScore(weatherDetails.weather[0].id, weatherDetails.clouds.all);
                if (weatherDetails.id == 5809844)
                {
                    // Exclusive Seattle message
                    // Prescribe the max dosage 
                    Console.WriteLine("Dr. D: Ah, so you're from Seattle. I recommend you\n" +
                        "take the maximum dosage of 2000IU Vitamin D per day according\n" +
                        "to a VD score of " + VDScore.ToString());
                } else if (VDScore >= 2000) // Do 2000 IU's
                {
                    // Prescribe VDScore IU's
                    Console.WriteLine("Dr. D: Based on the weather in " + weatherDetails.name
                        + ",\nI recommend the maximum dosage of 2000IU Vitamin D per day\n" +
                        "according to a VD score of " + VDScore.ToString());
                }
                else if(VDScore >= 1000)
                {
                    Console.WriteLine("Dr. D: Based on the weather in " + weatherDetails.name
                        + ",\nI recommend taking at least 1000IU Vitamin D per day according\n" +
                        "to a VD score of " + VDScore.ToString());
                    // No VD needed, just go outside
                    // for a half hour and soak in the sun
                }
                else
                {
                    Console.WriteLine("Dr. D: The skies in " + weatherDetails.name + " are looking\n" +
                        "clear. I recommend going outside for a half hour and soaking up some\n" +
                        "Vitamin D naturally. Your VD score is " + VDScore.ToString());
                }
                return 0;
            }
        }
    }
}
