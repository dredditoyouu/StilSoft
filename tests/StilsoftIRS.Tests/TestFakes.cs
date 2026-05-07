using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Repositories;

namespace StilsoftIRS.Tests
{
    internal sealed class FakeDbConnection : DbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;

        public override string ConnectionString { get; set; }

        public override string Database => "Fake";

        public override string DataSource => "Fake";

        public override string ServerVersion => "1.0";

        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName)
        {
        }

        public override void Close()
        {
            _state = ConnectionState.Closed;
        }

        public override void Open()
        {
            _state = ConnectionState.Open;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new FakeDbTransaction(this);
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotSupportedException();
        }
    }

    internal sealed class FakeDbTransaction : DbTransaction
    {
        private readonly DbConnection _connection;

        public FakeDbTransaction(DbConnection connection)
        {
            _connection = connection;
        }

        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

        protected override DbConnection DbConnection => _connection;

        public override void Commit()
        {
        }

        public override void Rollback()
        {
        }
    }

    internal sealed class FakeUserRepository : IUserRepository
    {
        public readonly List<User> Users = new List<User>();

        public User GetByLogin(string login) => Users.FirstOrDefault(item => item.Login == login);

        public User GetById(int id) => Users.FirstOrDefault(item => item.Id == id);

        public IList<User> GetAll() => Users.ToList();

        public int Add(User user)
        {
            user.Id = Users.Count == 0 ? 1 : Users.Max(item => item.Id) + 1;
            Users.Add(user);
            return user.Id;
        }

        public void Update(User user)
        {
            var index = Users.FindIndex(item => item.Id == user.Id);
            Users[index] = user;
        }
    }

    internal sealed class FakeStatusRepository : IStatusRepository
    {
        public readonly List<IncidentStatus> Statuses = new List<IncidentStatus>();

        public IList<IncidentStatus> GetAll() => Statuses.ToList();

        public IncidentStatus GetById(int id) => Statuses.FirstOrDefault(item => item.Id == id);

        public IncidentStatus GetByName(string name) => Statuses.FirstOrDefault(item => item.Name == name);
    }

    internal sealed class FakeIncidentRepository : IIncidentRepository
    {
        public readonly List<Incident> Incidents = new List<Incident>();

        public IList<Incident> GetAll(IncidentQuery query)
        {
            IEnumerable<Incident> items = Incidents;
            if (query != null)
            {
                if (query.StatusId.HasValue)
                {
                    items = items.Where(item => item.StatusId == query.StatusId.Value);
                }

                if (query.CategoryId.HasValue)
                {
                    items = items.Where(item => item.CategoryId == query.CategoryId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.Priority))
                {
                    items = items.Where(item => item.Priority == query.Priority);
                }

                if (query.CreatedFrom.HasValue)
                {
                    items = items.Where(item => item.CreatedAt >= query.CreatedFrom.Value);
                }

                if (query.CreatedTo.HasValue)
                {
                    items = items.Where(item => item.CreatedAt <= query.CreatedTo.Value);
                }
            }

            return items.ToList();
        }

        public IList<Incident> GetByCreatedPeriod(DateTime from, DateTime to)
        {
            return Incidents.Where(item => item.CreatedAt >= from && item.CreatedAt <= to).ToList();
        }

        public Incident GetById(int id) => Incidents.FirstOrDefault(item => item.Id == id);

        public int Add(Incident incident, DbConnection connection = null, DbTransaction transaction = null)
        {
            incident.Id = Incidents.Count == 0 ? 1 : Incidents.Max(item => item.Id) + 1;
            incident.CreatedAt = incident.CreatedAt == default(DateTime) ? DateTime.Now : incident.CreatedAt;
            Incidents.Add(incident);
            return incident.Id;
        }

        public void UpdateStatus(int incidentId, int statusId, DateTime? closedAt, DbConnection connection = null, DbTransaction transaction = null)
        {
            var incident = GetById(incidentId);
            incident.StatusId = statusId;
            incident.ClosedAt = closedAt;
        }
    }

    internal sealed class FakeResourceRepository : IResourceRepository
    {
        public readonly List<ResponseResource> Resources = new List<ResponseResource>();

        public IList<ResponseResource> GetAll() => Resources.ToList();

        public IList<ResponseResource> GetAvailable() => Resources.Where(item => item.IsAvailable).ToList();

        public ResponseResource GetById(int id, DbConnection connection = null, DbTransaction transaction = null)
            => Resources.FirstOrDefault(item => item.Id == id);

        public int Add(ResponseResource resource)
        {
            resource.Id = Resources.Count == 0 ? 1 : Resources.Max(item => item.Id) + 1;
            Resources.Add(resource);
            return resource.Id;
        }

        public void Update(ResponseResource resource)
        {
            var index = Resources.FindIndex(item => item.Id == resource.Id);
            Resources[index] = resource;
        }

        public void Delete(int id)
        {
            Resources.RemoveAll(item => item.Id == id);
        }

        public void SetAvailability(int resourceId, bool isAvailable, DbConnection connection = null, DbTransaction transaction = null)
        {
            var resource = GetById(resourceId);
            resource.IsAvailable = isAvailable;
        }
    }

    internal sealed class FakeIncidentResourceRepository : IIncidentResourceRepository
    {
        public readonly List<IncidentResource> Assignments = new List<IncidentResource>();

        public IList<IncidentResource> GetByIncidentId(int incidentId)
        {
            return Assignments.Where(item => item.IncidentId == incidentId).ToList();
        }

        public bool Exists(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null)
        {
            return Assignments.Any(item => item.IncidentId == incidentId && item.ResourceId == resourceId);
        }

        public int Add(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null)
        {
            var item = new IncidentResource
            {
                Id = Assignments.Count == 0 ? 1 : Assignments.Max(entry => entry.Id) + 1,
                IncidentId = incidentId,
                ResourceId = resourceId,
                AssignedAt = DateTime.Now
            };
            Assignments.Add(item);
            return item.Id;
        }

        public void Delete(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null)
        {
            Assignments.RemoveAll(item => item.IncidentId == incidentId && item.ResourceId == resourceId);
        }
    }

    internal sealed class FakeEventLogRepository : IEventLogRepository
    {
        public readonly List<EventLogEntry> Entries = new List<EventLogEntry>();

        public int Add(EventLogEntry entry, DbConnection connection = null, DbTransaction transaction = null)
        {
            entry.Id = Entries.Count == 0 ? 1 : Entries.Max(item => item.Id) + 1;
            entry.OccurredAt = entry.OccurredAt == default(DateTime) ? DateTime.Now : entry.OccurredAt;
            Entries.Add(entry);
            return entry.Id;
        }

        public IList<EventLogEntry> GetAll(DateTime? from = null, DateTime? to = null, int? incidentId = null)
        {
            IEnumerable<EventLogEntry> items = Entries;
            if (from.HasValue)
            {
                items = items.Where(item => item.OccurredAt >= from.Value);
            }

            if (to.HasValue)
            {
                items = items.Where(item => item.OccurredAt <= to.Value);
            }

            if (incidentId.HasValue)
            {
                items = items.Where(item => item.IncidentId == incidentId.Value);
            }

            return items.ToList();
        }
    }
}
