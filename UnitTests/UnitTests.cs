using AutoMapper;
using Newtonsoft.Json;
using TravelRoutes.Services;
using TravelRoutes.Services.Infrastructure;
using TravelRoutes.Services.Interfaces;
using System.Reflection;
using UnitTests.Mocks;

namespace UnitTests
{
    [TestClass]
    public class UnitTests
    {
        private readonly Mapper _mapper;
        private readonly FakeEnviromentService _enviroment;

        public UnitTests()
        {
            var config = new MapperConfiguration(cfg => AutoMapperConfig.Init(new Assembly[] { typeof(SearchService).Assembly }, cfg));
            _mapper = new Mapper(config);
            _enviroment = new FakeEnviromentService();
        }

        [TestMethod]
        public void PingApiTest()
        {
            var searchService = new SearchService(_mapper, _enviroment, new RouteCacheService(_enviroment), new SimpleTestHttpClientProvider());

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
                        
            var response = searchService.IsAvailableAsync(token).Result;            
            Assert.AreEqual(true, response);
        }

        [TestMethod]
        public void SimpleSearchApiTest()
        {
            var searchService = new SearchService(_mapper, _enviroment, new RouteCacheService(_enviroment), new SimpleTestHttpClientProvider());

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            var request = new SearchRequest { Origin = "Moscow", Destination = "Sochi", OriginDateTime = new DateTime(2023, 02, 11) };
            var response = searchService.SearchAsync(request, token).Result;
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Routes.Length);
            var actual = JsonConvert.SerializeObject(response.Routes[0]);
            var expected = JsonConvert.SerializeObject(new Route
            {
                Id = response.Routes[0].Id,
                Origin = "Moscow",
                Destination = "Sochi",
                OriginDateTime = new DateTime(2023, 02, 11, 12, 0, 0),
                DestinationDateTime = new DateTime(2023, 02, 11, 13, 0, 0),
                Price = 10000.00m,
                TimeLimit = new DateTime(2023, 02, 11, 0, 0, 0)
            });
            Assert.AreEqual(expected, actual);

            request = new SearchRequest { Origin = "", Destination = "", OriginDateTime = new DateTime() };
            response = searchService.SearchAsync(request, token).Result;
            Assert.AreEqual(10000.00m, response.MinPrice);
            Assert.AreEqual(11000.00m, response.MaxPrice);
            Assert.AreEqual(60, response.MinMinutesRoute);
            Assert.AreEqual(130, response.MaxMinutesRoute);
        }

        [TestMethod]
        public void BigDataSearchApiTest()
        {
            var searchService = new SearchService(_mapper, _enviroment, new RouteCacheService(_enviroment), new BigDataHttpClientProvider());

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            var request = new SearchRequest { Origin = "", Destination = "", OriginDateTime = new DateTime() };
            var response = searchService.SearchAsync(request, token).Result;
            Assert.IsNotNull(response);
            Assert.AreEqual(10000, response.Routes.Length);
            Assert.AreEqual(5057.70m, response.MinPrice);
            Assert.AreEqual(499956.72m, response.MaxPrice);
            Assert.AreEqual(-170019, response.MinMinutesRoute);
            Assert.AreEqual(169454, response.MaxMinutesRoute);
            Thread.Sleep(3000);
            request = new SearchRequest { Origin = "", Destination = "", OriginDateTime = new DateTime(), Filters = new SearchFilters { OnlyCached = true } };
            response = searchService.SearchAsync(request, token).Result;
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Routes.Length < 10000);
        }

        [TestMethod]
        public void CancellationSearchApiTest()
        {
            var searchService = new SearchService(_mapper, _enviroment, new RouteCacheService(_enviroment), new HttpClientProvider());

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            Task<SearchResponse>? task = null;
            try
            {
                var request = new SearchRequest { Origin = "M", Destination = "M", OriginDateTime = new DateTime() };                
                task = searchService.SearchAsync(request, token);
                task.Wait();
            } catch (Exception ex)
            {
                Assert.AreEqual("System.Net.Http", ex.InnerException?.Source);
                Assert.AreEqual(TaskStatus.Faulted, task?.Status);
            }

            try
            {
                var request = new SearchRequest { Origin = "M", Destination = "M", OriginDateTime = new DateTime() };                
                task = searchService.SearchAsync(request, token);
                cancelTokenSource.Cancel();
                task.Wait();
            }
            catch (Exception ex)
            {
                Assert.IsNull(ex.InnerException?.Source);
                Assert.AreEqual(TaskStatus.Canceled, task?.Status);
            }
        }

        [TestMethod]
        public void FilteringSearchApiTest()
        {
            var searchService = new SearchService(_mapper, _enviroment, new RouteCacheService(_enviroment), new BigDataHttpClientProvider());

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            var request = new SearchRequest { 
                Origin = "Mo", 
                Destination = "S", 
                OriginDateTime = new DateTime(),
                Filters = new SearchFilters
                {
                    DestinationDateTime= new DateTime(2023, 02, 22),
                    MaxPrice = 200000,
                    MinTimeLimit = new DateTime(2023, 02, 20)
                }
            };
            var response = searchService.SearchAsync(request, token).Result;
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Routes.Length);
            var actual = JsonConvert.SerializeObject(response.Routes[0]);
            var expected = JsonConvert.SerializeObject(new Route
            {
                Id = response.Routes[0].Id,
                Origin = "Moreno Valley",
                Destination = "St. Louis",
                OriginDateTime = new DateTime(2023, 02, 19, 9, 45, 37).ToUniversalTime(),
                DestinationDateTime = new DateTime(2023, 02, 22, 13, 49, 24).ToUniversalTime(),
                Price = 120233.37m,
                TimeLimit = new DateTime(2023, 05, 28, 1, 55, 34).ToUniversalTime()
            });
            Assert.AreEqual(expected, actual);

            actual = JsonConvert.SerializeObject(searchService.Get(response.Routes[0].Id.ToString()));
            Assert.AreEqual(expected, actual);

            Assert.IsNull(searchService.Get("yhfuyfujyfu"));
        }
    }    
}