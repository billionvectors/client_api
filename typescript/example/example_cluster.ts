import { ASimpleVectorsClient } from "../asimplevectors/src";

async function testClusterOperations() {
    const client = new ASimpleVectorsClient("localhost", 21001);

    try {
        // Initialize the cluster
        console.log("Initializing the cluster...");
        await client.initCluster();

        // Fetch cluster metrics
        console.log("Fetching cluster metrics...");
        const metrics = await client.getClusterMetrics();
        console.log("Metrics:\n", metrics);

        // Add learners
        console.log("Adding node 2 as a learner...");
        await client.addLearner(2, "127.0.0.1:21002", "127.0.0.1:22002");
        console.log("Adding node 3 as a learner...");
        await client.addLearner(3, "127.0.0.1:21003", "127.0.0.1:22003");

        // Change membership
        console.log("Changing cluster membership...");
        await client.changeMembership([1, 2, 3]);
        console.log("Membership changed successfully.");

        // Fetch updated metrics
        console.log("Fetching updated cluster metrics...");
        const updatedMetrics = await client.getClusterMetrics();
        console.log("Updated Metrics:\n", updatedMetrics);

    } catch (error) {
        console.error("An error occurred:", error);
    } finally {
        // Ensure the client is properly closed if necessary
        console.log("Cluster operations completed.");
    }
}

// Run the cluster operations
testClusterOperations().catch((error) => {
    console.error("Unhandled error:", error);
});
