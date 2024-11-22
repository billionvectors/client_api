import logging
import asyncio
import unittest
from asimplevectors.client import ASimpleVectorsClient

class RegressionTest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        """
        Set up the ASimpleVectorsClient with a live endpoint.
        """
        cls.loop = asyncio.get_event_loop()
        cls.client = ASimpleVectorsClient(host="localhost", port=21001)

        # Configure logging
        logging.basicConfig(
            level=logging.DEBUG,
            format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
        )
        cls.logger = logging.getLogger(__name__)

    @classmethod
    def tearDownClass(cls):
        """
        Close the ASimpleVectorsClient session after tests.
        """
        # Run the `close` coroutine using the existing event loop
        cls.loop.run_until_complete(cls.client.close())
        cls.loop.close()

    async def log(self, test_name, step, message):
        print(f"[{test_name} - Step {step}] {message}")

    def test_cluster_operations(self):
        """
        Test cluster initialization and metrics retrieval.
        """
        async def test():
            test_name = "test_cluster_operations"
            await self.log(test_name, 1, "Initializing the cluster.")
            await self.client.init_cluster()

            await self.log(test_name, 2, "Fetching cluster metrics.")
            metrics = await self.client.get_cluster_metrics()
            self.assertIsNotNone(metrics)
            self.assertEqual(metrics.state, "Leader")
            await self.log(test_name, 3, "Cluster metrics validated successfully.")

        self.loop.run_until_complete(test())

    def test_space_operations(self):
        """
        Test creating, retrieving, updating, and deleting spaces.
        """
        async def test():
            test_name = "test_space_operations"
            space_request = {"name": "test_space", "dimension": 128, "metric": "L2"}

            await self.log(test_name, 1, f"Creating space: {space_request['name']}.")
            await self.client.create_space(space_request)

            # Retrieve space
            await self.log(test_name, 2, f"Retrieving space: {space_request['name']}.")
            try:
                space = await self.client.get_space("test_space")
                await self.log(test_name, 2, f"Retrieved space: {space}")
                self.assertIsNotNone(space, "Space not found after creation.")
                self.assertEqual(space.name, "test_space")
            except Exception as e:
                await self.log(test_name, 2, f"Error retrieving space: {e}")
                self.fail(f"Space retrieval failed: {e}")

            updated_space_data = {"dimension": 256, "metric": "Cosine"}
            await self.log(test_name, 3, f"Updating space: {space_request['name']}.")
            await self.client.update_space("test_space", updated_space_data)

            await self.log(test_name, 4, f"Deleting space: {space_request['name']}.")
            await self.client.delete_space("test_space")

        self.loop.run_until_complete(test())

    def test_version_operations(self):
        """
        Test creating and retrieving versions.
        """
        async def test():
            test_name = "test_version_operations"
            space_request = {"name": "version_test_space", "dimension": 128, "metric": "L2"}

            await self.log(test_name, 1, f"Creating space: {space_request['name']}.")
            await self.client.create_space(space_request)

            version_request = {"name": "v1", "description": "Version for testing", "is_default": True}
            await self.log(test_name, 2, f"Creating version: {version_request['name']}.")
            await self.client.create_version("version_test_space", version_request)

            await self.log(test_name, 3, "Listing versions.")
            versions = await self.client.list_versions("version_test_space")
            self.assertIsNotNone(versions)
            self.assertGreater(len(versions.values), 0)

            await self.log(test_name, 4, f"Deleting space: {space_request['name']}.")
            await self.client.delete_space("version_test_space")

        self.loop.run_until_complete(test())

    def test_vector_operations(self):
        """
        Test vector creation and retrieval.
        """
        async def test():
            test_name = "test_vector_operations"
            space_request = {"name": "vector_test_space", "dimension": 3, "metric": "L2"}
            vector_request = {
                "vectors": [
                    {"id": 1, "data": [0.1, 0.2, 0.3], "metadata": {"label": "example"}},
                    {"id": 2, "data": [0.4, 0.5, 0.6], "metadata": {"label": "another_example"}},
                ]
            }

            await self.log(test_name, 1, f"Creating space: {space_request['name']}.")
            await self.client.create_space(space_request)

            await self.log(test_name, 2, "Adding vectors.")
            await self.client.create_vector("vector_test_space", vector_request)

            await self.log(test_name, 3, "Retrieving vectors by version.")
            vectors = await self.client.get_vectors_by_version("vector_test_space", version_id=1)
            self.assertIsNotNone(vectors)
            self.assertGreater(len(vectors.vectors), 0)

            await self.log(test_name, 4, f"Deleting space: {space_request['name']}.")
            await self.client.delete_space("vector_test_space")

        self.loop.run_until_complete(test())

    def test_search_operations(self):
        """
        Test vector search functionality.
        """
        async def test():
            test_name = "test_search_operations"
            space_request = {"name": "search_test_space", "dimension": 3, "metric": "L2"}
            vector_request = {
                "vectors": [
                    {"id": 1, "data": [0.1, 0.2, 0.3], "metadata": {"label": "example"}},
                    {"id": 2, "data": [0.4, 0.5, 0.6], "metadata": {"label": "another_example"}},
                ]
            }
            search_request = {"vector": [0.1, 0.2, 0.3], "top_k": 1}

            await self.log(test_name, 1, f"Creating space: {space_request['name']}.")
            await self.client.create_space(space_request)

            await self.log(test_name, 2, "Adding vectors.")
            await self.client.create_vector("search_test_space", vector_request)

            await self.log(test_name, 3, "Performing vector search.")
            results = await self.client.search_vector("search_test_space", search_request)
            self.assertIsNotNone(results)

            await self.log(test_name, 4, f"Deleting space: {space_request['name']}.")
            await self.client.delete_space("search_test_space")

        self.loop.run_until_complete(test())

    def test_key_value_operations(self):
        """
        Test key-value storage functionality.
        """
        async def test():
            test_name = "test_key_value_operations"
            space_request = {"name": "kv_test_space", "dimension": 3, "metric": "L2"}
            key = "example_key"
            value = "example_value"

            await self.log(test_name, 1, f"Creating space: {space_request['name']}.")
            await self.client.create_space(space_request)

            await self.log(test_name, 2, f"Storing key-value pair: {key}.")
            await self.client.put_key_value("kv_test_space", key, value)

            await self.log(test_name, 3, f"Retrieving key-value pair: {key}.")
            retrieved_value = await self.client.get_key_value("kv_test_space", key)
            self.assertEqual(retrieved_value, "\"example_value\"")

            await self.log(test_name, 4, f"Deleting key-value pair: {key}.")
            await self.client.delete_key_value("kv_test_space", key)

            await self.log(test_name, 5, f"Deleting space: {space_request['name']}.")
            await self.client.delete_space("kv_test_space")

        self.loop.run_until_complete(test())