import { ASimpleVectorsClient } from "../asimplevectors/src";

async function testKeyValue(client: ASimpleVectorsClient): Promise<void> {
    const spaceName = "spacename";
    const keyName = "test_key";
    const keyValueData = "Hello, ASimpleVectors!";

    // Step 1: Create a key-value pair in the space
    console.log(`Creating key-value pair in '${spaceName}'`);
    await client.putKeyValue(spaceName, keyName, keyValueData);
    console.log(`Key '${keyName}' created successfully.`);

    // Step 2: Retrieve the key-value pair
    console.log(`Retrieving key-value pair for key '${keyName}'`);
    try {
        const response = await client.getKeyValue(spaceName, keyName);
        if (response) {
            console.log(`Key '${keyName}' retrieved successfully:`, response);
        }
    } catch (error) {
        console.error(`Failed to retrieve key '${keyName}':`, error);
    }

    // Step 3: List all keys in the space
    console.log(`Listing all keys in '${spaceName}'`);
    try {
        const response = await client.listKeys(spaceName);
        if (response && response.keys) {
            console.log(`Keys in '${spaceName}':`, response.keys);
        }
    } catch (error) {
        console.error(`Failed to list keys in '${spaceName}':`, error);
    }

    // Step 4: Delete the key-value pair
    console.log(`Deleting key '${keyName}' from '${spaceName}'`);
    try {
        await client.deleteKeyValue(spaceName, keyName);
        console.log(`Key '${keyName}' deleted successfully.`);
    } catch (error) {
        console.error(`Failed to delete key '${keyName}':`, error);
    }

    // Step 5: Verify the key is deleted
    console.log(`Verifying deletion of key '${keyName}'`);
    try {
        // Attempt to delete the key again to confirm its absence
        await client.deleteKeyValue(spaceName, keyName);
        console.error(`Unexpected behavior: Key '${keyName}' was deleted twice.`);
    } catch (error) {
        if (error.response?.status === 400) {
            // Expected behavior: Key does not exist
            console.log(`Key '${keyName}' successfully deleted and verified.`);
        } else {
            // Unexpected error
            console.error(`Failed to verify deletion of key '${keyName}':`, error);
        }
    }
}

async function main(): Promise<void> {
    const client = new ASimpleVectorsClient("localhost", 21001);
    try {
        // Run the key-value storage test
        await testKeyValue(client);
    } finally {
        console.log("Closing the client.");
        // Ensure any necessary cleanup is handled here
    }
}

// Run the example
main().catch((error) => {
    console.error("An error occurred:", error);
});
