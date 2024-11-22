import asyncio
from asimplevectors.client import ASimpleVectorsClient, SpaceExistsError

# Test function to upsert, retrieve, and manage vectors
async def test_vector(client: ASimpleVectorsClient):
    # Step 1: Create a space
    print("Creating space 'spacename'")
    create_space_data = {
        "name": "spacename",
        "dimension": 4,
        "metric": "L2",
        "hnsw_config": {
            "M": 16,
            "ef_construct": 100
        }
    }
    try:
        await client.create_space(create_space_data)
    except SpaceExistsError as e:
        print(e)

    # Step 2: Upsert vectors without specifying version
    print("Upserting vectors to 'spacename' without specifying version")
    upsert_data_1 = {
        "vectors": [
            {
                "id": 1,
                "data": [0.1, 0.2, 0.3, 0.4],
                "metadata": {"label": "first"}
            },
            {
                "id": 2,
                "data": [0.5, 0.6, 0.7, 0.8],
                "metadata": {"label": "second"}
            }
        ]
    }
    await client.create_vector("spacename", upsert_data_1)
    print("Vectors upserted successfully.")

    # Step 3: Get vectors by default version
    print("Retrieving vectors for default version")
    response = await client.get_vectors_by_version("spacename", version_id=0, start=0, limit=10)
    if response:
        print("Retrieved vectors:\n", response)

    # Step 4: Upsert vectors with a specific version ID
    print("Upserting vectors to 'spacename' with specific version ID")
    upsert_data_2 = {
        "vectors": [
            {
                "id": 3,
                "data": [0.9, 0.8, 0.7, 0.6],
                "metadata": {"label": "third"}
            }
        ]
    }
    await client.create_vector("spacename", upsert_data_2)
    print("Vectors upserted to version successfully.")

    # Step 5: Get vectors by specific version ID
    print("Retrieving vectors for specific version ID 1")
    response = await client.get_vectors_by_version("spacename", version_id=1, start=0, limit=10)
    if response:
        print("Retrieved vectors for version 1:\n", response)

async def main():
    client = ASimpleVectorsClient(host="localhost")
    try:
        await test_vector(client)
    finally:
        await client.close()

if __name__ == "__main__":
    asyncio.run(main())
