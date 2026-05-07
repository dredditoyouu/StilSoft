using System;

namespace StilsoftIRS.Models
{
    internal sealed class EventLogEntry
    {
        public int Id { get; set; }

        public int? IncidentId { get; set; }

        public int UserId { get; set; }

        public string Action { get; set; }

        public string Comment { get; set; }

        public DateTime OccurredAt { get; set; }

        public string UserName { get; set; }

        public string IncidentTitle { get; set; }
    }
}
