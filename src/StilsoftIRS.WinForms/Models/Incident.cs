using System;

namespace StilsoftIRS.Models
{
    internal sealed class Incident
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string Priority { get; set; }

        public int CategoryId { get; set; }

        public int StatusId { get; set; }

        public int OperatorId { get; set; }

        public string CategoryName { get; set; }

        public string StatusName { get; set; }

        public string StatusColorHex { get; set; }

        public string OperatorName { get; set; }
    }
}
