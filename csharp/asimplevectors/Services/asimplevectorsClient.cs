using asimplevectors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace asimplevectors.Services
{
    public class asimplevectorsClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public asimplevectorsClient(string baseUrl, ILogger logger = null, string token = null)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<T> SendRequestAsync<T>(HttpMethod method, string endpoint, object payload = null)
        {
            var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");

            // Log Request Details (only if logger is not null)
            _logger?.LogInformation("Sending Request: {Method} {Url}", method, request.RequestUri);
            if (payload != null)
            {
                var jsonPayload = JsonSerializer.Serialize(payload, _jsonOptions);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                _logger?.LogDebug("Request Payload: {Payload}", jsonPayload);
            }

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log Response Details (only if logger is not null)
                _logger?.LogInformation("Response Status: {StatusCode}", response.StatusCode);
                _logger?.LogDebug("Response Content: {Content}", responseContent);

                response.EnsureSuccessStatusCode();

                return JsonSerializer.Deserialize<T>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred during request: {Method} {Url}", method, request.RequestUri);
                throw;
            }
        }

        // Cluster Methods
        public async Task InitClusterAsync()
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/cluster/init");
        }

        public async Task AddLearnerAsync(int nodeId, string apiAddr, string rpcAddr)
        {
            var body = new { NodeId = nodeId, ApiAddr = apiAddr, RpcAddr = rpcAddr };
            await SendRequestAsync<object>(HttpMethod.Post, "/cluster/add-learner", body);
        }

        public async Task ChangeMembershipAsync(List<int> membership)
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/cluster/change-membership", membership);
        }

        public async Task<ClusterMetricsResponse> GetClusterMetricsAsync()
        {
            // Fetch the raw response as a dictionary
            var response = await SendRequestAsync<Dictionary<string, object>>(HttpMethod.Get, "/cluster/metrics");

            // Access the nested "Ok" key
            var okSection = JsonSerializer.Serialize(response["Ok"]);
            var metrics = JsonSerializer.Deserialize<ClusterMetricsResponse>(okSection, _jsonOptions);

            return metrics;
        }

        // Space Methods
        public async Task CreateSpaceAsync(SpaceRequest spaceRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/api/space", spaceRequest);
        }

        public async Task<SpaceResponse> GetSpaceAsync(string spaceName)
        {
            return await SendRequestAsync<SpaceResponse>(HttpMethod.Get, $"/api/space/{spaceName}");
        }

        public async Task UpdateSpaceAsync(string spaceName, SpaceRequest updateRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}", updateRequest);
        }

        public async Task DeleteSpaceAsync(string spaceName)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/space/{spaceName}");
        }

        public async Task<ListSpacesResponse> ListSpacesAsync()
        {
            return await SendRequestAsync<ListSpacesResponse>(HttpMethod.Get, "/api/spaces");
        }

        // Version Methods
        public async Task CreateVersionAsync(string spaceName, VersionRequest versionRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}/version", versionRequest);
        }

        public async Task<ListVersionsResponse> ListVersionsAsync(string spaceName)
        {
            return await SendRequestAsync<ListVersionsResponse>(HttpMethod.Get, $"/api/space/{spaceName}/versions");
        }

        public async Task<VersionResponse> GetVersionByIdAsync(string spaceName, int versionId)
        {
            return await SendRequestAsync<VersionResponse>(HttpMethod.Get, $"/api/space/{spaceName}/version/{versionId}");
        }

        public async Task<VersionResponse> GetDefaultVersionAsync(string spaceName)
        {
            return await SendRequestAsync<VersionResponse>(HttpMethod.Get, $"/api/space/{spaceName}/version");
        }

        // Vector Methods
        public async Task CreateVectorAsync(string spaceName, VectorRequest vectorRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}/vector", vectorRequest);
        }

        public async Task<GetVectorsResponse> GetVectorsByVersionAsync(string spaceName, int versionId = 0, int? start = null, int? limit = null)
        {
            var query = $"?start={start}&limit={limit}";
            Dictionary<string, object> responseJson;

            if (versionId > 0)
            {
                responseJson = await SendRequestAsync<Dictionary<string, object>>(
                    HttpMethod.Get,
                    $"/api/space/{spaceName}/version/{versionId}/vectors{query}"
                );
            }
            else
            {
                responseJson = await SendRequestAsync<Dictionary<string, object>>(
                    HttpMethod.Get,
                    $"/api/space/{spaceName}/vectors{query}"
                );
            }

            // Extract and process the "vectors" field
            var rawVectors = JsonSerializer.Serialize(responseJson["vectors"]);
            var nestedVectors = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(rawVectors, _jsonOptions);

            // Convert nested "data" structure to `float[]`
            var vectors = nestedVectors.Select(vector =>
            {
                var id = vector["id"].GetInt32();

                // Extract the nested data array
                var nestedData = vector["data"].GetProperty("data");
                var dataArray = JsonSerializer.Deserialize<float[]>(nestedData.GetRawText());

                var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(vector["metadata"].GetRawText());

                return new VectorData
                {
                    Id = id,
                    Data = dataArray,
                    Metadata = metadata
                };
            }).ToList();

            return new GetVectorsResponse { Vectors = vectors };
        }

        // Search Methods
        public async Task<List<SearchResult>> SearchVectorAsync(string spaceName, SearchRequest searchRequest)
        {
            var searchResults = await SendRequestAsync<List<SearchResult>>(
                HttpMethod.Post,
                $"/api/space/{spaceName}/search",
                searchRequest
            );

            return searchResults;
        }

        public async Task<List<SearchResult>> SearchVectorByVersionAsync(string spaceName, int versionId, SearchRequest searchRequest)
        {
            var searchResults = await SendRequestAsync<List<SearchResult>>(
                HttpMethod.Post,
                $"/api/space/{spaceName}/version/{versionId}/search",
                searchRequest
            );

            return searchResults;
        }

        // Snapshot Methods
        public async Task CreateSnapshotAsync(CreateSnapshotRequest snapshotRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/api/snapshot", snapshotRequest);
        }

        public async Task<ListSnapshotsResponse> ListSnapshotsAsync()
        {
            return await SendRequestAsync<ListSnapshotsResponse>(HttpMethod.Get, "/api/snapshots");
        }

        public async Task DeleteSnapshotAsync(string snapshotDate)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/snapshot/{snapshotDate}/delete");
        }

        public async Task RestoreSnapshotAsync(string snapshotDate)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/snapshot/{snapshotDate}/restore");
        }

        // RBAC Methods
        public async Task<RbacTokenResponse> CreateRbacTokenAsync(RbacTokenRequest rbacRequest)
        {
            return await SendRequestAsync<RbacTokenResponse>(HttpMethod.Post, "/api/security/tokens", rbacRequest);
        }

        public async Task<ListRbacTokensResponse> ListRbacTokensAsync()
        {
            return await SendRequestAsync<ListRbacTokensResponse>(HttpMethod.Get, "/api/security/tokens");
        }

        public async Task DeleteRbacTokenAsync(string token)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/security/tokens/{token}");
        }

        public async Task UpdateRbacTokenAsync(string token, RbacTokenRequest rbacRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Put, $"/api/security/tokens/{token}", rbacRequest);
        }

        // Key-Value Methods
        public async Task PutKeyValueAsync(string spaceName, string key, KeyValueRequest keyValue)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}/key/{key}", keyValue);
        }

        public async Task<string> GetKeyValueAsync(string spaceName, string key)
        {
            var response = await SendRequestAsync<Dictionary<string, string>>(HttpMethod.Get, $"/api/space/{spaceName}/key/{key}");
            return response["text"]; 
        }

        public async Task<ListKeysResponse> ListKeysAsync(string spaceName)
        {
            return await SendRequestAsync<ListKeysResponse>(HttpMethod.Get, $"/api/space/{spaceName}/keys");
        }

        public async Task DeleteKeyValueAsync(string spaceName, string key)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/space/{spaceName}/key/{key}");
        }
    }
}
