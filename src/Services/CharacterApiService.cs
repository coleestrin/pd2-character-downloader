using System;
using System.Net.Http;
using System.Threading.Tasks;
using D2SLib.Configuration;
using D2SLib.Model.Api;
using Newtonsoft.Json;

namespace D2SLib.Services
{
    public class CharacterApiService : IDisposable
    {
        private readonly HttpClient _httpClient;

        public CharacterApiService()
        {
            _httpClient = new HttpClient();
        }

        public CharacterApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<CharacterData> GetCharacterDataAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            try
            {
                var apiUrl = AppSettings.ApiBaseUrl + username;
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var characterJson = await response.Content.ReadAsStringAsync();
                var characterData = JsonConvert.DeserializeObject<CharacterData>(characterJson);

                if (characterData == null)
                    throw new InvalidOperationException($"Failed to deserialize character data for username: {username}");

                return characterData;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to fetch character data for username: {username}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse character data for username: {username}", ex);
            }
        }

        public CharacterData GetCharacterData(string username)
        {
            return GetCharacterDataAsync(username).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}