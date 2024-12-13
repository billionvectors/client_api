﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class ClusterVote
    {
        public LeaderId LeaderId { get; set; }
        public bool Committed { get; set; }
    }

    public class LeaderId
    {
        public int Term { get; set; }
        public int NodeId { get; set; }
    }

    public class MembershipConfig
    {
        public LogId LogId { get; set; }
        public Membership Membership { get; set; }
    }

    public class LogId
    {
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
        public string RpcAddr { get; set; }
        public string ApiAddr { get; set; }
    }

    public class ClusterMetricsResponse
    {
        public string State { get; set; }
        public int Id { get; set; }
        public int CurrentTerm { get; set; }
        public ClusterVote Vote { get; set; }
        public int? LastLogIndex { get; set; }
        public int? LastQuorumAcked { get; set; }
        public MembershipConfig MembershipConfig { get; set; }
        public Dictionary<int, long> Heartbeat { get; set; }
    }
}