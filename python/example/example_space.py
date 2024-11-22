import asyncio
from asimplevectors.client import ASimpleVectorsClient, SpaceExistsError

# Test function to create, read, list, and update space configurations
async def test_space(client: ASimpleVectorsClient):
    # Step 1: Create a space
    print("Creating space 'spacename'")
    create_space_data = {
        "name": "spacename",
        "dimension": 128,  # default dense index
        "metric": "cosine",
        "hnsw_config": {
            "ef_construct": 123
        },
        "quantization_config": {
            "scalar": {
                "type": "int8",
            }
        },
        "sparse": {
            "metric": "Cosine"
        },
        "indexes": {  # additional custom indexes
            "my_index": {
                "dimension": 1536,
                "metric": "Cosine",
                "hnsw_config": {
                    "m": 32,
                    "ef_construct": 123
                },
                "quantization_config": {
                    "scalar": {
                        "type": "int8",
                    }
                }
            },
        }
    }

    try:
        await client.create_space(create_space_data)
        print("Space 'spacename' created successfully.")
    except SpaceExistsError:
        print("Space 'spacename' already exists.")

    # Step 2: Retrieve the space details
    print("Retrieving space 'spacename'")
    response = await client.get_space("spacename")
    if response:
        print("Space details:\n", response)

    # Step 3: List all spaces
    print("Listing all spaces")
    response = await client.list_spaces()
    if response:
        print("Spaces:", response)

    # Step 4: Update the space configuration
    print("Updating space 'spacename'")
    update_space_data = {
        "dense": {
            "dimension": 1234,
            "metric": "l2",
            "hnsw_config": {
                "m": 64,
                "ef_construct": 55
            }
        }
    }
    await client.update_space("spacename", update_space_data)
    print("Space 'spacename' updated successfully.")

    # Retrieve the updated space details
    print("Retrieving updated space 'spacename'")
    response = await client.get_space("spacename")
    if response:
        print("Updated space details:", response)

async def main():
    client = ASimpleVectorsClient(host="localhost")
    try:
        # Run the space API test
        await test_space(client)
    finally:
        # Ensure the client session is closed
        await client.close()

if __name__ == "__main__":
    asyncio.run(main())
