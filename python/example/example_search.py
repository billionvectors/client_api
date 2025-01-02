import asyncio
from asimplevectors.client import ASimpleVectorsClient

# Test function to create a space, upsert vectors, and search vectors
async def test_search(client: ASimpleVectorsClient):
    # Step 1: Create space 'spacename' on leader
    print("Creating space 'spacename' on leader")
    create_space_data = {
        "name": "spacename",
        "dimension": 4,
        "metric": "L2",
        "hnsw_config": {
            "M": 64,
            "ef_construct": 500
        }
    }
    await client.create_space(create_space_data)
    print("Space 'spacename' created successfully.")

    # Step 2: Upsert vectors to 'spacename' without specifying version
    print("Upserting vectors to 'spacename' without specifying version")
    upsert_vectors_data = {
        "vectors": [
            {
                "id": 1,
                "data": [0.1, 0.2, 0.3, 0.4],
                "metadata": {"meta": "first"}
            },
            {
                "id": 2,
                "data": [0.5, 0.6, 0.7, 0.8],
                "metadata": {"meta": "second"}
            },
            {
                "id": 3,
                "data": [0.9, 0.8, 0.7, 0.6],
                "metadata": {"meta": "third"}
            },
            {
                "id": 4,
                "data": [1.0, 0.1, 0.2, 0.3],
                "metadata": {"meta": "forth"}
            },
            {
                "id": 5,
                "data": [0.2, 0.3, 0.4, 0.3],
                "metadata": {"meta": "fifth"}
            }
        ]
    }
    await client.upsert_vector("spacename", upsert_vectors_data)
    print("Vectors upserted successfully.")

    # Step 3: Search vectors with a specific version ID
    print("Searching vectors with specific version id")
    search_data = {
        "vector": [0.2, 0.3, 0.4, 0.3]
    }
    response = await client.search_vector("spacename", search_data)
    if response:
        print("Search response:]\n", response)
    
    # Step 4: Filtered search with metadata
    print("Filter search with metadata")
    search_data_with_filter = {
        "vector": [0.2, 0.3, 0.4, 0.3],
        "filter": "meta == 'first' OR meta == 'second'"
    }
    response = await client.search_vector("spacename", search_data_with_filter)
    if response:
        print("Filtered search response:\n", response)

async def main():
    client = ASimpleVectorsClient(host="localhost")
    try:
        # Run the search test
        await test_search(client)
    finally:
        # Ensure the client session is closed
        await client.close()

if __name__ == "__main__":
    asyncio.run(main())
