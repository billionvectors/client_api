import asyncio
from asimplevectors.client import ASimpleVectorsClient, KeyNotFoundError

# Test function to create, read, list, and delete key-value pairs
async def test_keyvalue(client: ASimpleVectorsClient):
    # Step 1: Create a key-value pair in the space
    print("Creating key-value pair in 'spacename'")
    key_name = "test_key"
    key_value_data = {
        "text": "Hello, ASimpleVectors!"
    }
    await client.put_key_value("spacename", key_name, key_value_data)
    print(f"Key '{key_name}' created successfully.")

    # Step 2: Retrieve the key-value pair
    print(f"Retrieving key-value pair for key '{key_name}'")
    response = await client.get_key_value("spacename", key_name)
    if response:
        print(f"Key '{key_name}' retrieved successfully:", response)

    # Step 3: List all keys in the space
    print("Listing all keys in 'spacename'")
    response = await client.list_keys("spacename")
    if response:
        print("Keys in 'spacename':", response.keys)

    # Step 4: Delete the key-value pair
    print(f"Deleting key '{key_name}' from 'spacename'")
    await client.delete_key_value("spacename", key_name)
    print(f"Key '{key_name}' deleted successfully.")

    # Step 5: Verify the key is deleted
    print(f"Verifying deletion of key '{key_name}'")
    try:
        response = await client.get_key_value("spacename", key_name)
        print(f"Key '{key_name}' still exists:", response)
    except KeyNotFoundError:
        print(f"Key '{key_name}' successfully deleted.")

async def main():
    client = ASimpleVectorsClient(host="localhost")
    try:
        # Run the key-value storage test
        await test_keyvalue(client)
    finally:
        # Ensure the client session is closed
        await client.close()

if __name__ == "__main__":
    asyncio.run(main())
