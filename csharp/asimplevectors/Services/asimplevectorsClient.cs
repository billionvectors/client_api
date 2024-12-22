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
    /// <summary>
    /// Client for interacting with the asimplevectors API.
    /// Provides methods for managing clusters, spaces, versions, vectors, snapshots, RBAC tokens, and key-value data.
    /// </summary>
    public class asimplevectorsClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="asimplevectorsClient"/> class.
        /// </summary>
        /// <param name="baseUrl">Base URL of the ASimpleVectors API.</param>
        /// <param name="logger">Optional logger for logging API interactions.</param>
        /// <param name="token">Optional bearer token for authentication.</param>
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

        /// <summary>
        /// Releases resources used by the <see cref="asimplevectorsClient"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method to release resources.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose or the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _httpClient?.Dispose();
                }

                _disposed = true;
            }
        }

        ~asimplevectorsClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Sets the authorization token for API requests.
        /// </summary>
        /// <param name="token">The authorization token to set.</param>
        public void SetAuthorizationToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty", nameof(token));
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Sends an HTTP request to the ASimpleVectors API.
        /// </summary>
        /// <typeparam name="T">Expected response type.</typeparam>
        /// <param name="method">HTTP method (GET, POST, PUT, DELETE).</param>
        /// <param name="endpoint">API endpoint to call.</param>
        /// <param name="payload">Optional payload for the request.</param>
        /// <returns>Deserialized response of type <typeparamref name="T"/>.</returns>
        /// <exception cref="HttpRequestException">Thrown when the API response indicates an error.</exception>
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

        /// <summary>
        /// Initializes the cluster.
        /// </summary>
        public async Task InitClusterAsync()
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/cluster/init");
        }

        /// <summary>
        /// Adds a new learner to the cluster.
        /// </summary>
        /// <param name="nodeId">Node ID of the learner.</param>
        /// <param name="apiAddr">API address of the learner.</param>
        /// <param name="rpcAddr">RPC address of the learner.</param>
        public async Task AddLearnerAsync(int nodeId, string apiAddr, string rpcAddr)
        {
            var body = new { NodeId = nodeId, ApiAddr = apiAddr, RpcAddr = rpcAddr };
            await SendRequestAsync<object>(HttpMethod.Post, "/cluster/add-learner", body);
        }

        /// <summary>
        /// Changes the membership of the cluster.
        /// </summary>
        /// <param name="membership">List of node IDs representing the new membership configuration.</param>
        public async Task ChangeMembershipAsync(List<int> membership)
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/cluster/change-membership", membership);
        }

        /// <summary>
        /// Retrieves the metrics of the cluster.
        /// </summary>
        /// <returns>Cluster metrics in the form of <see cref="ClusterMetricsResponse"/>.</returns>
        public async Task<ClusterMetricsResponse> GetClusterMetricsAsync()
        {
            // Fetch the raw response as a dictionary
            var response = await SendRequestAsync<Dictionary<string, object>>(HttpMethod.Get, "/cluster/metrics");

            // Access the nested "Ok" key
            var okSection = JsonSerializer.Serialize(response["Ok"]);
            var metrics = JsonSerializer.Deserialize<ClusterMetricsResponse>(okSection, _jsonOptions);

            return metrics;
        }

        /// <summary>
        /// Creates a new space.
        /// </summary>
        /// <param name="spaceRequest">Configuration for the new space.</param>
        public async Task CreateSpaceAsync(SpaceRequest spaceRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/api/space", spaceRequest);
        }

        /// <summary>
        /// Retrieves details of a specific space.
        /// </summary>
        /// <param name="spaceName">Name of the space.</param>
        /// <returns>Details of the space in the form of <see cref="SpaceResponse"/>.</returns>
        public async Task<SpaceResponse> GetSpaceAsync(string spaceName)
        {
            return await SendRequestAsync<SpaceResponse>(HttpMethod.Get, $"/api/space/{spaceName}");
        }

        /// <summary>
        /// Updates the configuration of an existing space.
        /// </summary>
        /// <param name="spaceName">Name of the space to update.</param>
        /// <param name="updateRequest">New configuration for the space.</param>
        public async Task UpdateSpaceAsync(string spaceName, SpaceRequest updateRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}", updateRequest);
        }

        /// <summary>
        /// Deletes a space.
        /// </summary>
        /// <param name="spaceName">Name of the space to delete.</param>
        public async Task DeleteSpaceAsync(string spaceName)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/space/{spaceName}");
        }

        /// <summary>
        /// Lists all available spaces.
        /// </summary>
        /// <returns>A list of all spaces in the form of <see cref="ListSpacesResponse"/>.</returns>
        public async Task<ListSpacesResponse> ListSpacesAsync()
        {
            return await SendRequestAsync<ListSpacesResponse>(HttpMethod.Get, "/api/spaces");
        }

        /// <summary>
        /// Creates a new version for a specified space.
        /// </summary>
        /// <param name="spaceName">The name of the space to create the version in.</param>
        /// <param name="versionRequest">The configuration details for the new version.</param>
        public async Task CreateVersionAsync(string spaceName, VersionRequest versionRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}/version", versionRequest);
        }

        /// <summary>
        /// Retrieves a list of all versions for a specified space.
        /// </summary>
        /// <param name="spaceName">The name of the space whose versions are to be listed.</param>
        /// <returns>A list of versions in the form of <see cref="ListVersionsResponse"/>.</returns>
        public async Task<ListVersionsResponse> ListVersionsAsync(string spaceName)
        {
            return await SendRequestAsync<ListVersionsResponse>(HttpMethod.Get, $"/api/space/{spaceName}/versions");
        }

        /// <summary>
        /// Retrieves details of a specific version by its ID.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <param name="versionId">The ID of the version to retrieve.</param>
        /// <returns>The details of the version as a <see cref="VersionResponse"/>.</returns>
        public async Task<VersionResponse> GetVersionByIdAsync(string spaceName, int versionId)
        {
            return await SendRequestAsync<VersionResponse>(HttpMethod.Get, $"/api/space/{spaceName}/version/{versionId}");
        }

        /// <summary>
        /// Retrieves the default version of a specified space.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <returns>The details of the default version as a <see cref="VersionResponse"/>.</returns>
        public async Task<VersionResponse> GetDefaultVersionAsync(string spaceName)
        {
            return await SendRequestAsync<VersionResponse>(HttpMethod.Get, $"/api/space/{spaceName}/version");
        }

        /// <summary>
        /// Upsert vectors to a specified space.
        /// </summary>
        /// <param name="spaceName">The name of the space to add vectors to.</param>
        /// <param name="vectorRequest">The configuration details of the vectors to add.</param>
        public async Task UpsertVectorAsync(string spaceName, VectorRequest vectorRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}/vector", vectorRequest);
        }

        /// <summary>
        /// Retrieves vectors from a specified version of a space.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <param name="versionId">The version ID to retrieve vectors from. Use 0 for the default version.</param>
        /// <param name="start">Optional start index for pagination.</param>
        /// <param name="limit">Optional limit on the number of results.</param>
        /// <returns>A list of vectors in the form of <see cref="GetVectorsResponse"/>.</returns>
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

        /// <summary>
        /// Searches vectors within a specified space.
        /// </summary>
        /// <param name="spaceName">The name of the space to search in.</param>
        /// <param name="searchRequest">The search query and parameters.</param>
        /// <returns>A list of search results as <see cref="SearchResult"/>.</returns>
        public async Task<List<SearchResult>> SearchVectorAsync(string spaceName, SearchRequest searchRequest)
        {
            var searchResults = await SendRequestAsync<List<SearchResult>>(
                HttpMethod.Post,
                $"/api/space/{spaceName}/search",
                searchRequest
            );

            return searchResults;
        }

        /// <summary>
        /// Searches vectors within a specified version of a space.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <param name="versionId">The version ID to search in.</param>
        /// <param name="searchRequest">The search query and parameters.</param>
        /// <returns>A list of search results as <see cref="SearchResult"/>.</returns>
        public async Task<List<SearchResult>> SearchVectorByVersionAsync(string spaceName, int versionId, SearchRequest searchRequest)
        {
            var searchResults = await SendRequestAsync<List<SearchResult>>(
                HttpMethod.Post,
                $"/api/space/{spaceName}/version/{versionId}/search",
                searchRequest
            );

            return searchResults;
        }

        /// <summary>
        /// Performs reranking of search results using BM25 for a given space.
        /// </summary>
        /// <param name="spaceName">The name of the space to perform reranking in.</param>
        /// <param name="rerankRequest">The rerank request containing vector and token details.</param>
        /// <returns>A list of rerank results.</returns>
        public async Task<List<RerankResult>> RerankAsync(string spaceName, RerankRequest rerankRequest)
        {
            if (rerankRequest == null)
            {
                throw new ArgumentNullException(nameof(rerankRequest), "Rerank request cannot be null.");
            }

            return await SendRequestAsync<List<RerankResult>>(
                HttpMethod.Post,
                $"/api/space/{spaceName}/rerank",
                rerankRequest
            );
        }

        /// <summary>
        /// Performs reranking of search results using BM25 for a specific version of a space.
        /// </summary>
        /// <param name="spaceName">The name of the space to perform reranking in.</param>
        /// <param name="versionId">The version ID to rerank in.</param>
        /// <param name="rerankRequest">The rerank request containing vector and token details.</param>
        /// <returns>A list of rerank results.</returns>
        public async Task<List<RerankResult>> RerankByVersionAsync(string spaceName, int versionId, RerankRequest rerankRequest)
        {
            if (rerankRequest == null)
            {
                throw new ArgumentNullException(nameof(rerankRequest), "Rerank request cannot be null.");
            }

            return await SendRequestAsync<List<RerankResult>>(
                HttpMethod.Post,
                $"/api/space/{spaceName}/version/{versionId}/rerank",
                rerankRequest
            );
        }

        /// <summary>
        /// Creates a snapshot of the current state of the system.
        /// </summary>
        /// <param name="snapshotRequest">The configuration for the snapshot.</param>
        public async Task CreateSnapshotAsync(CreateSnapshotRequest snapshotRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Post, "/api/snapshot", snapshotRequest);
        }

        /// <summary>
        /// Lists all available snapshots.
        /// </summary>
        /// <returns>A list of snapshots as <see cref="ListSnapshotsResponse"/>.</returns>
        public async Task<ListSnapshotsResponse> ListSnapshotsAsync()
        {
            return await SendRequestAsync<ListSnapshotsResponse>(HttpMethod.Get, "/api/snapshots");
        }

        /// <summary>
        /// Deletes a specified snapshot.
        /// </summary>
        /// <param name="snapshotDate">The date of the snapshot to delete.</param>
        public async Task DeleteSnapshotAsync(string snapshotDate)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/snapshot/{snapshotDate}/delete");
        }

        /// <summary>
        /// Restores a specified snapshot.
        /// </summary>
        /// <param name="snapshotDate">The date of the snapshot to restore.</param>
        public async Task RestoreSnapshotAsync(string snapshotDate)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/snapshot/{snapshotDate}/restore");
        }

        /// <summary>
        /// Creates a new RBAC token.
        /// </summary>
        /// <param name="rbacRequest">The configuration for the new RBAC token.</param>
        /// <returns>The details of the created token as <see cref="RbacTokenResponse"/>.</returns>
        public async Task<RbacTokenResponse> CreateRbacTokenAsync(RbacTokenRequest rbacRequest)
        {
            return await SendRequestAsync<RbacTokenResponse>(HttpMethod.Post, "/api/security/tokens", rbacRequest);
        }

        /// <summary>
        /// Lists all available RBAC tokens.
        /// </summary>
        /// <returns>A list of tokens as <see cref="ListRbacTokensResponse"/>.</returns>
        public async Task<ListRbacTokensResponse> ListRbacTokensAsync()
        {
            return await SendRequestAsync<ListRbacTokensResponse>(HttpMethod.Get, "/api/security/tokens");
        }

        /// <summary>
        /// Deletes a specified RBAC token.
        /// </summary>
        /// <param name="token">The token to delete.</param>
        public async Task DeleteRbacTokenAsync(string token)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/security/tokens/{token}");
        }

        /// <summary>
        /// Updates a specified RBAC token.
        /// </summary>
        /// <param name="token">The token to update.</param>
        /// <param name="rbacRequest">The updated configuration for the token.</param>
        public async Task UpdateRbacTokenAsync(string token, RbacTokenRequest rbacRequest)
        {
            await SendRequestAsync<object>(HttpMethod.Put, $"/api/security/tokens/{token}", rbacRequest);
        }

        /// <summary>
        /// Adds or updates a key-value pair in a specified space.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <param name="key">The key to add or update.</param>
        /// <param name="keyValue">The value associated with the key.</param>
        public async Task PutKeyValueAsync(string spaceName, string key, KeyValueRequest keyValue)
        {
            await SendRequestAsync<object>(HttpMethod.Post, $"/api/space/{spaceName}/key/{key}", keyValue);
        }

        /// <summary>
        /// Retrieves the value associated with a specified key in a space.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The value as a string.</returns>
        public async Task<string> GetKeyValueAsync(string spaceName, string key)
        {
            var response = await SendRequestAsync<Dictionary<string, string>>(HttpMethod.Get, $"/api/space/{spaceName}/key/{key}");
            return response["text"]; 
        }

        /// <summary>
        /// Lists all keys in a specified space.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <returns>A list of keys as <see cref="ListKeysResponse"/>.</returns>
        public async Task<ListKeysResponse> ListKeysAsync(string spaceName)
        {
            return await SendRequestAsync<ListKeysResponse>(HttpMethod.Get, $"/api/space/{spaceName}/keys");
        }

        /// <summary>
        /// Deletes a specified key-value pair from a space.
        /// </summary>
        /// <param name="spaceName">The name of the space.</param>
        /// <param name="key">The key to delete.</param>
        public async Task DeleteKeyValueAsync(string spaceName, string key)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/api/space/{spaceName}/key/{key}");
        }
    }
}
