namespace asimplevectors.Example;

using asimplevectors.Models;
using asimplevectors.Services;

class Program
{
    private static async Task TestClusterOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Initializing the cluster...");
        await client.InitClusterAsync();

        Console.WriteLine("Fetching cluster metrics...");
        var metrics = await client.GetClusterMetricsAsync();
        Console.WriteLine("Metrics:\n" + metrics);

        Console.WriteLine("Adding node 2 as a learner...");
        await client.AddLearnerAsync(2, "127.0.0.1:21002", "127.0.0.1:22002");

        Console.WriteLine("Adding node 3 as a learner...");
        await client.AddLearnerAsync(3, "127.0.0.1:21003", "127.0.0.1:22003");

        Console.WriteLine("Changing cluster membership...");
        await client.ChangeMembershipAsync(new List<int> { 1, 2, 3 });
        Console.WriteLine("Membership changed successfully.");

        Console.WriteLine("Fetching updated cluster metrics...");
        var updatedMetrics = await client.GetClusterMetricsAsync();
        Console.WriteLine("Updated Metrics:\n" + updatedMetrics);
    }

    private static async Task TestKeyValueOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Creating key-value pair in 'spacename'");
        var keyName = "test_key";
        var keyValueData = new KeyValueRequest { Text = "Hello, ASimpleVectors!" };
        await client.PutKeyValueAsync("spacename", keyName, keyValueData);
        Console.WriteLine($"Key '{keyName}' created successfully.");

        Console.WriteLine($"Retrieving key-value pair for key '{keyName}'");
        var keyValue = await client.GetKeyValueAsync("spacename", keyName);
        Console.WriteLine($"Retrieved key-value pair: {keyValue}");

        Console.WriteLine("Listing all keys in 'spacename'");
        var keysResponse = await client.ListKeysAsync("spacename");
        Console.WriteLine("Keys: " + string.Join(", ", keysResponse.Keys));

        Console.WriteLine($"Deleting key '{keyName}' from 'spacename'");
        await client.DeleteKeyValueAsync("spacename", keyName);
        Console.WriteLine($"Key '{keyName}' deleted successfully.");
    }

    private static async Task TestSearchOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Creating space 'spacename'");
        var createSpaceData = new SpaceRequest
        {
            Name = "spacename",
            Dimension = 4,
            Metric = "L2",
            HnswConfig = new HnswConfig { M = 64, EfConstruct = 500 }
        };
        await client.CreateSpaceAsync(createSpaceData);
        Console.WriteLine("Space 'spacename' created successfully.");

        Console.WriteLine("Upserting vectors into 'spacename'");
        var vectorData = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData { Id = 1, Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f }, Metadata = new Dictionary<string, object> { { "meta", "first" } } },
                new VectorData { Id = 2, Data = new float[] { 0.5f, 0.6f, 0.7f, 0.8f }, Metadata = new Dictionary<string, object> { { "meta", "second" } } }
            }
        };
        await client.UpsertVectorAsync("spacename", vectorData);
        Console.WriteLine("Vectors upserted successfully.");

        Console.WriteLine("Searching vectors in 'spacename'");
        var searchRequest = new SearchRequest { Vector = new float[] { 0.2f, 0.3f, 0.4f, 0.3f } };
        var searchResponse = await client.SearchVectorAsync("spacename", searchRequest);
        Console.WriteLine("Search Response:\n" + string.Join("\n", searchResponse));
    }

    private static async Task TestRerankOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Creating space 'spacename' for rerank test");
        var createSpaceData = new SpaceRequest
        {
            Name = "spacename",
            Dimension = 4,
            Metric = "L2",
            HnswConfig = new HnswConfig { M = 64, EfConstruct = 500 }
        };
        await client.CreateSpaceAsync(createSpaceData);
        Console.WriteLine("Space 'spacename' created successfully.");

        Console.WriteLine("Upserting vectors with doc and doc_tokens");
        var vectorData = new VectorRequest
        {
            Vectors = new[]
            {
            new VectorData
            {
                Id = 1,
                Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f },
                Metadata = new Dictionary<string, object> { { "meta", "first" } },
                Doc = "This is the first document",
                DocTokens = new List<string> { "this", "is", "the", "first", "document" }
            },
            new VectorData
            {
                Id = 2,
                Data = new float[] { 0.5f, 0.6f, 0.7f, 0.8f },
                Metadata = new Dictionary<string, object> { { "meta", "second" } },
                Doc = "This is the second document",
                DocTokens = new List<string> { "this", "is", "the", "second", "document" }
            }
        }
        };
        await client.UpsertVectorAsync("spacename", vectorData);
        Console.WriteLine("Vectors upserted successfully.");

        Console.WriteLine("Performing rerank operation");
        var rerankRequest = new RerankRequest
        {
            Vector = new float[] { 0.2f, 0.3f, 0.4f, 0.5f },
            Tokens = new List<string> { "document", "first" }
        };

        var rerankResults = await client.RerankAsync("spacename", rerankRequest);

        Console.WriteLine("Rerank Results:");
        foreach (var result in rerankResults)
        {
            Console.WriteLine($"Vector ID: {result.VectorUniqueId}, Distance: {result.Distance}, BM25 Score: {result.BM25Score}");
        }

        Console.WriteLine("Deleting space 'spacename' after rerank test");
        await client.DeleteSpaceAsync("spacename");
        Console.WriteLine("Space 'spacename' deleted successfully.");
    }

    private static async Task TestSecurityOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Attempting to create space 'spacename' without permission.");
        var createSpaceData = new SpaceRequest
        {
            Name = "spacename",
            Dimension = 4,
            Metric = "L2",
            HnswConfig = new HnswConfig { M = 16, EfConstruct = 100 }
        };

        try
        {
            await client.CreateSpaceAsync(createSpaceData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Expected failure to create space without token: {ex.Message}");
        }

        Console.WriteLine("Creating a new RBAC token.");
        var rbacTokenData = new RbacTokenRequest
        {
            System = 2,
            Space = 2,
            Version = 2,
            Vector = 2,
            Snapshot = 2
        };

        try
        {
            var tokenResponse = await client.CreateRbacTokenAsync(rbacTokenData);
            Console.WriteLine("RBAC token created successfully.");
            client.SetAuthorizationToken(tokenResponse.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create RBAC token: {ex.Message}");
            return;
        }

        Console.WriteLine("Creating space 'spacename' with valid token.");
        try
        {
            await client.CreateSpaceAsync(createSpaceData);
            Console.WriteLine("Space 'spacename' created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create space with token: {ex.Message}");
        }

        Console.WriteLine("Upserting vectors into 'spacename'.");
        var vectorData = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData { Id = 1, Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f }, Metadata = new Dictionary<string, object> { { "label", "first" } } },
                new VectorData { Id = 2, Data = new float[] { 0.5f, 0.6f, 0.7f, 0.8f }, Metadata = new Dictionary<string, object> { { "label", "second" } } }
            }
        };

        try
        {
            await client.UpsertVectorAsync("spacename", vectorData);
            Console.WriteLine("Vectors upserted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to upsert vectors: {ex.Message}");
        }

        Console.WriteLine("Searching vectors in 'spacename'.");
        var searchRequest = new SearchRequest { Vector = new float[] { 0.2f, 0.3f, 0.4f, 0.3f } };
        try
        {
            var searchResults = await client.SearchVectorByVersionAsync("spacename", 1, searchRequest);
            if (searchResults.Count > 0)
            {
                Console.WriteLine("Search results:\n" + string.Join("\n", searchResults));
            }
            else
            {
                Console.WriteLine("No search results found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to search vectors: {ex.Message}");
        }
    }
    private static async Task TestSnapshotOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Creating snapshot for 'spacename'");
        var snapshotRequest = new CreateSnapshotRequest { };
        await client.CreateSnapshotAsync(snapshotRequest);
        Console.WriteLine("Snapshot created successfully.");

        Console.WriteLine("Listing snapshots");
        var snapshots = await client.ListSnapshotsAsync();

        if (snapshots?.Snapshots == null || snapshots.Snapshots.Length == 0)
        {
            Console.WriteLine("No snapshots available.");
            return;
        }

        Console.WriteLine("Snapshots:");
        foreach (var snapshot in snapshots.Snapshots)
        {
            Console.WriteLine($"- {snapshot.FileName ?? "No FileName"}");
        }

        // Safely access the first snapshot
        var firstSnapshot = snapshots.Snapshots[0];

        if (string.IsNullOrEmpty(firstSnapshot.FileName))
        {
            Console.WriteLine("The first snapshot does not have a valid FileName.");
            return;
        }

        string[] fileNameParts = firstSnapshot.FileName.Split('-');
        if (fileNameParts.Length < 2 || string.IsNullOrEmpty(fileNameParts[1]))
        {
            Console.WriteLine("The snapshot FileName does not match the expected format.");
            return;
        }

        var snapshotDate = fileNameParts[1].Split('.')[0];
        Console.WriteLine($"Snapshot Date: {snapshotDate}");

        Console.WriteLine("Restoring snapshot");
        await client.RestoreSnapshotAsync(snapshotDate);
        Console.WriteLine("Snapshot restored successfully.");
    }

    private static async Task TestSpaceOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Creating space 'spacename'");
        var createSpaceData = new SpaceRequest
        {
            Name = "spacename",
            Dimension = 128,
            Metric = "cosine",
            HnswConfig = new HnswConfig { EfConstruct = 123 },
            QuantizationConfig = new QuantizationConfig
            {
                Scalar = new ScalarQuantizationConfig { Type = "int8" }
            },
            Sparse = new SparseConfig { Metric = "Cosine" },
            Indexes = new Dictionary<string, CustomIndexConfig>
            {
                {
                    "my_index",
                    new CustomIndexConfig
                    {
                        Dimension = 1536,
                        Metric = "Cosine",
                        HnswConfig = new HnswConfig { M = 32, EfConstruct = 123 },
                        QuantizationConfig = new QuantizationConfig
                        {
                            Scalar = new ScalarQuantizationConfig { Type = "int8" }
                        }
                    }
                }
            }
        };

        try
        {
            await client.CreateSpaceAsync(createSpaceData);
            Console.WriteLine("Space 'spacename' created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create space: {ex.Message}");
        }

        Console.WriteLine("Retrieving space 'spacename'");
        var spaceDetails = await client.GetSpaceAsync("spacename");
        Console.WriteLine($"Space details:\n{spaceDetails}");

        Console.WriteLine("Listing all spaces");
        var spaces = await client.ListSpacesAsync();
        Console.WriteLine($"Spaces: {spaces.Values}");

        Console.WriteLine("Updating space 'spacename'");
        var updateSpaceData = new SpaceRequest
        {
            Dense = new DenseConfig
            {
                Dimension = 1234,
                Metric = "l2",
                HnswConfig = new HnswConfig { M = 64, EfConstruct = 55 }
            }
        };
        await client.UpdateSpaceAsync("spacename", updateSpaceData);
        Console.WriteLine("Space 'spacename' updated successfully.");

        Console.WriteLine("Retrieving updated space 'spacename'");
        var updatedSpaceDetails = await client.GetSpaceAsync("spacename");
        Console.WriteLine($"Updated space details:\n{updatedSpaceDetails}");
    }

    private static async Task TestVectorOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Creating space 'spacename'");
        var createSpaceData = new SpaceRequest
        {
            Name = "spacename",
            Dimension = 4,
            Metric = "L2",
            HnswConfig = new HnswConfig { M = 16, EfConstruct = 100 }
        };

        try
        {
            await client.CreateSpaceAsync(createSpaceData);
            Console.WriteLine("Space 'spacename' created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create space: {ex.Message}");
        }

        Console.WriteLine("Upserting vectors to 'spacename' without specifying version");
        var upsertData1 = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData { Id = 1, Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f }, Metadata = new Dictionary<string, object> { { "label", "first" } } },
                new VectorData { Id = 2, Data = new float[] { 0.5f, 0.6f, 0.7f, 0.8f }, Metadata = new Dictionary<string, object> { { "label", "second" } } }
            }
        };
        await client.UpsertVectorAsync("spacename", upsertData1);
        Console.WriteLine("Vectors upserted successfully.");

        Console.WriteLine("Retrieving vectors for default version");
        var vectors = await client.GetVectorsByVersionAsync("spacename", versionId: 0, start: 0, limit: 10);
        Console.WriteLine($"Retrieved vectors:\n{string.Join("\n", vectors.Vectors)}");

        Console.WriteLine("Upserting vectors to 'spacename' with specific version ID");
        var upsertData2 = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData { Id = 3, Data = new float[] { 0.9f, 0.8f, 0.7f, 0.6f }, Metadata = new Dictionary<string, object> { { "label", "third" } } }
            }
        };
        await client.UpsertVectorAsync("spacename", upsertData2);
        Console.WriteLine("Vectors upserted successfully.");

        Console.WriteLine("Retrieving vectors for version ID 1");
        var version1Vectors = await client.GetVectorsByVersionAsync("spacename", versionId: 1, start: 0, limit: 10);
        Console.WriteLine($"Retrieved vectors for version 1:\n{string.Join("\n", version1Vectors.Vectors)}");
    }

    private static async Task TestVersionOperations(asimplevectorsClient client)
    {
        Console.WriteLine("Creating space 'spacename'");
        var createSpaceData = new SpaceRequest
        {
            Name = "spacename",
            Dimension = 4,
            Metric = "L2",
            HnswConfig = new HnswConfig { M = 16, EfConstruct = 100 }
        };

        try
        {
            await client.CreateSpaceAsync(createSpaceData);
            Console.WriteLine("Space 'spacename' created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create space: {ex.Message}");
        }

        Console.WriteLine("Upserting vectors to 'spacename' without specifying version");
        var upsertData = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData { Id = 1, Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f }, Metadata = new Dictionary<string, object> { { "label", "first" } } },
                new VectorData { Id = 2, Data = new float[] { 0.5f, 0.6f, 0.7f, 0.8f }, Metadata = new Dictionary<string, object> { { "label", "second" } } }
            }
        };
        await client.UpsertVectorAsync("spacename", upsertData);
        Console.WriteLine("Vectors upserted successfully.");

        Console.WriteLine("Retrieving vectors for default version");
        var vectors = await client.GetVectorsByVersionAsync("spacename", versionId: 0, start: 0, limit: 10);
        Console.WriteLine($"Retrieved vectors:\n{string.Join("\n", vectors.Vectors)}");

        Console.WriteLine("Upserting vectors to 'spacename' with specific version ID");
        var upsertDataVersion = new VectorRequest
        {
            Vectors = new[]
            {
                new VectorData { Id = 3, Data = new float[] { 0.9f, 0.8f, 0.7f, 0.6f }, Metadata = new Dictionary<string, object> { { "label", "third" } } }
            }
        };
        await client.UpsertVectorAsync("spacename", upsertDataVersion);
        Console.WriteLine("Vectors upserted to version successfully.");

        Console.WriteLine("Retrieving vectors for version ID 1");
        var versionVectors = await client.GetVectorsByVersionAsync("spacename", versionId: 1, start: 0, limit: 10);
        Console.WriteLine($"Retrieved vectors for version 1:\n{string.Join("\n", versionVectors.Vectors)}");
    }

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide the example name to run: cluster, keys, search, snapshot, space, vector, version.");
            return;
        }

        var exampleName = args[0];
        var client = new asimplevectorsClient("http://localhost:21001");

        try
        {
            switch (exampleName.ToLower())
            {
                case "cluster":
                    await TestClusterOperations(client);
                    break;
                case "keys":
                    await TestKeyValueOperations(client);
                    break;
                case "search":
                    await TestSearchOperations(client);
                    break;
                case "rerank":
                    await TestRerankOperations(client);
                    break;
                case "security":
                    await TestSecurityOperations(client);
                    break;
                case "snapshot":
                    await TestSnapshotOperations(client);
                    break;
                case "space":
                    await TestSpaceOperations(client);
                    break;
                case "vector":
                    await TestVectorOperations(client);
                    break;
                case "version":
                    await TestVersionOperations(client);
                    break;
                default:
                    Console.WriteLine("Invalid example name. Choose from: cluster, keys, search, snapshot, space, vector, version.");
                    break;
            }
        }
        finally
        {
            client.Dispose();
        }
    }
}
