namespace asimpleVectors.Tests;

using asimplevectors.Services;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;
using asimplevectors.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

public class asimplevectorsTests : IAsyncLifetime
{
    private readonly asimplevectorsClient _client;
    private readonly ILogger<asimplevectorsClient> _logger;

    public asimplevectorsTests()
    {
        // Set up logger
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        _logger = loggerFactory.CreateLogger<asimplevectorsClient>();

        // Set up client with logger
        _client = new asimplevectorsClient("http://127.0.0.1:21001", _logger);
    }

    public Task InitializeAsync()
    {
        _logger.LogInformation("Initializing test suite.");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _logger.LogInformation("Disposing resources after test suite.");
        return Task.CompletedTask;
    }

    private async Task Log(string testName, int step, string message)
    {
        _logger.LogInformation("[{TestName} - Step {Step}] {Message}", testName, step, message);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TestClusterOperations()
    {
        const string testName = "TestClusterOperations";

        await Log(testName, 1, "Initializing the cluster.");
        await _client.InitClusterAsync();

        await Log(testName, 2, "Fetching cluster metrics.");
        var metrics = await _client.GetClusterMetricsAsync();

        metrics.Should().NotBeNull();
        metrics.State.Should().Be("Leader");
        await Log(testName, 3, "Cluster metrics validated successfully.");
    }

    [Fact]
    public async Task TestSpaceOperations()
    {
        const string testName = "TestSpaceOperations";

        var spaceRequest = new SpaceRequest
        {
            Name = "TestSpaceOperations",
            Dimension = 128,
            Metric = "L2"
        };

        await Log(testName, 1, $"Creating space: {spaceRequest.Name}.");
        await _client.CreateSpaceAsync(spaceRequest);

        await Log(testName, 2, $"Retrieving space: {spaceRequest.Name}.");
        var space = await _client.GetSpaceAsync("TestSpaceOperations");

        space.Should().NotBeNull();
        space.Name.Should().Be("TestSpaceOperations");

        var updatedSpace = new SpaceRequest
        {
            Dimension = 256,
            Metric = "Cosine"
        };
        await Log(testName, 3, $"Updating space: {spaceRequest.Name}.");
        await _client.UpdateSpaceAsync("TestSpaceOperations", updatedSpace);

        await Log(testName, 4, $"Deleting space: {spaceRequest.Name}.");
        await _client.DeleteSpaceAsync("TestSpaceOperations");
    }

    [Fact]
    public async Task TestVersionOperations()
    {
        const string testName = "TestVersionOperations";

        var spaceRequest = new SpaceRequest
        {
            Name = "version_test_space",
            Dimension = 128,
            Metric = "L2"
        };

        await Log(testName, 1, $"Creating space: {spaceRequest.Name}.");
        await _client.CreateSpaceAsync(spaceRequest);

        var versionRequest = new VersionRequest
        {
            Name = "v1",
            Description = "Version for testing",
            IsDefault = true
        };

        await Log(testName, 2, $"Creating version: {versionRequest.Name}.");
        await _client.CreateVersionAsync("version_test_space", versionRequest);

        await Log(testName, 3, "Listing versions.");
        var versions = await _client.ListVersionsAsync("version_test_space");

        versions.Values.Should().NotBeNullOrEmpty();

        await Log(testName, 4, $"Deleting space: {spaceRequest.Name}.");
        await _client.DeleteSpaceAsync("version_test_space");
    }

    [Fact]
    public async Task TestVectorOperations()
    {
        const string testName = "TestVectorOperations";

        var spaceRequest = new SpaceRequest
        {
            Name = "vector_test_space",
            Dimension = 3,
            Metric = "L2"
        };

        var vectorRequest = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData { Id = 1, Data = new float[] { 0.1f, 0.2f, 0.3f }, Metadata = new { Label = "example" } },
                new VectorData { Id = 2, Data = new float[] { 0.4f, 0.5f, 0.6f }, Metadata = new { Label = "another_example" } }
            }
        };

