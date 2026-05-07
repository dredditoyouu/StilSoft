using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;

namespace StilsoftIRS.Tests
{
    [TestClass]
    public class IncidentServiceTests
    {
        [TestMethod]
        public void CanTransition_ReturnsTrue_ForAllowedTransition()
        {
            var service = new IncidentService(new FakeIncidentRepository(), new FakeStatusRepository(), new EventLogService(new FakeEventLogRepository()));

            Assert.IsTrue(service.CanTransition(SystemConstants.NewStatus, SystemConstants.InProgressStatus));
        }

        [TestMethod]
        public void CanTransition_ReturnsFalse_ForForbiddenTransition()
        {
            var service = new IncidentService(new FakeIncidentRepository(), new FakeStatusRepository(), new EventLogService(new FakeEventLogRepository()));

            Assert.IsFalse(service.CanTransition(SystemConstants.NewStatus, SystemConstants.ClosedStatus));
        }

        [TestMethod]
        public void ChangeStatus_SetsClosedAt_WhenIncidentClosed()
        {
            var incidents = new FakeIncidentRepository();
            incidents.Incidents.Add(new Incident
            {
                Id = 11,
                StatusId = 4,
                StatusName = SystemConstants.ResolvedStatus,
                Title = "Incident",
                CreatedAt = DateTime.Today
            });

            var statuses = new FakeStatusRepository();
            statuses.Statuses.Add(new IncidentStatus { Id = 5, Name = SystemConstants.ClosedStatus });

            var eventLogs = new FakeEventLogRepository();
            var service = new IncidentService(incidents, statuses, new EventLogService(eventLogs), () => new FakeDbConnection());

            service.ChangeStatus(11, SystemConstants.ClosedStatus, 1, "Закрытие тестом");

            Assert.AreEqual(5, incidents.GetById(11).StatusId);
            Assert.IsTrue(incidents.GetById(11).ClosedAt.HasValue);
            Assert.IsTrue(eventLogs.Entries.Count >= 2);
        }
    }
}
