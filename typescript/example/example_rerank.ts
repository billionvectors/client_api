import { ASimpleVectorsClient } from "../asimplevectors/src";

async function testRerank(client: ASimpleVectorsClient): Promise<void> {
  // Step 1: Create space 'rerankspace' on leader
  console.log("Creating space 'rerankspace' on leader");
  const createSpaceData = {
    name: "rerankspace",
    dimension: 4,
    metric: "L2",
    hnsw_config: {
      M: 64,
      ef_construct: 500,
    },
  };
  await client.createSpace(createSpaceData);
  console.log("Space 'rerankspace' created successfully.");

  // Step 2: Upsert vectors to 'rerankspace' with documents and tokens
  console.log("Upserting vectors to 'rerankspace' with documents and tokens");
  const upsertVectorsData = {
    vectors: [
      {
        id: 1,
        data: [0.1, 0.2, 0.3, 0.4],
        doc: "This is a test document about vectors.",
        doc_tokens: ["test", "document", "vectors"],
        metadata: { category: "A" },
      },
      {
        id: 2,
        data: [0.5, 0.6, 0.7, 0.8],
        doc: "Another document with different content.",
        doc_tokens: ["another", "document", "content"],
        metadata: { category: "B" },
      },
    ],
  };
  await client.upsertVector("rerankspace", upsertVectorsData);
  console.log("Vectors with documents upserted successfully.");

  // Step 3: Perform rerank operation
  console.log("Performing rerank operation");
  const rerankData = {
    vector: [0.1, 0.2, 0.3, 0.4],
    tokens: ["test", "vectors"],
    top_k: 2,
  };
  const rerankResponse = await client.rerank("rerankspace", rerankData);
  if (rerankResponse) {
    console.log("Rerank response:\n", rerankResponse);
  }
}

async function main(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);
  try {
    // Run the rerank test
    await testRerank(client);
  } finally {
    console.log("Closing the client.");
    // Add any necessary cleanup logic here
  }
}

// Run the example
main().catch((error) => {
  console.error("An error occurred:", error);
});
