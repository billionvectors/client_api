import axios, { AxiosInstance } from "axios";
import FormData from "form-data"; 
import * as fs from "fs";
import * as path from "path";

class ASimpleVectorsClient {
  private baseUrl: string;
  private axiosInstance: AxiosInstance;

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
  

  setToken(token: string) {
    this.axiosInstance.defaults.headers["Authorization"] = `Bearer ${token}`;
  }

  async initCluster(): Promise<void> {
    await this.axiosInstance.post("/cluster/init", {});
  }
  
  async addLearner(nodeId: number, apiAddr: string, rpcAddr: string): Promise<void> {
    await this.axiosInstance.post("/cluster/add-learner", [nodeId, apiAddr, rpcAddr]);
  }

  async changeMembership(membership: number[]): Promise<void> {
    await this.axiosInstance.post("/cluster/change-membership", membership);
  }

  async getClusterMetrics(): Promise<any> {
    const response = await this.axiosInstance.get(`/cluster/metrics`);
    const data = response.data;
  
    if (!data || !data.Ok) {
      throw new Error("Unexpected response format: missing 'Ok' field");
    }
  
    return data.Ok;
  }

  async createSpace(spaceRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post("/api/space", spaceRequest);
  }

  async getSpace(spaceName: string): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}`);
    return response.data;
  }

  async updateSpace(spaceName: string, spaceData: Record<string, any>): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}`, spaceData);
  }

  async deleteSpace(spaceName: string): Promise<void> {
    await this.axiosInstance.delete(`/api/space/${spaceName}`);
  }

  async listSpaces(): Promise<any> {
    const response = await this.axiosInstance.get("/api/spaces");
    return response.data;
  }

  async createVersion(spaceName: string, versionRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}/version`, versionRequest);
  }

  async listVersions(spaceName: string): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/versions`);
    return response.data;
  }

  async getVersionById(spaceName: string, versionId: number): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/version/${versionId}`);
    return response.data;
  }

  async getDefaultVersion(spaceName: string): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/version`);
    return response.data;
  }

  async createVector(spaceName: string, vectorRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}/vector`, vectorRequest);
  }

  async getVectorsByVersion(spaceName: string, versionId: number, start?: number, limit?: number): Promise<any> {
    const params = { start, limit };
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/version/${versionId}/vectors`, { params });
    return response.data;
  }

  async searchVector(spaceName: string, searchRequest: Record<string, any>): Promise<any> {
    const response = await this.axiosInstance.post(`/api/space/${spaceName}/search`, searchRequest);
    return response.data;
  }

  async searchVectorByVersion(spaceName: string, versionId: number, searchRequest: Record<string, any>): Promise<any> {
    const response = await this.axiosInstance.post(
      `/api/space/${spaceName}/version/${versionId}/search`,
      searchRequest
    );
    return response.data;
  }

  async createSnapshot(snapshotRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post("/api/snapshot", snapshotRequest);
  }

  async listSnapshots(): Promise<any> {
    const response = await this.axiosInstance.get("/api/snapshots");
    return response.data;
  }

  async deleteSnapshot(snapshotDate: string): Promise<void> {
    await this.axiosInstance.delete(`/api/snapshot/${snapshotDate}/delete`);
  }

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

  async restoreSnapshot(snapshotDate: string): Promise<void> {
    await this.axiosInstance.post(`/api/snapshot/${snapshotDate}/restore`, {});
  }

  async uploadRestoreSnapshot(filePath: string): Promise<void> {
    const url = `/api/snapshots/restore`;
  
    const formData = new FormData();
    formData.append("file", fs.createReadStream(filePath));
  
    await this.axiosInstance.post(url, formData, {
      headers: formData.getHeaders(),
    });
  }

  async createRbacToken(rbacRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.post("/api/security/tokens", rbacRequest);
  }

  async listRbacTokens(): Promise<any> {
    const response = await this.axiosInstance.get("/api/security/tokens");
    return response.data;
  }

  async deleteRbacToken(token: string): Promise<void> {
    await this.axiosInstance.delete(`/api/security/tokens/${token}`);
  }

  async updateRbacToken(token: string, rbacRequest: Record<string, any>): Promise<void> {
    await this.axiosInstance.put(`/api/security/tokens/${token}`, rbacRequest);
  }

  async putKeyValue(spaceName: string, key: string, value: string): Promise<void> {
    await this.axiosInstance.post(`/api/space/${spaceName}/key/${key}`, { value });
  }  

  async getKeyValue(spaceName: string, key: string): Promise<string> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/key/${key}`);
    return response.data.value;
  }
  
  async listKeys(spaceName: string): Promise<any> {
    const response = await this.axiosInstance.get(`/api/space/${spaceName}/keys`);
    return response.data;
  }

  async deleteKeyValue(spaceName: string, key: string): Promise<void> {
    await this.axiosInstance.delete(`/api/space/${spaceName}/key/${key}`);
  }
}

export default ASimpleVectorsClient;
