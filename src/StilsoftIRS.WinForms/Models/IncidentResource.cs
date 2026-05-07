using System;

namespace StilsoftIRS.Models
{
    internal sealed class IncidentResource
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }

        public int ResourceId { get; set; }

        public string ResourceName { get; set; }

        public string ResourceType { get; set; }

        public string Responsible { get; set; }

        public DateTime AssignedAt { get; set; }
    }
}
