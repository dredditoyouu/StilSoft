using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;

namespace StilsoftIRS.Tests
{
    [TestClass]
    public class ResourceServiceTests
    {
        [TestMethod]
        public void AssignResource_MarksResourceBusyAndCreatesLink()
        {
            var resources = new FakeResourceRepository();
            resources.Resources.Add(new ResponseResource { Id = 3, Name = "Forensics Kit", IsAvailable = true });

            var incidents = new FakeIncidentRepository();
            incidents.Incidents.Add(new Incident { Id = 7, Title = "Incident", StatusName = SystemConstants.InProgressStatus, CreatedAt = DateTime.Today });

            var assignments = new FakeIncidentResourceRepository();
            var logs = new FakeEventLogRepository();
            var service = new ResourceService(resources, assignments, incidents, new EventLogService(logs), () => new FakeDbConnection());

            service.AssignResource(7, 3, 1, "Тестовое назначение");

            Assert.IsFalse(resources.GetById(3).IsAvailable);
            Assert.IsTrue(assignments.Exists(7, 3));
            Assert.AreEqual(1, logs.Entries.Count);
        }

        [TestMethod]
        public void AssignResource_Throws_WhenResourceAlreadyBusy()
        {
            var resources = new FakeResourceRepository();
            resources.Resources.Add(new ResponseResource { Id = 3, Name = "Forensics Kit", IsAvailable = false });

            var incidents = new FakeIncidentRepository();
            incidents.Incidents.Add(new Incident { Id = 7, Title = "Incident", StatusName = SystemConstants.InProgressStatus, CreatedAt = DateTime.Today });

            var service = new ResourceService(resources, new FakeIncidentResourceRepository(), incidents, new EventLogService(new FakeEventLogRepository()), () => new FakeDbConnection());

            Assert.ThrowsException<InvalidOperationException>(() => service.AssignResource(7, 3, 1));
        }
    }
}
