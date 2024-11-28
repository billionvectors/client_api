from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field, ConfigDict, root_validator

# Unified Response Models
class SuccessResponse(BaseModel):
    result: str = "success"

class ErrorResponse(BaseModel):
    error: str
    
# Cluster DTOs
class ClusterVote(BaseModel):
    leader_id: Dict[str, int]
    committed: bool

class MembershipConfig(BaseModel):
    log_id: Optional[Dict[str, Any]] = None
    membership: Dict[str, Any] = {}

class ClusterMetricsResponse(BaseModel):
    running_state: Optional[Dict[str, Optional[Any]]] = {}
    id: int
    current_term: int
    vote: Optional[ClusterVote] = None
    last_log_index: Optional[int] = None
    last_applied: Optional[Dict[str, Any]] = None
    snapshot: Optional[Any] = None
    purged: Optional[Any] = None
    state: str
    current_leader: Optional[int] = None
    millis_since_quorum_ack: Optional[int] = None
    last_quorum_acked: Optional[int] = None
    membership_config: MembershipConfig
    heartbeat: Optional[Dict[str, int]] = {}
    replication: Optional[Dict[str, Any]] = {}

    @root_validator(pre=True)
    def handle_null_values(cls, values):
        """
        Ensure all 'None' values are handled correctly.
        """
        for key, value in values.items():
            if value is None:
                values[key] = None  # Ensure None values are retained
        return values

    @classmethod
    def from_response(cls, response_json: dict) -> "ClusterMetricsResponse":
        """
        Convert the API response JSON into a ClusterMetricsResponse model.
        This method ensures `None` values are handled correctly.
        """
        if "Ok" in response_json:
            metrics = response_json["Ok"]
            return cls(**metrics)
        raise ValueError("Invalid response format, 'Ok' key not found")

# Space DTOs
class HnswConfig(BaseModel):
    EfConstruct: Optional[int] = None
    M: Optional[int] = None

class QuantizationConfig(BaseModel):
    Product: Optional["ProductQuantizationConfig"] = None
    Scalar: Optional["ScalarQuantizationConfig"] = None

class ScalarQuantizationConfig(BaseModel):
    Type: Optional[str] = "f32"

class ProductQuantizationConfig(BaseModel):
    Compression: Optional[str] = "none"


class DenseConfig(BaseModel):
    dimension: Optional[int] = None
    metric: Optional[str] = None
    hnsw_config: Optional[HnswConfig] = None
    quantization_config: Optional[QuantizationConfig] = None

class SparseConfig(BaseModel):
    metric: Optional[str] = None

class SpaceRequest(BaseModel):
    name: str
    dimension: Optional[int] = None
    metric: Optional[str] = None
    hnsw_config: Optional[HnswConfig] = None
    quantization_config: Optional[QuantizationConfig] = None
    dense: Optional[DenseConfig] = None
    sparse: Optional[SparseConfig] = None
    indexes: Optional[Any] = None

class SpaceResponse(BaseModel):
    created_time_utc: int
    name: str
    spaceId: int
    updated_time_utc: int
    version: "VersionData"

class VersionData(BaseModel):
    vectorIndices: List["VectorIndexData"]
    versionId: int

class VectorIndexData(BaseModel):
    created_time_utc: int
    dimension: int
    hnswConfig: "HnswConfig"
    is_default: bool
    metricType: int
    name: str
    quantizationConfig: Optional["QuantizationConfig"] = None  # Allow null or missing
    updated_time_utc: int
    vectorIndexId: int
    vectorValueType: int

    # Add a method to handle default values for missing fields
    def ensure_defaults(self):
        if self.quantizationConfig is None:
            self.quantizationConfig = QuantizationConfig()

class ListSpacesResponse(BaseModel):
    values: List["SpaceInfo"]

class SpaceInfo(BaseModel):
    name: str
    id: int
    description: str
    created_time_utc: int
    updated_time_utc: int

