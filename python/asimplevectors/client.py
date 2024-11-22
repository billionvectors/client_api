"""
asimpleVectors Python Client: A Python client for interacting with asimpleVectors API.
- https://github.com/billionvectors/asimplevectors
"""
import logging
import os
import httpx
import numpy as np
import aiofiles
from requests_toolbelt import MultipartEncoder
from pathlib import Path
from typing import Optional, Any, Dict, Type
from .models import (
    ClusterVote, MembershipConfig, ClusterMetricsResponse,
    SpaceResponse, ListSpacesResponse, SpaceErrorResponse,
    VersionResponse, ListVersionsResponse, VersionErrorResponse,
    VectorResponse, GetVectorsResponse, VectorErrorResponse,
    SearchResponse, SearchErrorResponse,
    SnapshotResponse, ListSnapshotsResponse, SnapshotErrorResponse,
    RbacTokenResponse, ListRbacTokensResponse, RbacTokenErrorResponse,
    KeyValueResponse, ListKeysResponse, KeyValueErrorResponse
)

logger = logging.getLogger(__name__)

class KeyNotFoundError(Exception):
    """Custom exception to indicate that the specified key was not found."""
    pass

class SpaceExistsError(Exception):
    """Custom exception to indicate that the space already exists."""
    pass

class ASimpleVectorsClient:
    """
    Python client for the ASimpleVectors API. Provides methods to interact with cluster, space, version, 
    vector, search, snapshot, RBAC tokens, and key-value store functionalities.

    :param host: The hostname or IP of the ASimpleVectors server.
    :param port: The port number for the API (default: 21001).
    :param use_ssl: Boolean indicating whether to use HTTPS. Defaults to False for localhost.
    :param config: Optional configuration dictionary for additional settings.
    :param token: Optional Bearer token for authorization.
    """
    def __init__(
        self,
        host: str,
        port: int = 21001,
        use_ssl: Optional[bool] = None,
        config: Optional[Dict[str, Any]] = None,
        token: Optional[str] = None
    ):
        """
        Initializes the ASimpleVectorsClient with the given parameters.
        """
        if config is None:
            config = {}

        if use_ssl is None:
            use_ssl = False if host.lower() == 'localhost' else True

        scheme = 'https' if use_ssl else 'http'
        self.base_url = f"{scheme}://{host}:{port}/api"
        self.cluster_url = f"{scheme}://{host}:{port}/cluster"
        self.session = httpx.AsyncClient()

        auth = config.get('auth', None)
        if auth:
            self.session.auth = auth

        # Set default headers
        headers = {'Content-Type': 'application/json'}
        if token:
            headers['Authorization'] = f'Bearer {token}'
        self.session.headers.update(headers)

    def set_token(self, token: str):
        """
        Sets or updates the Authorization token in the client.

        :param token: The Bearer token to use for Authorization.
        """
        self.session.headers['Authorization'] = f'Bearer {token}'

    async def make_request(
        self,
        method: str,
        url: str,
        data: Optional[Dict] = None,
        response_model: Type[Any] = None,
        error_model: Type[Any] = None
    ) -> Optional[Any]:
        """
        Make an HTTP request to the API and handle the response.

        :param method: HTTP method (GET, POST, etc.).
        :param url: API endpoint URL.
        :param data: Request payload as a dictionary.
        :param response_model: Expected model for successful response.
        :param error_model: Expected model for error response.
        :return: Parsed response model or None if an error occurs.
        :raises ConnectionError: If the request fails to connect.
        :raises HTTPStatusError: If the server returns an HTTP error status.
        """
        try:
            logger.debug(f"Making {method} request to {url} with data: {data}")
            headers = {}
            if data:
                headers['Content-Type'] = 'application/json'

            response = await self.session.request(method, url, json=data, headers=headers)
            logger.debug(f"Received response: {response.status_code}, {response.text}")
            response.raise_for_status()

            response_json = response.json()
            logger.debug(f"Response JSON: {response_json}")

            if response_model and response.status_code in {200, 201}:
                if isinstance(response_json, list):
                    return [response_model(**item) for item in response_json]
                return response_model(**response_json)
            elif error_model:
                error = error_model(**response_json)
                logger.error(f"Error response: {error.error}")
                return None
            return response_json  # Return raw JSON if no model is provided
        except Exception as e:
            logger.error(f"Request failed: {e}")
            raise

    # cluster methods
    async def init_cluster(self) -> None:
        """
        Initialize the cluster as a single-node cluster.

        Example:
            client = ASimpleVectorsClient(host="localhost", port=21001)
            await client.init_cluster()
            print("Cluster initialized successfully.")
        """
        url = f"{self.cluster_url}/init"
        try:
            response = await self.make_request("POST", url, data={})
            print("Cluster initialized successfully.")
        except Exception as e:
            print(f"Failed to initialize cluster: {e}")
            raise

    async def add_learner(self, node_id: int, api_addr: str, rpc_addr: str) -> None:
        """
        Add a learner node to the cluster.

        :param node_id: The ID of the learner node.
        :param api_addr: The API address of the learner node.
        :param rpc_addr: The RPC address of the learner node.

        Example:
            await client.add_learner(node_id=2, api_addr="127.0.0.1:21002", rpc_addr="127.0.0.1:22002")
            print("Learner node added successfully.")
        """
        url = f"{self.cluster_url}/add-learner"
        body = [node_id, api_addr, rpc_addr]
        try:
            response = await self.make_request("POST", url, data=body)
            print(f"Learner node {node_id} added successfully.")
        except Exception as e:
            print(f"Failed to add learner node {node_id}: {e}")
            raise

    async def change_membership(self, membership: list) -> None:
        """
        Change the cluster membership.

        :param membership: A list of node IDs to include in the cluster.

        Example:
            await client.change_membership(membership=[1, 2, 3])
            print("Cluster membership changed successfully.")
        """
        url = f"{self.cluster_url}/change-membership"
        body = membership
        try:
            response = await self.make_request("POST", url, data=body)
            print("Cluster membership changed successfully.")
        except Exception as e:
            print(f"Failed to change cluster membership: {e}")
            raise

    async def get_cluster_metrics(self) -> Optional[ClusterMetricsResponse]:
        """
        Fetch and process cluster metrics.

        :return: A ClusterMetricsResponse object or None.

        Example:
            metrics = await client.get_cluster_metrics()
            if metrics:
                print(f"Cluster ID: {metrics.id}, State: {metrics.state}")
        """
        url = f"{self.cluster_url}/metrics"
        try:
            # Make the request directly using session
            response = await self.session.get(url)
            response.raise_for_status()
            response_json = response.json()

            if "Ok" in response_json:
                cluster_metrics = ClusterMetricsResponse.from_response(response_json)
                return cluster_metrics
            else:
                print(f"Unexpected response format: {response_json}")
                return None
        except httpx.RequestError as e:
            print(f"Request failed: {str(e)}")
            raise
        except httpx.HTTPStatusError as e:
            print(f"HTTP Error: {str(e)}")
            raise
        except Exception as e:
            print(f"Failed to fetch cluster metrics: {e}")
            raise

    # Space Methods
    async def create_space(self, space_request: Dict) -> None:
        """
        Creates a new space in the vector database.

        :param space_request: Dictionary containing space configuration details.
        :raises SpaceExistsError: If the space already exists (HTTP 409).
        
        Example:
            space_request = {
                "name": "example_space",
                "dimension": 128,
                "metric": "L2"
            }
            await client.create_space(space_request)
        """
        url = f"{self.base_url}/space"
        try:
            await self.make_request("POST", url, data=space_request)
            print(f"Space '{space_request.get('name')}' created successfully.")
        except httpx.HTTPStatusError as e:
            if e.response.status_code == 409:
                raise SpaceExistsError(f"Space '{space_request.get('name')}' already exists.") from e
            else:
                raise  # Re-throw other HTTP errors

    async def get_space(self, space_name: str) -> Optional[SpaceResponse]:
        """
        Retrieves details of a specific space.
        """
        url = f"{self.base_url}/space/{space_name}"
        try:
            logger.info(f"Retrieving space: {space_name}")
            response_json = await self.make_request("GET", url)
            logger.debug(f"Response for space '{space_name}': {response_json}")
            
            # Ensure response_json is not None
            if not response_json:
                logger.error(f"Space '{space_name}' not found. Response is None.")
                return None

            # Process vector indices if present
            if "version" in response_json:
                for vector_index in response_json["version"].get("vectorIndices", []):
                    if vector_index.get("quantizationConfig") is None:
                        vector_index["quantizationConfig"] = {}

            # Convert JSON to SpaceResponse and ensure defaults
            space_response = SpaceResponse(**response_json)
            for vector_index in space_response.version.vectorIndices:
                vector_index.ensure_defaults()

            return space_response

        except Exception as e:
            logger.error(f"Error retrieving space '{space_name}': {e}")
            return None

    async def update_space(self, space_name: str, space_data: Dict) -> None:
        """
        Updates the configuration of an existing space.

        :param space_name: The name of the space to update.
        :param space_data: Dictionary containing the updated configuration details.

        Example:
            updated_data = {
                "dimension": 256,
                "metric": "Cosine"
            }
            await client.update_space("example_space", updated_data)
        """
        url = f"{self.base_url}/space/{space_name}"
        await self.make_request("POST", url, data=space_data)

    async def delete_space(self, space_name: str) -> None:
        """
        Deletes a space from the vector database.

        :param space_name: The name of the space to delete.

        Example:
            await client.delete_space("example_space")
        """
        url = f"{self.base_url}/space/{space_name}"
        await self.make_request("DELETE", url)

    async def list_spaces(self) -> Optional[ListSpacesResponse]:
        """
        Lists all spaces available in the vector database.

        :return: A `ListSpacesResponse` object containing a list of all spaces, or None if an error occurs.

        Example:
            spaces = await client.list_spaces()
            if spaces and spaces.values:
                for space in spaces.values:
                    print(f"Space: {space.name}, ID: {space.id}")
        """
        url = f"{self.base_url}/spaces"
        return await self.make_request("GET", url, response_model=ListSpacesResponse, error_model=SpaceErrorResponse)

    # Version Methods
    async def create_version(self, space_name: str, version_request: Dict) -> None:
        """
        Creates a new version for the specified space.

        :param space_name: The name of the space for which the version is being created.
        :param version_request: Dictionary with version configuration details.

        Example:
            version_request = {
                "name": "example_version",
                "description": "Version for testing",
                "is_default": True
            }
            await client.create_version("example_space", version_request)
        """
        url = f"{self.base_url}/space/{space_name}/version"
        await self.make_request("POST", url, data=version_request)

    async def list_versions(self, space_name: str) -> Optional[ListVersionsResponse]:
        """
        Lists all versions available for the specified space.

        :param space_name: The name of the space whose versions are to be listed.
        :return: ListVersionsResponse object containing details of all versions, or None if no versions exist.

        Example:
            versions = await client.list_versions("example_space")
            if versions and versions.values:
                for version in versions.values:
                    print(f"Version: {version.name}, ID: {version.id}")
        """
        url = f"{self.base_url}/space/{space_name}/versions"
        return await self.make_request("GET", url, response_model=ListVersionsResponse, error_model=VersionErrorResponse)

    async def get_version_by_id(self, space_name: str, version_id: int) -> Optional[VersionResponse]:
        """
        Retrieves details of a specific version by its ID.

        :param space_name: The name of the space to which the version belongs.
        :param version_id: The ID of the version to retrieve.
        :return: VersionResponse object containing version details, or None if not found.

        Example:
            version = await client.get_version_by_id("example_space", 1)
            if version:
                print(f"Retrieved version: {version.name}, ID: {version.id}")
        """
        url = f"{self.base_url}/space/{space_name}/version/{version_id}"
        return await self.make_request("GET", url, response_model=VersionResponse, error_model=VersionErrorResponse)

    async def get_default_version(self, space_name: str) -> Optional[VersionResponse]:
        """
        Retrieves the default version of a specified space.

        :param space_name: The name of the space whose default version is to be retrieved.
        :return: VersionResponse object containing the default version details, or None if not found.

        Example:
            default_version = await client.get_default_version("example_space")
            if default_version:
                print(f"Default version: {default_version.name}, ID: {default_version.id}")
        """
        url = f"{self.base_url}/space/{space_name}/version"
        return await self.make_request("GET", url, response_model=VersionResponse, error_model=VersionErrorResponse)

    # Vector Methods
    async def create_vector(self, space_name: str, vector_request: Dict) -> None:
        """
        Upserts vectors into the specified space. Supports numpy arrays and lists as input.

        :param space_name: Name of the space where the vectors will be upserted.
        :param vector_request: Dictionary containing vector data. 
        :raises ValueError: If vector data is not a valid numpy array or list.
        :raises Exception: For other failures.

        Example:
            vector_request = {
                "vectors": [
                    {
                        "id": 1,
                        "data": [0.1, 0.2, 0.3],
                        "metadata": {"label": "example"}
                    },
                    {
                        "id": 2,
                        "data": [0.4, 0.5, 0.6],
                        "metadata": {"label": "another_example"}
                    }
                ]
            }
            await client.create_vector("example_space", vector_request)
        """
        url = f"{self.base_url}/space/{space_name}/vector"

        # Validate and convert vector data
        if "vectors" in vector_request:
            for vector in vector_request["vectors"]:
                if isinstance(vector["data"], np.ndarray):
                    # Convert numpy array to list
                    vector["data"] = vector["data"].tolist()
                elif not isinstance(vector["data"], list):
                    raise ValueError(
                        f"Invalid vector data type: {type(vector['data'])}. Expected numpy array or list."
                    )

        # Make the API request
        await self.make_request("POST", url, data=vector_request)
        print(f"Vectors upserted successfully into space '{space_name}'.")

    async def get_vectors_by_version(
        self,
        space_name: str,
        version_id: int,
        start: Optional[int] = None,
        limit: Optional[int] = None
    ) -> Optional[GetVectorsResponse]:
        """
        Retrieves vectors from a specific version of a space.

        :param space_name: Name of the space to retrieve vectors from.
        :param version_id: ID of the version to retrieve vectors for.
        :param start: Optional start index for pagination.
        :param limit: Optional limit for the number of vectors to retrieve.
        :return: GetVectorsResponse object containing vector details, or None if no vectors are found.

        Example:
            vectors = await client.get_vectors_by_version("example_space", 1, start=0, limit=10)
            if vectors and vectors.vectors:
                for vector in vectors.vectors:
                    print(f"Vector ID: {vector.id}")
        """
        url = f"{self.base_url}/space/{space_name}/version/{version_id}/vectors"
        params = {}
        if start is not None:
            params['start'] = start
        if limit is not None:
            params['limit'] = limit

        response = await self.session.get(url, params=params)
        response.raise_for_status()
        response_json = response.json()
        return GetVectorsResponse.from_response(response_json)

    # Search Methods
    async def search_vector(self, space_name: str, search_request: Dict) -> Optional[SearchResponse]:
        """
        Searches for the nearest neighbors to a given vector within a space.

        :param space_name: Name of the space to perform the search in.
        :param search_request: Dictionary containing the search query.
        :return: SearchResponse object containing search results, or None if no matches are found.

        Example:
            search_request = {
                "vector": [0.1, 0.2, 0.3],
                "top_k": 5
            }
            results = await client.search_vector("example_space", search_request)
            if results:
                for result in results:
                    print(f"Distance: {result.distance}, Label: {result.label}")
        """
        url = f"{self.base_url}/space/{space_name}/search"
        return await self.make_request("POST", url, data=search_request, response_model=SearchResponse, error_model=SearchErrorResponse)

    async def search_vector_by_version(self, space_name: str, version_id: int, search_request: Dict) -> Optional[SearchResponse]:
        """
        Searches for the nearest neighbors to a given vector within a specific version of a space.

        :param space_name: Name of the space to perform the search in.
        :param version_id: ID of the version to perform the search in.
        :param search_request: Dictionary containing the search query.
        :return: SearchResponse object containing search results, or None if no matches are found.

        Example:
            client = ASimpleVectorsClient(host="localhost", port=21001)
            search_request = {
                "vector": [0.1, 0.2, 0.3],
                "top_k": 5
            }
            results = await client.search_vector_by_version("example_space", 1, search_request)
            if results:
                for result in results:
                    print(f"Distance: {result.distance}, Label: {result.label}")
        """
        url = f"{self.base_url}/space/{space_name}/version/{version_id}/search"
        return await self.make_request("POST", url, data=search_request, response_model=SearchResponse, error_model=SearchErrorResponse)

    # Snapshot Methods
    async def create_snapshot(self, snapshot_request: Dict) -> None:
        """
        Creates a snapshot for the specified space.

        :param snapshot_request: Dictionary containing snapshot creation details.
        :raises Exception: For failures during snapshot creation.

        Example:
            await client.create_snapshot({})
        """
        url = f"{self.base_url}/snapshot"
        await self.make_request("POST", url, data=snapshot_request)

    async def list_snapshots(self) -> Optional[ListSnapshotsResponse]:
        """
        Lists all available snapshots.

        :return: ListSnapshotsResponse containing snapshot metadata or None if no snapshots exist.
        :raises Exception: For failures during snapshot retrieval.

        Example:
            snapshots = await client.list_snapshots()
            if snapshots:
                for snapshot in snapshots.snapshots:
                    print(f"Snapshot file: {snapshot.file_name}")
        """
        url = f"{self.base_url}/snapshots"
        return await self.make_request("GET", url, response_model=ListSnapshotsResponse, error_model=SnapshotErrorResponse)

    async def delete_snapshot(self, snapshot_date: str) -> None:
        """
        Deletes a snapshot by its date.

        :param snapshot_date: The date portion of the snapshot filename (e.g., "202311161122": yyyymmddHHMM).
        :raises ValueError: If the snapshot_date is invalid.
        :raises Exception: For failures during snapshot deletion.

        Example:
            await client.delete_snapshot("202311161122")
        """
        url = f"{self.base_url}/snapshot/{snapshot_date}/delete"
        try:
            response = await self.session.delete(url)
            response.raise_for_status()
            print(f"Snapshot with date {snapshot_date} deleted successfully.")
        except httpx.HTTPStatusError as e:
            print(f"HTTP Error while deleting snapshot: {str(e)}")
            raise e
        except Exception as e:
            print(f"An error occurred while deleting snapshot: {e}")
            raise

    async def download_snapshot(self, snapshot_date: str, download_folder: str) -> str:
        """
        Downloads a snapshot to the specified folder.

        :param snapshot_date: Date string extracted from the snapshot filename.
        :param download_folder: Folder path where the snapshot will be downloaded.
        :return: Full path of the downloaded snapshot file.
        :raises Exception: For failures during snapshot download.

        Example:
            snapshot_path = await client.download_snapshot("202311161122", "./temp")
        """
        url = f"{self.base_url}/snapshot/{snapshot_date}/download"
        
        # Ensure the download folder exists
        os.makedirs(download_folder, exist_ok=True)

        file_path = os.path.join(download_folder, f"snapshot-{snapshot_date}.zip")
        async with self.session.stream("GET", url) as response:
            response.raise_for_status()
            with open(file_path, "wb") as file:
                async for chunk in response.aiter_bytes():
                    file.write(chunk)

        print(f"Snapshot downloaded to {file_path}")
        return file_path

    async def restore_snapshot(self, snapshot_date: str) -> None:
        """
        Restores a snapshot by its date.

        :param snapshot_date: Date string extracted from the snapshot filename.
        :raises Exception: For failures during snapshot restoration.

        Example:
            await client.restore_snapshot("202311161122")
        """
        url = f"{self.base_url}/snapshot/{snapshot_date}/restore"
        await self.make_request("POST", url, data={})
        print(f"Snapshot from date {snapshot_date} restored successfully.")

    async def upload_restore_snapshot(self, file_path: str) -> None:
        """
        Uploads a snapshot file and restores it on the server.

        :param file_path: The full path to the snapshot file to upload.
        :raises Exception: For failures during snapshot upload and restoration.

        Example:
            await client.upload_restore_snapshot("./temp/snapshot-202311161122.zip")
        """
        url = f"{self.base_url}/snapshots/restore"
        print(f"Uploading file: {file_path}")
        print(f"POST URL: {url}")

        try:
            # Prepare the multipart data using MultipartEncoder
            async with aiofiles.open(file_path, 'rb') as f:
                file_data = await f.read()

            encoder = MultipartEncoder(
                fields={
                    "file": (file_path.split('/')[-1], file_data, "application/zip"),
                }
            )
            headers = self.session.headers.copy()
            headers["Content-Type"] = encoder.content_type

            # Use httpx to send the request
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    url,
                    headers=headers,
                    content=encoder.to_string(),  # Serialize the multipart data
                )
                response.raise_for_status()
                print(f"Snapshot restored successfully: {response.json()}")

        except httpx.HTTPStatusError as e:
            print(f"HTTP Error: {str(e)}")
            print("Bad Request: Check if the server accepts multipart file uploads.")
            raise e
        except Exception as e:
            print(f"Failed to upload and restore snapshot: {str(e)}")
            raise

    # Security Methods
    async def create_rbac_token(self, rbac_request: Dict) -> None:
        """
        Creates a new RBAC token.

        :param rbac_request: Dictionary containing RBAC token creation details.
        :raises Exception: For failures during RBAC token creation.

        Example:
            rbac_request = {
                "space_id": 1,
                "system": 1,
                "space": 1,
                "version": 1,
                "vector": 1,
                "snapshot": 1,
                "security": 1,
                "keyvalue": 1
            }
            await client.create_rbac_token(rbac_request)
            print("RBAC token created successfully.")
        """
        url = f"{self.base_url}/security/tokens"
        await self.make_request("POST", url, data=rbac_request)

    async def list_rbac_tokens(self) -> Optional[ListRbacTokensResponse]:
        """
        Lists all existing RBAC tokens.

        :return: ListRbacTokensResponse containing RBAC token metadata or None if no tokens exist.
        :raises Exception: For failures during RBAC token retrieval.

        Example:
            tokens = await client.list_rbac_tokens()
            if tokens:
                for token in tokens.tokens:
                    print(f"Token ID: {token.id}, Token: {token.token}")
        """
        url = f"{self.base_url}/security/tokens"
        response_json = await self.make_request("GET", url)
        if response_json:
            return ListRbacTokensResponse.from_response(response_json)
        return None

    async def delete_rbac_token(self, token: str) -> None:
        """
        Deletes an existing RBAC token by its value.

        :param token: The RBAC token to delete.
        :raises Exception: For failures during token deletion.

        Example:
            token_to_delete = "example_token_value"
            await client.delete_rbac_token(token_to_delete)
        """
        url = f"{self.base_url}/security/tokens/{token}"
        try:
            response = await self.session.delete(url)
            response.raise_for_status()
        except httpx.HTTPStatusError as e:
            print(f"HTTP Error while deleting RBAC token: {str(e)}")
            raise e
        except Exception as e:
            print(f"An error occurred while deleting RBAC token: {e}")
            raise

    async def update_rbac_token(self, token: str, rbac_request: Dict) -> None:
        """
        Updates an existing RBAC token.

        :param token: The RBAC token to update.
        :param rbac_request: Dictionary containing updated RBAC token details.
        :raises Exception: For failures during token update.

        Example:
            token_to_update = "example_token_value"
            updated_rbac_request = {
                "space_id": 1,
                "system": 1,
                "space": 1,
                "version": 1,
                "vector": 1,
                "snapshot": 1,
                "security": 1,
                "keyvalue": 1
            }
            await client.update_rbac_token(token_to_update, updated_rbac_request)
        """
        url = f"{self.base_url}/security/tokens/{token}"
        try:
            response = await self.session.put(url, json=rbac_request)
            response.raise_for_status()
        except httpx.HTTPStatusError as e:
            print(f"HTTP Error while updating RBAC token: {str(e)}")
            raise e
        except Exception as e:
            print(f"An error occurred while updating RBAC token: {e}")
            raise

    # Key-Value Storage Methods
    async def put_key_value(self, space_name: str, key: str, value: Dict) -> None:
        """
        Stores or updates a key-value pair in a specified space.

        :param space_name: Name of the space where the key-value pair will be stored.
        :param key: The key to store or update.
        :param value: The value to be associated with the key. Must be a dictionary.
        :raises Exception: For failures during the key-value insertion process.

        Example:
            space_name = "example_space"
            key = "example_key"
            value = {"data": "example_value"}
            try:
                await client.put_key_value(space_name, key, value)
                print(f"Key '{key}' stored successfully in space '{space_name}'.")
            except Exception as e:
                print(f"Failed to store key-value pair: {e}")
        """
        url = f"{self.base_url}/space/{space_name}/key/{key}"
        await self.make_request("POST", url, data=value)

    async def get_key_value(self, space_name: str, key: str) -> Optional[str]:
        """
        Retrieves the value of a specified key in the given space.

        :param space_name: Name of the space.
        :param key: The key to retrieve.
        :return: The value of the key as a string, or None if the key is not found.
        :raises KeyNotFoundError: If the key does not exist in the specified space.
        :raises Exception: For other failures during the retrieval process.

        Example:
            space_name = "example_space"
            key = "example_key"
            try:
                value = await client.get_key_value(space_name, key)
                if value:
                    print(f"Value for key '{key}': {value}")
                else:
                    print(f"Key '{key}' not found in space '{space_name}'.")
            except KeyNotFoundError as e:
                print(f"Key not found: {e}")
            except Exception as e:
                print(f"Failed to retrieve key-value pair: {e}")
        """
        url = f"{self.base_url}/space/{space_name}/key/{key}"
        try:
            response = await self.session.get(url)
            response.raise_for_status()
            return response.text
        except httpx.HTTPStatusError as e:
            if e.response.status_code in {400, 404}:
                raise KeyNotFoundError(f"Key '{key}' not found in space '{space_name}'.") from e
            else:
                raise

    async def list_keys(self, space_name: str) -> Optional[ListKeysResponse]:
        """
        Lists all keys in a given space.

        :param space_name: Name of the space for which keys are to be listed.
        :return: A ListKeysResponse object containing the list of keys or None if no keys are found.
        :raises Exception: For failures during the key retrieval process.

        Example:
            space_name = "example_space"
            keys_response = await client.list_keys(space_name)
            if keys_response:
                print("Keys in space:", keys_response.keys)
        """
        url = f"{self.base_url}/space/{space_name}/keys"
        return await self.make_request("GET", url, response_model=ListKeysResponse, error_model=KeyValueErrorResponse)

    async def delete_key_value(self, space_name: str, key: str) -> None:
        """
        Deletes a specific key-value pair in a given space.

        :param space_name: Name of the space containing the key.
        :param key: The key to be deleted.
        :raises KeyNotFoundError: If the key does not exist in the specified space.
        :raises Exception: For failures during the key deletion process.

        Example:
            space_name = "example_space"
            key_to_delete = "example_key"
            try:
                await client.delete_key_value(space_name, key_to_delete)
                print(f"Key '{key_to_delete}' deleted successfully.")
            except KeyNotFoundError as e:
                print(f"Key not found: {e}")
            except Exception as e:
                print(f"Failed to delete key: {e}")
        """
        url = f"{self.base_url}/space/{space_name}/key/{key}"
        try:
            response = await self.session.delete(url)
            response.raise_for_status()
            print(f"Key '{key}' deleted successfully.")
        except httpx.HTTPStatusError as e:
            if e.response.status_code in {400, 404}:
                raise KeyNotFoundError(f"Key '{key}' not found in space '{space_name}'.") from e
            else:
                raise  # Rethrow other HTTP errors

    async def close(self) -> None:
        await self.session.aclose()
