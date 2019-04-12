using AlexaDemo.SpaceFacts.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AlexaDemo.SpaceFacts
{
    public class NgrokClient
    {
        public const string NGROK_SERVER = "http://localhost:4040";

        private static readonly HttpClient _httpClient;

        private ILogger _logger;

        static NgrokClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(NGROK_SERVER);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public NgrokClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<Tunnel>> GetTunnelListAsync()
        {
            var response = await _httpClient.GetAsync("/api/tunnels");

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();

                NgrokTunnelsResponse apiResponse = null;
                try
                {
                    apiResponse = JsonConvert.DeserializeObject<NgrokTunnelsResponse>(responseText);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize ngrok tunnel response");
                    throw;
                }

                return apiResponse.Tunnels;
            }
            return null;
        }
    }
}
