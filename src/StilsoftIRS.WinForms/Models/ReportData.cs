using System;
using System.Collections.Generic;

namespace StilsoftIRS.Models
{
    internal sealed class ReportData
    {
        public ReportData()
        {
            Incidents = new List<Incident>();
            PriorityBreakdown = new List<ReportMetric>();
            CategoryBreakdown = new List<ReportMetric>();
        }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public int TotalIncidents { get; set; }

        public int EscalatedCount { get; set; }

        public TimeSpan? AverageReactionTime { get; set; }

        public TimeSpan? AverageClosureTime { get; set; }

        public IList<Incident> Incidents { get; }

        public IList<ReportMetric> PriorityBreakdown { get; }

        public IList<ReportMetric> CategoryBreakdown { get; }
    }

    internal sealed class ReportMetric
    {
        public string Name { get; set; }

        public int Count { get; set; }
    }
}
