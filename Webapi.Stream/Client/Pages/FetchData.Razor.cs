using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Webapi.Stream.Shared;

namespace Webapi.Stream.Client.Pages
{
    public partial class FetchData
    {
        public List<WeatherForecast> forecasts;
        public CancellationTokenSource cts = new CancellationTokenSource();
        protected override async Task OnInitializedAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "WeatherForecast");
            request.SetBrowserResponseStreamingEnabled(true); // Enable response streaming
            using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using var stream = await response.Content.ReadAsStreamAsync();
            await SetForcatsDataWithJsonTextReader(stream);
            //await SetForcatsDataWithJsonTextReader(stream);
        }

        private async Task SetForcatsDataWithJsonTextReader(System.IO.Stream stream)
        {
            var serializer = new JsonSerializer();
            forecasts = new List<WeatherForecast>();
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                while (await jr.ReadAsync(cts.Token))
                {
                    if (jr.TokenType != JsonToken.StartArray && jr.TokenType != JsonToken.EndArray)
                    {
                        forecasts.Add(serializer.Deserialize<WeatherForecast>(jr));
                        StateHasChanged();
                    }
                }
            }
        }
        private async Task SetForcatsDataWithStream(System.IO.Stream stream)
        {
            forecasts = new List<WeatherForecast>();
            while (!cts.Token.IsCancellationRequested)
            {

                Memory<byte> buffer = new Memory<byte>(new byte[1000]);

                var read = await stream.ReadAsync(buffer, cts.Token);

                if (read == 0) // End of stream
                    return;
                var str = System.Text.Encoding.Default.GetString(buffer.ToArray());
                Console.WriteLine(str);
                try
                {
                    var wf = JsonConvert.DeserializeObject<WeatherForecast>(str);
                    forecasts.Add(wf);
                }
                catch
                {

                }
                StateHasChanged();
            }
        }
    }
}
