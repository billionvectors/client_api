import { ASimpleVectorsClient } from "../asimplevectors/src";

async function testSpace(client: ASimpleVectorsClient): Promise<void> {
  // Step 1: Create a space
  console.log("Creating space 'spacename'");
  const createSpaceData = {
    name: "spacename",
    dimension: 128, // Default dense index
    metric: "cosine",
    hnsw_config: {
      ef_construct: 123,
    },
    quantization_config: {
      scalar: {
        type: "int8",
      },
    },
    sparse: {
      metric: "Cosine",
    },
    indexes: {
      my_index: {
        dimension: 1536,
        metric: "Cosine",
        hnsw_config: {
          m: 32,
          ef_construct: 123,
        },
        quantization_config: {
          scalar: {
            type: "int8",
          },
        },
      },
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

  // Step 2: Retrieve the space details
  console.log("Retrieving space 'spacename'");
  try {
    const response = await client.getSpace("spacename");
    if (response) {
      console.log("Space details:\n", response);
    }
  } catch (error) {
    console.error("Failed to retrieve space details:", error);
    return;
  }

  // Step 3: List all spaces
  console.log("Listing all spaces");
  try {
    const response = await client.listSpaces();
    if (response && response.values) {
      console.log("Spaces:", response.values);
    }
  } catch (error) {
    console.error("Failed to list spaces:", error);
    return;
  }

  // Step 4: Update the space configuration
  console.log("Updating space 'spacename'");
  const updateSpaceData = {
    dense: {
      dimension: 1234,
      metric: "l2",
      hnsw_config: {
        m: 64,
        ef_construct: 55,
      },
    },
  };
  try {
    await client.updateSpace("spacename", updateSpaceData);
    console.log("Space 'spacename' updated successfully.");
  } catch (error) {
    console.error("Failed to update space:", error);
    return;
  }

  // Retrieve the updated space details
  console.log("Retrieving updated space 'spacename'");
  try {
    const response = await client.getSpace("spacename");
    if (response) {
      console.log("Updated space details:", response);
    }
  } catch (error) {
    console.error("Failed to retrieve updated space details:", error);
  }
}

async function main(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);
  try {
    // Run the space API test
    await testSpace(client);
  } finally {
    console.log("Closing the client.");
  }
}

// Run the example
main().catch((error) => {
  console.error("An error occurred:", error);
});
