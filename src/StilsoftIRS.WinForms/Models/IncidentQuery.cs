using System;

namespace StilsoftIRS.Models
{
    internal sealed class IncidentQuery
    {
        public int? StatusId { get; set; }

        public int? CategoryId { get; set; }

        public string Priority { get; set; }

        public string SearchText { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }
    }
}
