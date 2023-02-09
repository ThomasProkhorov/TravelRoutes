using Newtonsoft.Json;
using TravelRoutes.Services.Interfaces;
using System.Net;

namespace UnitTests.Mocks
{
    internal class SimpleTestHttpClientProvider : IHttpClientProvider
    {
        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken token)
        {
            return requestUri.Contains("provider-one") ? new HttpResponseMessage(HttpStatusCode.OK) : new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken token)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent( requestUri.Contains("provider-one") ? "{\"Routes\":[{\"From\":\"Moscow\",\"To\":\"Sochi\",\"DateFrom\":\"2023-02-11T12:00:00.000\",\"DateTo\":\"2023-02-11T13:00:00.000\",\"Price\":\"10000.00\",\"TimeLimit\":\"2023-02-11T00:00:00.000\"}," +
                            "{\"From\":\"Moscow\",\"To\":\"Sochi\",\"DateFrom\":\"2023-02-12T12:00:00.000\",\"DateTo\":\"2023-02-12T14:10:00.000\",\"Price\":\"11000.00\",\"TimeLimit\":\"2023-02-12T00:00:00.000\"}]}" :
                            "{\"Routes\":[{\"Departure\":{\"Point\":\"Moscow\",\"Date\":\"2023-02-11T12:00:00.000\"},\"Arrival\":{\"Point\":\"Sochi\",\"Date\":\"2023-02-11T13:00:00.000\"},\"Price\":\"10000.00\",\"TimeLimit\":\"2023-02-11T00:00:00.000\"}," +
                            "{\"Departure\":{\"Point\":\"Moscow\",\"Date\":\"2023-02-12T12:00:00.000\"},\"Arrival\":{\"Point\":\"Sochi\",\"Date\":\"2023-02-12T14:10:00.000\"},\"Price\":\"10000.00\",\"TimeLimit\":\"2023-02-12T00:00:00.000\"}]}");
            return result;
        }
    }

    internal class BigDataHttpClientProvider : IHttpClientProvider
    {
        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken token)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken token)
        {
            var items = new List<Route>();
            using (StreamReader r = new StreamReader(".\\..\\..\\..\\routes.json"))
            {
                string json = await r.ReadToEndAsync();
                items = JsonConvert.DeserializeObject<List<Route>>(json);
            }

            var jsonResult = JsonConvert.SerializeObject(requestUri.Contains("provider-one") ?
                items?.Select(i => new ProviderOneRoute
                {
                    From = i.Origin,
                    To = i.Destination,
                    DateFrom = i.OriginDateTime,
                    DateTo = i.DestinationDateTime,
                    Price = i.Price,
                    TimeLimit = i.TimeLimit
                }).ToArray() :
                 items?.Select(i => new ProviderTwoRoute
                 {
                     Departure = new ProviderTwoPoint { Point = i.Origin, Date = i.OriginDateTime },
                     Arrival = new ProviderTwoPoint { Point = i.Destination, Date = i.DestinationDateTime },
                     Price = i.Price,
                     TimeLimit = i.TimeLimit
                 }).ToArray());


            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent("{\"Routes\":" + jsonResult + "}");

            return result;
        }
    }
}