# Version DTOs
class VersionRequest(BaseModel):
    name: str
    description: Optional[str] = None
    tag: Optional[str] = None
    is_default: Optional[bool] = None

class VersionResponse(BaseModel):
    id: int
    created_time_utc: int
    description: Optional[str] = None
    is_default: bool
    name: str
    tag: Optional[str] = None
    updated_time_utc: int

class ListVersionsResponse(BaseModel):
    values: List["VersionInfo"]


class VersionInfo(BaseModel):
    id: int
    name: str
    description: Optional[str] = None
    is_default: bool
    tag: Optional[str] = None

# Vector DTOs
class VectorData(BaseModel):
    id: int
    data: List[float]
    metadata: Any  # Adjust type as needed


class VectorRequest(BaseModel):
    vectors: List[VectorData]


class VectorResponse(BaseModel):
    result: str


class VectorDataResponse(BaseModel):
    id: int
    data: List[float]
    metadata: Any  # Adjust type as needed
    
class GetVectorsResponse(BaseModel):
    vectors: List[VectorDataResponse]

    @classmethod
    def from_response(cls, response_json: dict) -> "GetVectorsResponse":
        """
        Custom method to parse and transform the response JSON into GetVectorsResponse.
        """
        if "vectors" in response_json:
            # Flatten the nested 'data' field for each vector
            response_json["vectors"] = [
                {
                    **vector,
                    "data": vector["data"]["data"]
                }
                for vector in response_json["vectors"]
            ]
        return cls(**response_json)
        
# Search DTOs
class SearchRequest(BaseModel):
    vector: List[float]


class SearchResponse(BaseModel):
    distance: float
    label: int


# Snapshot DTOs
class CreateSnapshotRequest(BaseModel):
    spacename: str


class SnapshotResponse(BaseModel):
    result: str


class ListSnapshotsResponse(BaseModel):
    snapshots: List["SnapshotInfo"]


class SnapshotInfo(BaseModel):
    file_name: str


# Security DTOs
class RbacTokenRequest(BaseModel):
    space_id: int
    system: int
    space: int
    version: int
    vector: int
    snapshot: int
    security: int
    keyvalue: int


class RbacTokenResponse(BaseModel):
    result: str
    token: str

class TokenDetails(BaseModel):
    id: int
    space_id: int
    token: str
    expire_time_utc: int
    system: int
    space: int
    version: int
    vector: int
    search: int
    snapshot: int
    security: int
    keyvalue: int

class ListRbacTokensResponse(BaseModel):
    tokens: List[TokenDetails]

    @classmethod
    def from_response(cls, response_json: List[Dict]) -> "ListRbacTokensResponse":
        """
        Convert the API response into the ListRbacTokensResponse format.
        """
        return cls(tokens=[TokenDetails(**token) for token in response_json])

# Key-Value DTOs
class KeyValueRequest(BaseModel):
    text: str


class KeyValueResponse(BaseModel):
    result: str


class ListKeysResponse(BaseModel):
    keys: List[str]


# Error Responses
class SpaceErrorResponse(BaseModel):
    error: str


class VersionErrorResponse(BaseModel):
    error: str


class VectorErrorResponse(BaseModel):
    error: str


class SearchErrorResponse(BaseModel):
    error: str


class SnapshotErrorResponse(BaseModel):
    error: str


class RbacTokenErrorResponse(BaseModel):
    error: str


class KeyValueErrorResponse(BaseModel):
    error: str


# Update forward references
SpaceResponse.update_forward_refs()
VersionData.update_forward_refs()
VectorIndexData.update_forward_refs()
ListSpacesResponse.update_forward_refs()
SpaceInfo.update_forward_refs()
ListVersionsResponse.update_forward_refs()
VersionInfo.update_forward_refs()
GetVectorsResponse.update_forward_refs()
VectorDataResponse.update_forward_refs()
ListSnapshotsResponse.update_forward_refs()
SnapshotInfo.update_forward_refs()
ListRbacTokensResponse.update_forward_refs()
TokenDetails.update_forward_refs()
