import asyncio
from asimplevectors.client import ASimpleVectorsClient, SpaceExistsError

async def test_security(client: ASimpleVectorsClient):
    # Step 1: Try to create space without permission
    print("Attempting to create space 'spacename' without permission.")
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
    except Exception as e:
        print(f"Expected failure to create space without token: {e}")

    # Step 2: Create a new RBAC token
    print("Creating a new RBAC token.")
    rbac_token_data = {
        "user_id": 0,
        "system": 2,
        "space": 2,
        "version": 2,
        "vector": 2,
        "snapshot": 2
    }
    try:
        await client.create_rbac_token(rbac_token_data)
        print("RBAC token created successfully.")
    except Exception as e:
        print(f"Failed to create RBAC token: {e}")

    # Step 3: List all RBAC tokens and extract the token
    print("Listing all RBAC tokens.")
    try:
        tokens_response = await client.list_rbac_tokens()
        if tokens_response and tokens_response.tokens:
            token = tokens_response.tokens[0].token
            print(f"Extracted token: {token}")
            client.set_token(token)  # Set token in client
        else:
            print("No tokens found.")
    except Exception as e:
        print(f"Failed to list RBAC tokens: {e}")
        return

    # Step 4: Create space with a valid token
    print("Creating space 'spacename' with valid token.")
    try:
        await client.create_space(create_space_data)
        print("Space 'spacename' created successfully.")
    except SpaceExistsError:
        print("Space 'spacename' already exists.")
    except Exception as e:
        print(f"Failed to create space with token: {e}")

    # Step 5: Upsert vectors into the space
    print("Upserting vectors into 'spacename'.")
    upsert_data = {
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
            },
        ]
    }
    try:
        await client.create_vector("spacename", upsert_data)
        print("Vectors upserted successfully.")
    except Exception as e:
        print(f"Failed to upsert vectors: {e}")

    # Step 6: Search vectors with a specific version ID
    print("Searching vectors in 'spacename'.")
    search_data = {
        "vector": [0.2, 0.3, 0.4, 0.3]
    }
    try:
        search_results = await client.search_vector_by_version("spacename", 1, search_data)
        if search_results:
            print("Search results:", search_results)
        else:
            print("No search results found.")
    except Exception as e:
        print(f"Failed to search vectors: {e}")

async def main():
    client = ASimpleVectorsClient(host="localhost")

    try:
        await test_security(client)
    finally:
        await client.close()

if __name__ == "__main__":
    asyncio.run(main())
