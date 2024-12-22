import ASimpleVectorsClient from "../src/client";

describe("ASimpleVectorsClient Regression Tests", () => {
  let client: ASimpleVectorsClient;

  beforeAll(() => {
    client = new ASimpleVectorsClient("localhost", 21001);
  });

  afterAll(async () => {
  });

  const log = (testName: string, step: number, message: string) => {
    console.log(`[${testName} - Step ${step}] ${message}`);
  };

  test("Cluster Operations", async () => {
    const testName = "Cluster Operations";

    log(testName, 1, "Initializing the cluster.");
    await client.initCluster();

    log(testName, 2, "Fetching cluster metrics.");
    const metrics = await client.getClusterMetrics();
    expect(metrics).toBeDefined();
    expect(metrics.state).toBe("Leader");

    log(testName, 3, "Cluster metrics validated successfully.");
  });

  test("Space Operations", async () => {
    const testName = "Space Operations";
    const spaceRequest = { name: "test_space", dimension: 128, metric: "L2" };

    log(testName, 1, `Creating space: ${spaceRequest.name}.`);
    await client.createSpace(spaceRequest);

    log(testName, 2, `Retrieving space: ${spaceRequest.name}.`);
    const space = await client.getSpace("test_space");
    expect(space).toBeDefined();
    expect(space.name).toBe("test_space");

    const updatedSpaceData = { dimension: 256, metric: "Cosine" };
    log(testName, 3, `Updating space: ${spaceRequest.name}.`);
    await client.updateSpace("test_space", updatedSpaceData);

    log(testName, 4, `Deleting space: ${spaceRequest.name}.`);
    await client.deleteSpace("test_space");
  });

  test("Version Operations", async () => {
    const testName = "Version Operations";
    const spaceRequest = { name: "version_test_space", dimension: 128, metric: "L2" };

    log(testName, 1, `Creating space: ${spaceRequest.name}.`);
    await client.createSpace(spaceRequest);

    const versionRequest = { name: "v1", description: "Version for testing", is_default: true };
    log(testName, 2, `Creating version: ${versionRequest.name}.`);
    await client.createVersion("version_test_space", versionRequest);

    log(testName, 3, "Listing versions.");
    const versions = await client.listVersions("version_test_space");
    expect(versions).toBeDefined();
    expect(versions.values.length).toBeGreaterThan(0);

    log(testName, 4, `Deleting space: ${spaceRequest.name}.`);
    await client.deleteSpace("version_test_space");
  });

  test("Vector Operations", async () => {
    const testName = "Vector Operations";
    const spaceRequest = { name: "vector_test_space", dimension: 3, metric: "L2" };
    const vectorRequest = {
      vectors: [
        { id: 1, data: [0.1, 0.2, 0.3], metadata: { label: "example" } },
        { id: 2, data: [0.4, 0.5, 0.6], metadata: { label: "another_example" } },
      ],
    };

    log(testName, 1, `Creating space: ${spaceRequest.name}.`);
    await client.createSpace(spaceRequest);

    log(testName, 2, "Adding vectors.");
    await client.upsertVector("vector_test_space", vectorRequest);

    log(testName, 3, "Retrieving vectors by version.");
    const vectors = await client.getVectorsByVersion("vector_test_space", 1);
    expect(vectors).toBeDefined();
    expect(vectors.vectors.length).toBeGreaterThan(0);

    log(testName, 4, `Deleting space: ${spaceRequest.name}.`);
    await client.deleteSpace("vector_test_space");
  });

  test("Search Operations", async () => {
    const testName = "Search Operations";
    const spaceRequest = { name: "search_test_space", dimension: 3, metric: "L2" };
    const vectorRequest = {
      vectors: [
        { id: 1, data: [0.1, 0.2, 0.3], metadata: { label: "example" } },
        { id: 2, data: [0.4, 0.5, 0.6], metadata: { label: "another_example" } },
      ],
    };
    const searchRequest = { vector: [0.1, 0.2, 0.3], top_k: 1 };

    log(testName, 1, `Creating space: ${spaceRequest.name}.`);
    await client.createSpace(spaceRequest);

    log(testName, 2, "Adding vectors.");
    await client.upsertVector("search_test_space", vectorRequest);

    log(testName, 3, "Performing vector search.");
    const results = await client.searchVector("search_test_space", searchRequest);
    expect(results).toBeDefined();

    log(testName, 4, `Deleting space: ${spaceRequest.name}.`);
    await client.deleteSpace("search_test_space");
  });

  test("Key-Value Operations", async () => {
    const testName = "Key-Value Operations";
    const spaceRequest = { name: "kv_test_space", dimension: 3, metric: "L2" };
    const key = "example_key";
    const value = "example_value";
  
    log(testName, 1, `Creating space: ${spaceRequest.name}.`);
    await client.createSpace(spaceRequest);
  
    log(testName, 2, `Storing key-value pair: ${key}.`);
    await client.putKeyValue("kv_test_space", key, value);
  
    log(testName, 3, `Retrieving key-value pair: ${key}.`);
    const retrievedValue = await client.getKeyValue("kv_test_space", key);
    expect(retrievedValue).toBe(value);
  
    log(testName, 4, `Deleting key-value pair: ${key}.`);
    await client.deleteKeyValue("kv_test_space", key);
  
    log(testName, 5, `Deleting space: ${spaceRequest.name}.`);
    await client.deleteSpace("kv_test_space");
  });  
});
