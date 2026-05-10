using AttendanceSync.Domain.Dtos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure.Clients
{

    public class KayanClient : IKayanClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public KayanClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> SendBulkAttendanceAsync(List<KayanAttendanceRequest> request)
        {
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/v1/TimeAttendance/AddOnlineAttendanceBulk");

            httpRequest.Headers.Add("accept", "text/plain");
            httpRequest.Headers.Add("clientID", _config["Kayan:ClientId"]);
            httpRequest.Headers.Add("ClientSecret", _config["Kayan:ClientSecret"]);

            httpRequest.Content = JsonContent.Create(
                request,
                mediaType: new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Kayan bulk send failed. StatusCode: {response.StatusCode}. Response: {responseBody}");

            return responseBody;
        }
    }

}

