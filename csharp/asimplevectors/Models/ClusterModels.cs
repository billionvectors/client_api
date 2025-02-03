using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace asimplevectors.Models
{
    public class ClusterVote
    {
        [JsonPropertyName("leader_id")]
        public LeaderId LeaderId { get; set; }
        public bool Committed { get; set; }
    }

    public class LeaderId
    {
        public int Term { get; set; }

        [JsonPropertyName("node_id")]
        public int NodeId { get; set; }
    }

    public class MembershipConfig
    {
        [JsonPropertyName("log_id")]
        public LogId LogId { get; set; }
        public Membership Membership { get; set; }
    }

    public class LogId
    {
        [JsonPropertyName("leader_id")]
        public LeaderId LeaderId { get; set; }
        public int Index { get; set; }
    }

    public class Membership
    {
        public int[][] Configs { get; set; }
        public Dictionary<int, Node> Nodes { get; set; }
    }

    public class Node
    {
        [JsonPropertyName("rpc_addr")]
        public string RpcAddr { get; set; }
        [JsonPropertyName("api_addr")]
        public string ApiAddr { get; set; }
    }

    public class ClusterMetricsResponse
    {
        [JsonPropertyName("running_state")]
        public RunningState RunningState { get; set; }
        public int Id { get; set; }
        [JsonPropertyName("current_term")]
        public int CurrentTerm { get; set; }
        public ClusterVote Vote { get; set; }
        [JsonPropertyName("last_log_index")]
        public int? LastLogIndex { get; set; }
        [JsonPropertyName("last_applied")]
        public LogId LastApplied { get; set; }
        public object Snapshot { get; set; }
        public object Purged { get; set; }
        public string State { get; set; }
        [JsonPropertyName("current_leader")]
        public int CurrentLeader { get; set; }
        [JsonPropertyName("millis_since_quorum_ack")]
        public long MillisSinceQuorumAck { get; set; }
        [JsonPropertyName("last_quorum_acked")]
        public long LastQuorumAcked { get; set; }
        [JsonPropertyName("membership_config")]
        public MembershipConfig MembershipConfig { get; set; }
        public Dictionary<int, long> Heartbeat { get; set; }
        public Dictionary<int, LogId> Replication { get; set; }

        public override string ToString()
        {
            return $"State: {State}, NodeCount: {MembershipConfig?.Membership?.Nodes?.Count ?? 0}, " +
                   $"IndexCount: {LastLogIndex ?? 0}, ReplicationCount: {Replication?.Count ?? 0}";
        }
    }

    public class RunningState
    {
        public string Ok { get; set; }
    }
}
