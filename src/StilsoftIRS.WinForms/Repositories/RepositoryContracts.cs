using System;
using System.Collections.Generic;
using System.Data.Common;
using StilsoftIRS.Models;

namespace StilsoftIRS.Repositories
{
    internal interface IUserRepository
    {
        User GetByLogin(string login);

        User GetById(int id);

        IList<User> GetAll();

        int Add(User user);

        void Update(User user);
    }

    internal interface ICategoryRepository
    {
        IList<IncidentCategory> GetAll();

        IncidentCategory GetById(int id);

        int Add(IncidentCategory category);

        void Update(IncidentCategory category);

        void Delete(int id);
    }

    internal interface IStatusRepository
    {
        IList<IncidentStatus> GetAll();

        IncidentStatus GetById(int id);

        IncidentStatus GetByName(string name);
    }

    internal interface IIncidentRepository
    {
        IList<Incident> GetAll(IncidentQuery query);

        IList<Incident> GetByCreatedPeriod(DateTime from, DateTime to);

        Incident GetById(int id);

        int Add(Incident incident, DbConnection connection = null, DbTransaction transaction = null);

        void UpdateStatus(int incidentId, int statusId, DateTime? closedAt, DbConnection connection = null, DbTransaction transaction = null);
    }

    internal interface IResourceRepository
    {
        IList<ResponseResource> GetAll();

        IList<ResponseResource> GetAvailable();

        ResponseResource GetById(int id, DbConnection connection = null, DbTransaction transaction = null);

        int Add(ResponseResource resource);

        void Update(ResponseResource resource);

        void Delete(int id);

        void SetAvailability(int resourceId, bool isAvailable, DbConnection connection = null, DbTransaction transaction = null);
    }

    internal interface IIncidentResourceRepository
    {
        IList<IncidentResource> GetByIncidentId(int incidentId);

        bool Exists(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null);

        int Add(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null);

        void Delete(int incidentId, int resourceId, DbConnection connection = null, DbTransaction transaction = null);
    }

    internal interface IEventLogRepository
    {
        int Add(EventLogEntry entry, DbConnection connection = null, DbTransaction transaction = null);

        IList<EventLogEntry> GetAll(DateTime? from = null, DateTime? to = null, int? incidentId = null);
    }
}
