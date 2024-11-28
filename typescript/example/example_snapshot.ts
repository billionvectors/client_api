import * as fs from "fs";
import * as path from "path";
import { ASimpleVectorsClient } from "../asimplevectors/src";

async function exampleSnapshot(client: ASimpleVectorsClient): Promise<void> {
  try {
    // Step 1: Create space 'spacename'
    console.log("Creating space 'spacename'");
    const createSpaceData = {
      name: "spacename",
      dimension: 4,
      metric: "L2",
      hnsw_config: {
        M: 16,
        ef_construct: 100,
      },
    };
    await client.createSpace(createSpaceData);
    console.log("Space 'spacename' created successfully.\n");

    // Step 2: Upsert vectors
    console.log("Upserting vectors to 'spacename'");
    const upsertData = {
      vectors: [
        { id: 1, data: [0.1, 0.2, 0.3, 0.4], metadata: { label: "first" } },
        { id: 2, data: [0.5, 0.6, 0.7, 0.8], metadata: { label: "second" } },
        { id: 3, data: [0.9, 0.8, 0.7, 0.6], metadata: { label: "third" } },
        { id: 4, data: [1.0, 0.1, 0.2, 0.3], metadata: { label: "fourth" } },
        { id: 5, data: [0.2, 0.3, 0.4, 0.3], metadata: { label: "fifth" } },
      ],
    };
    await client.createVector("spacename", upsertData);
    console.log("Vectors upserted successfully.\n");

    // Step 3: Create snapshot
    console.log("Creating snapshot for 'spacename'");
    const snapshotRequest = { spacename: "spacename" };
    await client.createSnapshot(snapshotRequest);
    console.log("Snapshot created successfully.\n");

    // Step 4: List snapshots
    console.log("Listing snapshots");
    const snapshotsResponse = await client.listSnapshots();
    if (snapshotsResponse && snapshotsResponse.snapshots) {
      const firstSnapshot = snapshotsResponse.snapshots[0];
      console.log(
        `Snapshots listed: ${JSON.stringify(snapshotsResponse, null, 2)}`
      );
      const snapshotFilename = firstSnapshot.file_name;

      // Get the first snapshot's filename and extract the date
      const snapshotDate = snapshotFilename.split("-")[1].split(".")[0];
      console.log(`Snapshot filename: ${snapshotFilename}, date: ${snapshotDate}`);

      // Step 5: Download snapshot
      console.log("Downloading snapshot");
      const downloadPath = await client.downloadSnapshot(snapshotDate, "temp");
      console.log(`Snapshot downloaded to: ${downloadPath}`);

      // Step 6: Upsert modified vectors
      console.log("Upserting modified vectors");
      const modifiedVectorData = {
        vectors: [
          { id: 1, data: [1.0, 1.0, 1.0, 1.0], metadata: { label: "modified" } },
        ],
      };
      await client.createVector("spacename", modifiedVectorData);
      console.log("Modified vectors upserted successfully.\n");

      // Step 7: Search vectors (before restore)
      console.log("Searching vectors (before restore)");
      const searchRequest = { vector: [0.1, 0.2, 0.3, 0.4] };
      let searchResponse = await client.searchVector("spacename", searchRequest);
      console.log(`Search response (before restore): ${JSON.stringify(searchResponse)}\n`);

      // Step 8: Restore snapshot
      console.log("Restoring snapshot");
      await client.restoreSnapshot(snapshotDate);
      console.log("Snapshot restored successfully.");

      // Step 9: Search vectors (after restore)
      console.log("Searching vectors (after restore)");
      searchResponse = await client.searchVector("spacename", searchRequest);
      console.log(`Search response (after restore): ${JSON.stringify(searchResponse)}\n`);

      // Step 10: Delete snapshot
      console.log(`Deleting snapshot with date: ${snapshotDate}`);
      await client.deleteSnapshot(snapshotDate);
      console.log("Snapshot deleted successfully.");

      // Verify snapshot deletion
      console.log("Verifying snapshot deletion");
      const snapshotsAfterDeletion = await client.listSnapshots();
      if (snapshotsAfterDeletion && snapshotsAfterDeletion.snapshots.length === 0) {
        console.log("No snapshots found after deletion.");
      } else {
        console.log(
          `Snapshots still exist: ${JSON.stringify(snapshotsAfterDeletion)}`
        );
      }

      // Step 11: Restore snapshot from downloaded file
      console.log("Restoring snapshot from downloaded zip file");
      const zipFilePath = path.join("temp", `snapshot-${snapshotDate}.zip`);
      if (fs.existsSync(zipFilePath)) {
        await client.uploadRestoreSnapshot(zipFilePath);
        console.log("Snapshot restored from uploaded file successfully.");
      } else {
        console.log(`Snapshot file not found: ${zipFilePath}`);
      }
    } else {
      console.log("No snapshots found.");
    }
  } catch (error) {
    console.error(`An error occurred: ${error}`);
  }
}

async function main(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);
  try {
    await exampleSnapshot(client);
  } finally {
    console.log("Closing client...");
  }
}

main().catch((error) => {
  console.error("An unexpected error occurred:", error);
});
