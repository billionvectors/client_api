import { ASimpleVectorsClient } from "../asimplevectors/src";

async function testSearch(client: ASimpleVectorsClient): Promise<void> {
  // Step 1: Create space 'spacename' on leader
  console.log("Creating space 'spacename' on leader");
  const createSpaceData = {
    name: "spacename",
    dimension: 4,
    metric: "L2",
    hnsw_config: {
      M: 64,
      ef_construct: 500,
    },
  };
  await client.createSpace(createSpaceData);
  console.log("Space 'spacename' created successfully.");

  // Step 2: Upsert vectors to 'spacename' without specifying version
  console.log("Upserting vectors to 'spacename' without specifying version");
  const upsertVectorsData = {
    vectors: [
      {
        id: 1,
        data: [0.1, 0.2, 0.3, 0.4],
        metadata: { meta: "first" },
      },
      {
        id: 2,
        data: [0.5, 0.6, 0.7, 0.8],
        metadata: { meta: "second" },
      },
      {
        id: 3,
        data: [0.9, 0.8, 0.7, 0.6],
        metadata: { meta: "third" },
      },
      {
        id: 4,
        data: [1.0, 0.1, 0.2, 0.3],
        metadata: { meta: "forth" },
      },
      {
        id: 5,
        data: [0.2, 0.3, 0.4, 0.3],
        metadata: { meta: "fifth" },
      },
    ],
  };
  await client.createVector("spacename", upsertVectorsData);
  console.log("Vectors upserted successfully.");

  // Step 3: Search vectors with a specific version ID
  console.log("Searching vectors with specific version id");
  const searchData = {
    vector: [0.2, 0.3, 0.4, 0.3],
  };
  let response = await client.searchVector("spacename", searchData);
  if (response) {
    console.log("Search response:\n", response);
  }

  // Step 4: Filtered search with metadata
  console.log("Filter search with metadata");
  const searchDataWithFilter = {
    vector: [0.2, 0.3, 0.4, 0.3],
    filter: "meta == 'first' OR meta == 'second'",
  };
  response = await client.searchVector("spacename", searchDataWithFilter);
  if (response) {
    console.log("Filtered search response:\n", response);
  }
}

async function main(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);
  try {
    // Run the search test
    await testSearch(client);
  } finally {
    console.log("Closing the client.");
    // Add any necessary cleanup logic here
  }
}

// Run the example
main().catch((error) => {
  console.error("An error occurred:", error);
});
