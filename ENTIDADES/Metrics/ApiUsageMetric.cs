using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ENTIDADES.Metrics
{
    public class ApiUsageMetric
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int? UserId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? QueryString { get; set; }
        public double ResponseTimeMs { get; set; }
        public bool IsSlowRequest { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ApplicationName { get; set; } = "POS3";
    }
}
