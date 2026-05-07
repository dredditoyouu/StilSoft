using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;

namespace StilsoftIRS.Tests
{
    [TestClass]
    public class ReportServiceTests
    {
        [TestMethod]
        public void BuildReport_ComputesKeyAggregates()
        {
            var incidents = new FakeIncidentRepository();
            incidents.Incidents.Add(new Incident
            {
                Id = 1,
                Title = "Incident 1",
                Priority = SystemConstants.CriticalPriority,
                CategoryName = "Фишинг",
                StatusName = SystemConstants.ClosedStatus,
                CreatedAt = new DateTime(2026, 4, 1, 8, 0, 0),
                ClosedAt = new DateTime(2026, 4, 1, 10, 0, 0)
            });
            incidents.Incidents.Add(new Incident
            {
                Id = 2,
                Title = "Incident 2",
                Priority = SystemConstants.HighPriority,
                CategoryName = "Вредоносное ПО",
                StatusName = SystemConstants.EscalatedStatus,
                CreatedAt = new DateTime(2026, 4, 2, 8, 0, 0)
            });

            var logs = new FakeEventLogRepository();
            logs.Entries.Add(new EventLogEntry { Id = 1, IncidentId = 1, Action = "Смена статуса", OccurredAt = new DateTime(2026, 4, 1, 8, 15, 0) });
            logs.Entries.Add(new EventLogEntry { Id = 2, IncidentId = 2, Action = "Эскалация", OccurredAt = new DateTime(2026, 4, 2, 8, 10, 0) });

            var service = new ReportService(incidents, logs);
            var report = service.BuildReport(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));

            Assert.AreEqual(2, report.TotalIncidents);
            Assert.AreEqual(1, report.EscalatedCount);
            Assert.AreEqual(2, report.PriorityBreakdown.Count);
            Assert.AreEqual(2, report.CategoryBreakdown.Count);
            Assert.IsTrue(report.AverageReactionTime.HasValue);
            Assert.IsTrue(report.AverageClosureTime.HasValue);
        }
    }
}
