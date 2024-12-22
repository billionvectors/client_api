import asyncio
from asimplevectors.client import ASimpleVectorsClient

# Test function to create a space, upsert vectors with document tokens, and perform rerank
async def test_rerank(client: ASimpleVectorsClient):
    # Step 1: Create space 'rerank_space' on leader
    print("Creating space 'rerank_space' on leader")
    create_space_data = {
        "name": "rerank_space",
        "dimension": 4,
        "metric": "L2",
        "hnsw_config": {
            "M": 64,
            "ef_construct": 500
        }
    }
    await client.create_space(create_space_data)
    print("Space 'rerank_space' created successfully.")

    # Step 2: Upsert vectors with document tokens
    print("Upserting vectors to 'rerank_space' with document tokens")
    upsert_vectors_data = {
        "vectors": [
            {
                "id": 1,
                "data": [0.1, 0.2, 0.3, 0.4],
                "doc": "This is a test document about vectors.",
                "doc_tokens": ["test", "document", "vectors"]
            },
            {
                "id": 2,
                "data": [0.5, 0.6, 0.7, 0.8],
                "doc": "Another document with different content.",
                "doc_tokens": ["another", "document", "content"]
            }
        ]
    }
    await client.upsert_vector("rerank_space", upsert_vectors_data)
    print("Vectors upserted successfully with document tokens.")

    # Step 3: Perform rerank operation
    print("Performing rerank operation")
    rerank_data = {
        "vector": [0.1, 0.2, 0.3, 0.4],
        "tokens": ["test", "vectors"],
        "top_k": 2
    }
    response = await client.rerank("rerank_space", rerank_data)
    if response:
        print("Rerank response:\n", response)

async def main():
    client = ASimpleVectorsClient(host="localhost")
    try:
        # Run the rerank test
        await test_rerank(client)
    finally:
        # Ensure the client session is closed
        await client.close()

if __name__ == "__main__":
    asyncio.run(main())
