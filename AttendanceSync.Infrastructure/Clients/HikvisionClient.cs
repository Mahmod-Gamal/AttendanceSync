using AttendanceSync.Domain.Dtos;
using AttendanceSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure.Clients
{

    public class HikvisionClient : IHikvisionClient
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public HikvisionClient(HttpClient httpClient, AppDbContext db, IConfiguration config)
        {
            _httpClient = httpClient;
            _db = db;
            _config = config;
        }

        public async Task<string> GetValidTokenAsync()
        {
            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var savedToken = await _db.HikvisionTokens
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (savedToken != null && savedToken.ExpireTime > nowUnix + 300)
                return savedToken.AccessToken;

            var request = new HikvisionLoginRequest
            {
                AppKey = _config["Hikvision:AppKey"]!,
                SecretKey = _config["Hikvision:SecretKey"]!
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/hccgw/platform/v1/token/get",
                request);

            var result = await response.Content.ReadFromJsonAsync<HikvisionLoginResponse>();

            if (!response.IsSuccessStatusCode || result?.ErrorCode != "0" || result.Data == null)
                throw new Exception("Failed to authenticate with Hikvision API.");

            var token = new HikvisionToken
            {
                AccessToken = result.Data.AccessToken,
                ExpireTime = result.Data.ExpireTime,
                CreatedAt = DateTime.UtcNow
            };

            _db.HikvisionTokens.Add(token);
            await _db.SaveChangesAsync();

            return token.AccessToken;
        }

        public async Task<HikvisionAttendanceResponse?> GetAttendanceAsync(
            string token,
            DateTime beginTime,
            DateTime endTime,
            int pageIndex,
            int pageSize)
        {
            var request = new HikvisionAttendanceRequest
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchCriteria = new HikvisionAttendanceSearchCriteria
                {
                    BeginTime = beginTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    EndTime = endTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Type = 1
                }
            };

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/hccgw/acs/v1/event/certificaterecords/search");

            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Token", token);
            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Hikvision attendance fetch failed. StatusCode: {response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<HikvisionAttendanceResponse>();
        }
    }

}

