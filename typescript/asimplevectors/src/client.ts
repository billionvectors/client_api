import axios, { AxiosInstance } from "axios";
import * as fs from "fs";
import * as path from "path";
const FormData = require("form-data");

class ASimpleVectorsClient {
  private baseUrl: string;
  private axiosInstance: AxiosInstance;

  /**
   * Initializes the asimplevectorsClient.
   * 
   * @param host - The hostname or IP address of the asimplevectors server.
   * @param port - The port number for the API (default: 21001).
   * @param useSsl - Boolean indicating whether to use HTTPS. Defaults to false.
   * @param token - Optional Bearer token for authorization.
   */
  constructor(
    host: string,
    port: number = 21001,
    useSsl: boolean = false,
    token?: string
  ) {
    const scheme = useSsl ? "https" : "http";
    const baseHost = `${scheme}://${host}:${port}`;
    
    this.baseUrl = `${baseHost}`;
  
    this.axiosInstance = axios.create({
      baseURL: this.baseUrl, // Default base URL
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    });
  }
  
  /**
   * Sets or updates the Authorization token.
   * 
   * @param token - The Bearer token for authentication.
   */
  setToken(token: string) {
    this.axiosInstance.defaults.headers["Authorization"] = `Bearer ${token}`;
  }

  /**
   * Initializes the cluster as a single-node cluster.
   */
  async initCluster(): Promise<void> {
    await this.axiosInstance.post("/cluster/init", {});
  }
  
  /**
   * Adds a learner node to the cluster.
   * 
   * @param nodeId - ID of the learner node.
   * @param apiAddr - API address of the learner node.
   * @param rpcAddr - RPC address of the learner node.
   */
  async addLearner(nodeId: number, apiAddr: string, rpcAddr: string): Promise<void> {
    await this.axiosInstance.post("/cluster/add-learner", [nodeId, apiAddr, rpcAddr]);
  }

  /**
   * Changes the cluster membership.
   * 
   * @param membership - List of node IDs for the cluster membership.
   */
  async changeMembership(membership: number[]): Promise<void> {
    await this.axiosInstance.post("/cluster/change-membership", membership);
  }

  /**
   * Retrieves cluster metrics.
   * 
   * @returns Cluster metrics data.
   */
  async getClusterMetrics(): Promise<any> {
    const response = await this.axiosInstance.get(`/cluster/metrics`);
    const data = response.data;
  
    if (!data || !data.Ok) {
      throw new Error("Unexpected response format: missing 'Ok' field");
    }
  
    return data.Ok;
  }

  /**
   * Creates a new space in the database.
   * 
   * @param spaceRequest - Space configuration details.
   */
  async createSpace(spaceRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post("/api/space", spaceRequest);
  }

