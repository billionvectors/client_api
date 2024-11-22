# asimplevectors Client Library

asimplevectors Client is a Python package providing an asynchronous API client to interact with the asimplevectors service, supporting vector management, search, space configuration, and RBAC-based security.

## Features

- **Space Management**: Create, update, delete, and list spaces with flexible configurations.
- **Versioning**: Manage versions for spaces, including creating and retrieving specific versions.
- **Vector Operations**: Upsert, retrieve, and search vectors with support for numpy arrays and lists.
- **RBAC Security**: Manage tokens for role-based access control (RBAC) and apply them to secure API calls.
- **Snapshot Management**: Create and manage snapshots of vector spaces.
- **Async Support**: Fully asynchronous API for high-performance applications.

## Installation

pip installation will be supported soon.

## Requirements
- [asimplevectors](https://github.com/billionvectors/asimplevectors)
- Python 3.8+
- Dependencies listed in requirements.txt

## Usage
### Initialization
```python
from asimplevectors.client import ASimpleVectorsClient

# Initialize the client
client = ASimpleVectorsClient(host="localhost", port=21001)

# Use async context manager to ensure session closure
async with client:
    ...
```
### Example: Space Management
```python
import asyncio

async def main():
    client = ASimpleVectorsClient(host="localhost")

    # Create a space
    create_space_data = {
        "name": "spacename",
        "dimension": 128,
        "metric": "L2"
    }
    await client.create_space(create_space_data)
    print("Space created successfully.")

    # List spaces
    spaces = await client.list_spaces()
    print("Available spaces:", spaces)

    await client.close()

asyncio.run(main())
```
### Example: Vector Operations
```python
import numpy as np
import asyncio

async def vector_operations():
    client = ASimpleVectorsClient(host="localhost")

    # Upsert vectors
    vector_data = {
        "vectors": [
            {"id": 1, "data": np.array([0.1, 0.2, 0.3, 0.4]), "metadata": {"label": "first"}}
        ]
    }
    await client.create_vector("spacename", vector_data)
    print("Vector upserted successfully.")

    # Retrieve vectors by version
    vectors = await client.get_vectors_by_version("spacename", version_id=0)
    print("Retrieved vectors:", vectors)

    await client.close()

asyncio.run(vector_operations())
```
### Example: RBAC Token Management
```python
async def manage_tokens():
    client = ASimpleVectorsClient(host="localhost")

    # Create an RBAC token
    token_data = {
        "user_id": 1,
        "space": 2,
        "vector": 2
    }
    await client.create_rbac_token(token_data)
    print("Token created successfully.")

    # List RBAC tokens
    tokens = await client.list_rbac_tokens()
    print("Available tokens:", tokens)

    await client.close()

asyncio.run(manage_tokens())
```
## Development
### Setting up the development environment
1. Setup [asimplevectors](https://github.com/billionvectors/asimplevectors) server from docker
```bash
docker pull billionvectors/asimplevectors:0.2.0
docker run -p 21001:21001 -p 21002:21002 asimplevectors:0.2.0
```

2. Clone the repository:
```bash
git clone https://github.com/billionvectors/client_api.git
cd client_api
```

3. Run test
```bash
cd python
./run_example.sh search
```