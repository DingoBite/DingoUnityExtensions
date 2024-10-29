using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DingoUnityExtensions.NetWorking
{
    public class HttpClientWrapper : IDisposable
    {
        private readonly HttpClient _httpClient = new();
        public float DefaultTimeout { get; }

        public HttpClientWrapper() => DefaultTimeout = (float)_httpClient.Timeout.TotalSeconds;

        public void SetTimeout(float timeoutSeconds) => _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        public void ResetTimeout() => _httpClient.Timeout = TimeSpan.FromSeconds(DefaultTimeout);
        
        public async Task<HttpResponseMessage> PostRequest(string url, HttpContent content, CancellationToken token = default)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync(url, content, token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            response?.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<HttpResponseMessage> GetRequest(string url, CancellationToken token = default)
        {
            var response = await _httpClient.GetAsync(url, token);
            response.EnsureSuccessStatusCode();
            return response;
        }
        
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}