  /**
   * Retrieves details of a space.
   * 
   * @param spaceName - Name of the space.
   * @returns Space details.
   */
  async getSpace(spaceName: string): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}`);
    return response.data;
  }

  /**
   * Updates the configuration of an existing space.
   * 
   * @param spaceName - Name of the space.
   * @param spaceData - Updated configuration details.
   */
  async updateSpace(spaceName: string, spaceData: Record<string, any>): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}`, spaceData);
  }

  /**
   * Deletes a space from the database.
   * 
   * @param spaceName - Name of the space to delete.
   */
  async deleteSpace(spaceName: string): Promise<void> {
    await this.axiosInstance.delete(`/api/space/${spaceName}`);
  }

  /**
   * Lists all spaces available in the database.
   * 
   * @returns List of spaces.
   */
  async listSpaces(): Promise<any> {
    const response = await this.axiosInstance.get("/api/spaces");
    return response.data;
  }

  /**
   * Creates a new version for the specified space.
   * 
   * @param spaceName - Name of the space.
   * @param versionRequest - Version configuration details.
   */
  async createVersion(spaceName: string, versionRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}/version`, versionRequest);
  }

  /**
   * Lists all versions for the specified space.
   * 
   * @param spaceName - Name of the space.
   * @param start - Optional start index for pagination.
   * @param limit - Optional limit on the number of results.
   * @returns List of versions.
   */
  async listVersions(spaceName: string, start: number = 0, limit: number = 100): Promise<any> {
    const params = { start, limit };
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/versions`, { params });
    return response.data;
  }

  /**
   * Retrieves details of a version by its ID.
   * 
   * @param spaceName - Name of the space.
   * @param versionId - ID of the version.
   * @returns Version details.
   */
  async getVersionById(spaceName: string, versionId: number): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/version/${versionId}`);
    return response.data;
  }

  /**
   * Retrieves the default version for the specified space.
   * 
   * @param spaceName - Name of the space.
   * @returns Default version details.
   */
  async getDefaultVersion(spaceName: string): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/version`);
    return response.data;
  }

  /**
   * Deletes a specific version from a space.
   * 
   * @param spaceName - Name of the space.
   * @param versionId - ID of the version to delete.
   */
  async deleteVersion(spaceName: string, versionId: number): Promise<void> {
    await this.axiosInstance.delete(`/api/space/${spaceName}/version/${versionId}`);
  }

  /**
   * Upserts vectors into the specified space.
   * 
   * @param spaceName - Name of the space.
   * @param vectorRequest - Vector data to upsert.
   */
  async upsertVector(spaceName: string, vectorRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}/vector`, vectorRequest);
  }

  /**
   * Retrieves vectors from a specific version of a space.
   * 
   * @param spaceName - Name of the space.
   * @param versionId - ID of the version to retrieve vectors from.
   * @param start - Optional start index for pagination.
   * @param limit - Optional limit for the number of vectors to retrieve.
   * @param filter - Optional filter for the query.
   * @returns List of vectors from the specified version.
   */
  async getVectorsByVersion(spaceName: string, versionId: number, start?: number, limit?: number, filter?: string): Promise<any> {
    const params: any = { start, limit };
    if (filter) {
      params.filter = filter;
    }
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/version/${versionId}/vectors`, { params });
    return response.data;
  }

  /**
   * Searches for nearest neighbors to a vector within a space.
   * 
   * @param spaceName - Name of the space.
   * @param searchRequest - Search query details.
   * @returns Search results.
   */
  async searchVector(spaceName: string, searchRequest: Record<string, any>): Promise<any> {
    const response = await this.axiosInstance.post(`/api/space/${spaceName}/search`, searchRequest);
    return response.data;
  }

  /**
   * Searches for nearest neighbors to a vector within a specific version of a space.
   * 
   * @param spaceName - Name of the space.
   * @param versionId - ID of the version to search in.
   * @param searchRequest - Search query details.
   * @returns Search results from the specified version.
   */
  async searchVectorByVersion(spaceName: string, versionId: number, searchRequest: Record<string, any>): Promise<any> {
    const response = await this.axiosInstance.post(
      `/api/space/${spaceName}/version/${versionId}/search`,
      searchRequest
    );
    return response.data;
  }

  /**
   * Performs reranking of search results using BM25.
   * 
   * @param spaceName - Name of the space.
   * @param rerankRequest - Rerank query details.
   * @returns Rerank results.
   */
  async rerank(
    spaceName: string,
    rerankRequest: Record<string, any>
  ): Promise<any> {
    const response = await this.axiosInstance.post(
      `/api/space/${spaceName}/rerank`,
      rerankRequest
    );
    return response.data;
  }

  /**
   * Performs reranking of search results for a specific version using BM25.
   * 
   * @param spaceName - Name of the space.
   * @param versionId - ID of the version to rerank in.
   * @param rerankRequest - Rerank query details.
   * @returns Rerank results from the specified version.
   */
  async rerankByVersion(
    spaceName: string,
    versionId: number,
    rerankRequest: Record<string, any>
  ): Promise<any> {
    const response = await this.axiosInstance.post(
      `/api/space/${spaceName}/version/${versionId}/rerank`,
      rerankRequest
    );
    return response.data;
  }

  /**
   * Creates a snapshot of the database.
   * 
   * @param snapshotRequest - Configuration details for the snapshot.
   */
  async createSnapshot(snapshotRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post("/api/snapshot", snapshotRequest);
  }

  /**
   * Lists all available snapshots.
   * 
   * @returns List of snapshots.
   */
  async listSnapshots(): Promise<any> {
    const response = await this.axiosInstance.get("/api/snapshots");
    return response.data;
  }

  /**
   * Deletes a specific snapshot.
   * 
   * @param snapshotDate - Date of the snapshot to delete.
   */
  async deleteSnapshot(snapshotDate: string): Promise<void> {
    await this.axiosInstance.delete(`/api/snapshot/${snapshotDate}/delete`);
  }

  /**
   * Downloads a snapshot to a specified folder.
   * 
   * @param snapshotDate - Snapshot date.
   * @param downloadFolder - Folder path for saving the snapshot.
   * @returns Path to the downloaded snapshot.
   */
  async downloadSnapshot(snapshotDate: string, downloadFolder: string): Promise<string> {
    // Ensure the target folder exists
    if (!fs.existsSync(downloadFolder)) {
      fs.mkdirSync(downloadFolder, { recursive: true });
      console.log(`Created download folder: ${downloadFolder}`);
    }
  
    // Define the file path for the downloaded snapshot
    const filePath = path.join(downloadFolder, `snapshot-${snapshotDate}.zip`);
  
    try {
      const response = await this.axiosInstance.get(`/snapshot/${snapshotDate}/download`, {
        responseType: "stream",
      });
  
      // Save the downloaded file to the target path
      const writer = fs.createWriteStream(filePath);
      response.data.pipe(writer);
  
      // Wait for the file to finish writing
      await new Promise<void>((resolve, reject) => {
        writer.on("finish", resolve);
        writer.on("error", reject);
      });
  
      console.log(`Snapshot downloaded successfully to ${filePath}`);
      return filePath;
    } catch (error) {
      console.error(`Failed to download snapshot: ${error}`);
      throw error;
    }
  }

  /**
   * Restores a specific snapshot by its date.
   * 
   * @param snapshotDate - Date of the snapshot to restore.
   */
  async restoreSnapshot(snapshotDate: string): Promise<void> {
    await this.axiosInstance.post(`/api/snapshot/${snapshotDate}/restore`, {});
  }

  /**
   * Uploads a snapshot file to the server and restores it.
   * 
   * @param filePath - Path to the snapshot file to upload.
   */
  async uploadRestoreSnapshot(filePath: string): Promise<void> {
    const url = `/api/snapshots/restore`;
  
    const formData = new FormData();
    formData.append("file", fs.createReadStream(filePath));
  
    await this.axiosInstance.post(url, formData, {
      headers: formData.getHeaders(),
    });
  }

  /**
   * Creates a new RBAC (Role-Based Access Control) token.
   * 
   * @param rbacRequest - Details for creating the RBAC token.
   */
  async createRbacToken(rbacRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post("/api/security/tokens", rbacRequest);
  }

  /**
   * Lists all existing RBAC tokens.
   * 
   * @returns List of RBAC tokens.
   */
  async listRbacTokens(): Promise<any> {
    const response = await this.axiosInstance.get("/api/security/tokens");
    return response.data;
  }

  /**
   * Deletes an RBAC token.
   * 
   * @param token - The token value to delete.
   */
  async deleteRbacToken(token: string): Promise<void> {
    await this.axiosInstance.delete(`/api/security/tokens/${token}`);
  }

  /**
   * Updates an existing RBAC token.
   * 
   * @param token - The token value to update.
   * @param rbacRequest - Updated details for the RBAC token.
   */
  async updateRbacToken(token: string, rbacRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.put(`/api/security/tokens/${token}`, rbacRequest);
  }

  /**
   * Stores or updates a key-value pair in a space.
   * 
   * @param spaceName - Name of the space.
   * @param key - Key to store or update.
   * @param value - Value associated with the key.
   */
  async putKeyValue(spaceName: string, key: string, value: string): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}/key/${key}`, { value });
  }  

  /**
   * Retrieves the value of a specific key in a space.
   * 
   * @param spaceName - Name of the space.
   * @param key - Key to retrieve.
   * @returns Value associated with the key.
   */
  async getKeyValue(spaceName: string, key: string): Promise<string> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/key/${key}`);
    return response.data.value;
  }
  
  /**
   * Lists all keys in a space.
   * 
   * @param spaceName - Name of the space.
   * @param start - Optional start index for pagination.
   * @param limit - Optional limit on the number of results.
   * @returns List of keys in the space.
   */
  async listKeys(spaceName: string, start: number = 0, limit: number = 100): Promise<any> {
    const params = { start, limit };
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/keys`, { params });
    return response.data;
  }

  /**
   * Deletes a key-value pair from a space.
   * 
   * @param spaceName - Name of the space.
   * @param key - Key to delete.
   */
  async deleteKeyValue(spaceName: string, key: string): Promise<void> {
    await this.axiosInstance.delete(`/api/space/${spaceName}/key/${key}`);
  }
}

export default ASimpleVectorsClient;