        await Log(testName, 1, $"Creating space: {spaceRequest.Name}.");
        await _client.CreateSpaceAsync(spaceRequest);

        await Log(testName, 2, "Adding vectors.");
        await _client.UpsertVectorAsync("vector_test_space", vectorRequest);

        await Log(testName, 3, "Verifying vector storage.");
        var rawVectors = await _client.GetVectorsByVersionAsync("vector_test_space", 1);

        rawVectors.Vectors.Should().NotBeNullOrEmpty("Vectors should be present after addition.");

        await Log(testName, 4, $"Deleting space: {spaceRequest.Name}.");
        await _client.DeleteSpaceAsync("vector_test_space");
    }

    [Fact]
    public async Task TestSearchOperations()
    {
        const string testName = "TestSearchOperations";

        var spaceRequest = new SpaceRequest
        {
            Name = "search_test_space",
            Dimension = 3,
            Metric = "L2"
        };

        var vectorRequest = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData
                {
                    Id = 1,
                    Data = new float[] { 0.1f, 0.2f, 0.3f },
                    Metadata = new { Label = "example" }
                },
                new VectorData
                {
                    Id = 2,
                    Data = new float[] { 0.4f, 0.5f, 0.6f },
                    Metadata = new { Label = "another_example" }
                }
            }
        };

        var searchRequest = new SearchRequest
        {
            Vector = new float[] { 0.1f, 0.2f, 0.3f },
            TopK = 1
        };

        await Log(testName, 1, $"Creating space: {spaceRequest.Name}.");
        await _client.CreateSpaceAsync(spaceRequest);

        await Log(testName, 2, "Adding vectors.");
        await _client.UpsertVectorAsync("search_test_space", vectorRequest);

        await Log(testName, 3, "Performing vector search.");
        var results = await _client.SearchVectorAsync("search_test_space", searchRequest);

        results.Should().NotBeNullOrEmpty();
        results.First().Distance.Should().BeApproximately(0.0f, 0.001f);

        await Log(testName, 4, $"Deleting space: {spaceRequest.Name}.");
        await _client.DeleteSpaceAsync("search_test_space");
    }

    [Fact]
    public async Task TestRerankOperationsWithDocAndTokens()
    {
        const string testName = "TestRerankOperationsWithDocAndTokens";

        var spaceRequest = new SpaceRequest
        {
            Name = "rerank_doc_test_space",
            Dimension = 3,
            Metric = "L2"
        };

        var vectorRequest = new VectorRequest
        {
            Vectors = new[]
            {
            new VectorData
            {
                Id = 1,
                Data = new float[] { 0.1f, 0.2f, 0.3f },
                Metadata = new { Label = "test_vector_1" },
                Doc = "This is a test document about vectors.",
                DocTokens = new List<string> { "test", "document", "vectors" }
            },
            new VectorData
            {
                Id = 2,
                Data = new float[] { 0.4f, 0.5f, 0.6f },
                Metadata = new { Label = "test_vector_2" },
                Doc = "Another document with different content.",
                DocTokens = new List<string> { "another", "document", "content" }
            }
        }
        };

        var rerankRequest = new RerankRequest
        {
            Vector = new float[] { 0.1f, 0.2f, 0.3f },
            Tokens = new List<string> { "test", "vectors" },
            TopK = 2
        };

        await Log(testName, 1, $"Creating space: {spaceRequest.Name}.");
        await _client.CreateSpaceAsync(spaceRequest);

        await Log(testName, 2, "Upserting vectors with documents and tokens.");
        await _client.UpsertVectorAsync("rerank_doc_test_space", vectorRequest);

        await Log(testName, 3, "Performing rerank operation.");
        var results = await _client.RerankAsync("rerank_doc_test_space", rerankRequest);

        results.Should().NotBeNullOrEmpty("Rerank results should not be empty.");
        results.Count.Should().Be(rerankRequest.TopK, $"Expected top {rerankRequest.TopK} results.");

        foreach (var result in results)
        {
            _logger.LogInformation(
                "Vector ID: {Label}, Distance: {Distance}, BM25 Score: {BM25Score}",
                result.Label,
                result.Distance,
                result.BM25Score
            );
        }

        await Log(testName, 4, $"Deleting space: {spaceRequest.Name}.");
        await _client.DeleteSpaceAsync("rerank_doc_test_space");
    }

    [Fact]
    public async Task TestKeyValueOperations()
    {
        const string testName = "TestKeyValueOperations";

        var spaceRequest = new SpaceRequest
        {
            Name = "kv_test_space",
            Dimension = 3,
            Metric = "L2"
        };
        var key = "example_key";
        var value = new KeyValueRequest
        {
            Text = "example_value"
        };

        await Log(testName, 1, $"Creating space: {spaceRequest.Name}.");
        await _client.CreateSpaceAsync(spaceRequest);

        await Log(testName, 2, $"Storing key-value pair: {key}.");
        await _client.PutKeyValueAsync("kv_test_space", key, value);

        await Log(testName, 3, $"Retrieving key-value pair: {key}.");
        var retrievedValue = await _client.GetKeyValueAsync("kv_test_space", key);
        retrievedValue.Should().Be("example_value");

        await Log(testName, 4, $"Deleting key-value pair: {key}.");
        await _client.DeleteKeyValueAsync("kv_test_space", key);

        await Log(testName, 5, $"Deleting space: {spaceRequest.Name}.");
        await _client.DeleteSpaceAsync("kv_test_space");
    }

    [Fact]
    public async Task TestSnapshot()
    {
        // Arrange
        string spaceName = "TestSnapshot";

        var spaceRequest = new SpaceRequest
        {
            Name = spaceName,
            Description = "Test space"
        };
        await _client.CreateSpaceAsync(spaceRequest);

        await Log("TestSnapshot", 1, $"Create Snapshot");
        var snapshotRequest = new CreateSnapshotRequest
        {
        };
        await _client.CreateSnapshotAsync(snapshotRequest);

        var snapshotsResponse = await _client.ListSnapshotsAsync();
        await Log("TestSnapshot", 2, $"Snapshot Length: {snapshotsResponse.Snapshots.Length}");
        var latestSnapshot = snapshotsResponse.Snapshots.OrderByDescending(s => s.Date).FirstOrDefault();
        await Log("TestSnapshot", 3, $"Last Snapshot: {latestSnapshot}");
        string snapshotDate = latestSnapshot.Date;
        await Log("TestSnapshot", 4, $"Snapshot Date: {snapshotDate}");

        Assert.NotNull(snapshotDate);

        await Log("TestSnapshot", 5, $"Download Snapshot: {snapshotDate}");
        Stream result = await _client.DownloadSnapshotAsync(snapshotDate);

        // Copy the result stream to a MemoryStream to support seeking
        var memoryStream = new MemoryStream();
        await result.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Assert
        Assert.NotNull(memoryStream);

        // Upload snapshot test
        await Log("TestSnapshot", 6, $"Upload Snapshot: {snapshotDate}");
        var tempFilePath = Path.GetTempFileName();
        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(fileStream);
        }
        var fileInfo = new FileInfo(tempFilePath);
        await _client.UploadSnapshotAsync(fileInfo);

        // Verify if the space exists after uploading the snapshot
        await Log("TestSnapshot", 7, $"Retrieving space: {spaceName}.");
        var space = await _client.GetSpaceAsync(spaceName);

        space.Should().NotBeNull();
        space.Name.Should().Be(spaceName);

        // Cleanup
        fileInfo.Delete();
        await _client.DeleteSpaceAsync(spaceName);
    }
}