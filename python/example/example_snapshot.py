import asyncio
from asimplevectors.client import ASimpleVectorsClient
import os
import json
import aiohttp


async def example_snapshot(client: ASimpleVectorsClient):
    try:
        # Step 1: Create space 'spacename'
        print("Creating space 'spacename'")
        create_space_data = {
            "name": "spacename",
            "dimension": 4,
            "metric": "L2",
            "hnsw_config": {
                "M": 16,
                "ef_construct": 100
            }
        }
        await client.create_space(create_space_data)
        print("Space 'spacename' created successfully.\n")

        # Step 2: Upsert vectors
        print("Upserting vectors to 'spacename'")
        upsert_data = {
            "vectors": [
                {"id": 1, "data": [0.1, 0.2, 0.3, 0.4], "metadata": {"label": "first"}},
                {"id": 2, "data": [0.5, 0.6, 0.7, 0.8], "metadata": {"label": "second"}},
                {"id": 3, "data": [0.9, 0.8, 0.7, 0.6], "metadata": {"label": "third"}},
                {"id": 4, "data": [1.0, 0.1, 0.2, 0.3], "metadata": {"label": "fourth"}},
                {"id": 5, "data": [0.2, 0.3, 0.4, 0.3], "metadata": {"label": "fifth"}}
            ]
        }
        await client.upsert_vector("spacename", upsert_data)
        print("Vectors upserted successfully.\n")

        # Step 3: Create snapshot
        print("Creating snapshot for 'spacename'")
        snapshot_request = {"spacename": "spacename"}
        await client.create_snapshot(snapshot_request)
        print("Snapshot created successfully.\n")

        # Step 4: List snapshots
        print("Listing snapshots")
        snapshots_response = await client.list_snapshots()
        if snapshots_response and snapshots_response.snapshots:
            first_snapshot = snapshots_response.snapshots[0]
            print(f"Snapshots listed: {json.dumps(snapshots_response.dict(), indent=2)}")
            snapshot_filename = first_snapshot.file_name
        else:
            print("No snapshots found.")
            return

        # Get the first snapshot's filename and extract the date
        snapshot_date = snapshot_filename.split('-')[1].split('.')[0]
        print(f"Snapshot filename: {snapshot_filename}, date: {snapshot_date}")

        # Step 5: Download snapshot
        print("Downloading snapshot")
        download_path = await client.download_snapshot(snapshot_date, "temp")
        print(f"Snapshot downloaded to: {download_path}")

        # Step 6: Upsert modified vectors
        print("Upserting modified vectors")
        modified_vector_data = {
            "vectors": [
                {"id": 1, "data": [1.0, 1.0, 1.0, 1.0], "metadata": {"label": "modified"}}
            ]
        }
        await client.upsert_vector("spacename", modified_vector_data)
        print("Modified vectors upserted successfully.\n")

        # Step 7: Search vectors (before restore)
        print("Searching vectors (before restore)")
        search_request = {"vector": [0.1, 0.2, 0.3, 0.4]}
        search_response = await client.search_vector("spacename", search_request)
        print(f"Search response (before restore): {search_response}\n")

        # Step 8: Restore snapshot
        print("Restoring snapshot")
        await client.restore_snapshot(snapshot_date)
        print("Snapshot restored successfully.")

        # Step 9: Search vectors (after restore)
        print("Searching vectors (after restore)")
        search_response_after = await client.search_vector("spacename", search_request)
        print(f"Search response (after restore): {search_response_after}\n")

        # Step 10: Delete snapshot
        print(f"Deleting snapshot with date: {snapshot_date}")
        await client.delete_snapshot(snapshot_date)
        print("Snapshot deleted successfully.")

        # Verify snapshot deletion
        print("Verifying snapshot deletion")
        snapshots_after_deletion = await client.list_snapshots()
        if snapshots_after_deletion and not snapshots_after_deletion.snapshots:
            print("No snapshots found after deletion.")
        else:
            print(f"Snapshots still exist: {snapshots_after_deletion}")

        # Step 11: Restore snapshot from downloaded file
        print("Restoring snapshot from downloaded zip file")
        zip_file_path = os.path.join("temp", f"snapshot-{snapshot_date}.zip")
        if os.path.exists(zip_file_path):
            await client.upload_restore_snapshot(zip_file_path)
            print("Snapshot restored from uploaded file successfully.")
        else:
            print(f"Snapshot file not found: {zip_file_path}")
            
    except Exception as e:
        print(f"An error occurred: {e}")


async def main():
    client = ASimpleVectorsClient(host="localhost")
    try:
        await example_snapshot(client)
    finally:
        await client.close()


if __name__ == "__main__":
    asyncio.run(main())
