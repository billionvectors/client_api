import { ASimpleVectorsClient } from "../asimplevectors/src";

async function testVersion(client: ASimpleVectorsClient): Promise<void> {
  // Step 1: Create a space
  console.log("Creating space 'spacename'");
  const createSpaceData = {
    name: "spacename",
    dimension: 4,
    metric: "L2",
    hnsw_config: {
      M: 16,
      ef_construct: 100,
    },
  };

  try {
    await client.createSpace(createSpaceData);
    console.log("Space 'spacename' created successfully.");
  } catch (error) {
    if (error.response?.status === 409) {
      console.log("Space 'spacename' already exists.");
    } else {
      console.error("Failed to create space:", error);
      return;
    }
  }

  // Step 2: Upsert vectors without specifying version
  console.log("Upserting vectors to 'spacename' without specifying version");
  const upsertData1 = {
    vectors: [
      {
        id: 1,
        data: [0.1, 0.2, 0.3, 0.4],
        metadata: { label: "first" },
      },
      {
        id: 2,
        data: [0.5, 0.6, 0.7, 0.8],
        metadata: { label: "second" },
      },
    ],
  };
  try {
    await client.createVector("spacename", upsertData1);
    console.log("Vectors upserted successfully.");
  } catch (error) {
    console.error("Failed to upsert vectors:", error);
    return;
  }

  // Step 3: Get vectors by default version
  console.log("Retrieving vectors for default version");
  try {
    const response = await client.getVectorsByVersion("spacename", 0, 0, 10);
    if (response) {
      console.log("Retrieved vectors:\n", response.vectors);
    }
  } catch (error) {
    console.error("Failed to retrieve vectors for default version:", error);
  }

  // Step 4: Upsert vectors with a specific version ID
  console.log("Upserting vectors to 'spacename' with specific version ID");
  const upsertData2 = {
    vectors: [
      {
        id: 3,
        data: [0.9, 0.8, 0.7, 0.6],
        metadata: { label: "third" },
      },
    ],
  };
  try {
    await client.createVector("spacename", upsertData2);
    console.log("Vectors upserted to version successfully.");
  } catch (error) {
    console.error("Failed to upsert vectors to version:", error);
    return;
  }

  // Step 5: Get vectors by specific version ID
  console.log("Retrieving vectors for specific version ID 1");
  try {
    const response = await client.getVectorsByVersion("spacename", 1, 0, 10);
    if (response) {
      console.log("Retrieved vectors for version 1:\n", response.vectors);
    }
  } catch (error) {
    console.error("Failed to retrieve vectors for version 1:", error);
  }
}

async function main(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);
  try {
    await testVersion(client);
  } finally {
    console.log("Closing the client.");
  }
}

// Run the example
main().catch((error) => {
  console.error("An error occurred:", error);
});
