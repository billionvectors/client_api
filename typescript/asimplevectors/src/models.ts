export interface ClusterMetricsResponse {
    id: number;
    state: string;
    running_state?: any;
    current_term: number;
    vote?: any;
    last_log_index?: number;
    last_applied?: any;
    snapshot?: any;
    purged?: any;
    current_leader?: number;
    millis_since_quorum_ack?: number;
    last_quorum_acked?: number;
    membership_config: MembershipConfig;
    heartbeat?: Record<string, number>;
    replication?: Record<string, any>;
  }
  
  export interface MembershipConfig {
    log_id?: any;
    membership: Record<string, any>;
  }
  
  export interface SpaceResponse {
    created_time_utc: number;
    name: string;
    spaceId: number;
    updated_time_utc: number;
    version: VersionData;
  }
  
  export interface VersionData {
    vectorIndices: VectorIndexData[];
    versionId: number;
  }
  
  export interface VectorIndexData {
    created_time_utc: number;
    dimension: number;
    hnswConfig?: any;
    is_default: boolean;
    metricType: number;
    name: string;
    quantizationConfig?: any;
    updated_time_utc: number;
    vectorIndexId: number;
    vectorValueType: number;
  }
  
  export interface ListSpacesResponse {
    values: SpaceInfo[];
  }
  
  export interface SpaceInfo {
    id: number;
    name: string;
    description: string;
    created_time_utc: number;
    updated_time_utc: number;
  }
  
  export interface SearchResponse {
    distance: number;
    label: number;
  }

  export interface RerankRequest {
    vector: number[];
    tokens: string[];
    top_k: number;
  }

  export interface RerankResponse {
    vectorUniqueId: number;
    distance: number;
    bm25Score: number;
  }
  
  export interface VersionResponse {
    id: number;
    name: string;
    description?: string;
    is_default: boolean;
    tag?: string;
    created_time_utc: number;
    updated_time_utc: number;
  }
  
  export interface ListVersionsResponse {
    total_count: number;
    values: VersionResponse[];
  }
  
  export interface VectorRequest {
    vectors: VectorData[];
  }
  
  export interface VectorData {
    id: number;
    data: number[];
    metadata: any; // Use specific type for metadata if known
    doc?: string; // Optional property for document text
    doc_tokens?: string[]; // Optional property for document tokens
  }
  
  export interface VectorResponse {
    result: string;
  }
  
  export interface SnapshotResponse {
    result: string;
  }
  
  export interface ListSnapshotsResponse {
    snapshots: SnapshotInfo[];
  }
  
  export interface SnapshotInfo {
    file_name: string;
    date: string;
  }
  
  export interface RbacTokenRequest {
    space_id: number;
    system: number;
    space: number;
    version: number;
    vector: number;
    snapshot: number;
    security: number;
    keyvalue: number;
  }
  
  export interface RbacTokenResponse {
    result: string;
    token: string;
  }
  
  export interface ListRbacTokensResponse {
    tokens: TokenDetails[];
  }
  
  export interface TokenDetails {
    id: number;
    space_id: number;
    token: string;
    expire_time_utc: number;
    system: number;
    space: number;
    version: number;
    vector: number;
    snapshot: number;
    security: number;
    keyvalue: number;
  }
  
  export interface KeyValueResponse {
    result: string;
  }
  
  export interface ListKeysResponse {
    total_count: number;
    keys: string[];
  }
  
  export interface ErrorResponse {
    error: string;
  }
