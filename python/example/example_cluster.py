import asyncio
from asimplevectors.client import ASimpleVectorsClient

# this test should be executed with 3 clusters
async def test_cluster_operations():
    client = ASimpleVectorsClient(host="localhost", port=21001)

    try:
        # Initialize the cluster
        print("Initializing the cluster...")
        await client.init_cluster()

        # Fetch cluster metrics
        print("Fetching cluster metrics...")
        metrics = await client.get_cluster_metrics()
        print("Metrics:\n", metrics)

        # Add learners
        print("Adding node 2 as a learner...")
        await client.add_learner(2, "127.0.0.1:21002", "127.0.0.1:22002")
        print("Adding node 3 as a learner...")
        await client.add_learner(3, "127.0.0.1:21003", "127.0.0.1:22003")

        # Change membership
        print("Changing cluster membership...")
        await client.change_membership([1, 2, 3])
        print("Membership changed successfully.")

        # Fetch updated metrics
        print("Fetching updated cluster metrics...")
        updated_metrics = await client.get_cluster_metrics()
        print("Updated Metrics:\n", updated_metrics)

    finally:
        # Ensure the client is properly closed
        await client.close()

if __name__ == "__main__":
    asyncio.run(test_cluster_operations())
