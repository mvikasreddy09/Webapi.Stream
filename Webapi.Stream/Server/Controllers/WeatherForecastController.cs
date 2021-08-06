using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Webapi.Stream.Shared;

namespace Webapi.Stream.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public async Task GetAsync(CancellationToken cancellationToken)
        {
            var rng = new Random();
            var range = Enumerable.Range(1, 100).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();
            Response.Headers.Add("Content-Type", "text/event-stream");

            for (int i = 0; i < range.Length; i++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch
                {
                    throw;
                }
                await Task.Delay(rng.Next(100,1000));
                string dataItem = $"{JsonConvert.SerializeObject(range[i])}";
                if(i ==0)
                {
                    dataItem = "[" + dataItem;
                    dataItem += ",";
                }
                else if(i == range.Length-1)
                {
                    dataItem = dataItem + "]";
                }
                else
                {
                    dataItem += ",";
                }
                byte[] dataItemBytes = ASCIIEncoding.ASCII.GetBytes(dataItem);
                await Response.Body.WriteAsync(dataItemBytes, 0, dataItemBytes.Length);
                await Response.Body.FlushAsync();
            }
        }
        //[HttpGet]
        //public HttpResponseMessage Get()
        //{
        //    var response = new HttpResponseMessage();
        //    var content = new PushStreamContent(new Action<System.IO.Stream, HttpContent, TransportContext>(WriteContent), "application/json");

        //    response.Headers.TransferEncodingChunked = true;
        //    response.Content = content;

        //    return response;
        //}
        //public static void WriteContent(System.IO.Stream stream, HttpContent content, TransportContext context)
        //{
        //    var serializer = JsonSerializer.CreateDefault();
        //    var rng = new Random();
        //    using (var sw = new StreamWriter(stream))
        //    using (var jw = new JsonTextWriter(sw))
        //    {
        //        jw.WriteStartArray();
        //        foreach (var id in Enumerable.Range(1, 10))
        //        {
        //            serializer.Serialize(jw, new WeatherForecast
        //            {
        //                Date = DateTime.Now.AddDays(id),
        //                TemperatureC = rng.Next(-20, 55),
        //                Summary = Summaries[rng.Next(Summaries.Length)]
        //            });
        //        }
        //        jw.WriteEndArray();
        //    }
        //}
        //[HttpGet]
        //public async IAsyncEnumerable<WeatherForecast> GetAsync()
        //{
        //    var rng = new Random();
        //    var range = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = Summaries[rng.Next(Summaries.Length)]
        //    }).ToArray();
        //    Response
        //    foreach(var r in range)
        //    {
        //        await Task.Delay(1000);
        //        yield return r;
        //    }
        //}
    }
}
