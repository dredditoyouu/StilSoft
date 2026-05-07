using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using StilsoftIRS.Models;
using StilsoftIRS.Repositories;

namespace StilsoftIRS.Services
{
    internal sealed class ResourceService
    {
        private readonly IResourceRepository _resources;
        private readonly IIncidentResourceRepository _incidentResources;
        private readonly IIncidentRepository _incidents;
        private readonly EventLogService _eventLogService;
        private readonly Func<DbConnection> _connectionFactory;

        public ResourceService(
            IResourceRepository resources,
            IIncidentResourceRepository incidentResources,
            IIncidentRepository incidents,
            EventLogService eventLogService,
            Func<DbConnection> connectionFactory = null)
        {
            _resources = resources;
            _incidentResources = incidentResources;
            _incidents = incidents;
            _eventLogService = eventLogService;
            _connectionFactory = connectionFactory ?? Infrastructure.DbConnectionFactory.CreateConnection;
        }

        public IList<ResponseResource> GetResources()
        {
            return _resources.GetAll();
        }

        public IList<ResponseResource> GetAvailableResources()
        {
            return _resources.GetAvailable();
        }

        public IList<IncidentResource> GetAssignedResources(int incidentId)
        {
            return _incidentResources.GetByIncidentId(incidentId);
        }

        public void SaveResource(ResponseResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (string.IsNullOrWhiteSpace(resource.Name))
            {
                throw new InvalidOperationException("Наименование ресурса обязательно.");
            }

            resource.Name = resource.Name.Trim();
            resource.ResourceType = string.IsNullOrWhiteSpace(resource.ResourceType) ? null : resource.ResourceType.Trim();
            resource.Responsible = string.IsNullOrWhiteSpace(resource.Responsible) ? null : resource.Responsible.Trim();

            if (resource.Id == 0)
            {
                resource.Id = _resources.Add(resource);
                return;
            }

            _resources.Update(resource);
        }

        public void DeleteResource(int id)
        {
            _resources.Delete(id);
        }

        public void AssignResource(int incidentId, int resourceId, int actingUserId, string comment = null)
        {
            var incident = _incidents.GetById(incidentId);
            if (incident == null)
            {
                throw new InvalidOperationException("Инцидент не найден.");
            }

            using (var connection = _connectionFactory())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var resource = _resources.GetById(resourceId, connection, transaction);
                        if (resource == null)
                        {
                            throw new InvalidOperationException("Ресурс не найден.");
                        }

                        if (!resource.IsAvailable)
                        {
                            throw new InvalidOperationException("Ресурс уже занят и не может быть назначен.");
                        }

                        if (_incidentResources.Exists(incidentId, resourceId, connection, transaction))
                        {
                            throw new InvalidOperationException("Ресурс уже назначен на этот инцидент.");
                        }

                        _incidentResources.Add(incidentId, resourceId, connection, transaction);
                        _resources.SetAvailability(resourceId, false, connection, transaction);
                        _eventLogService.AddEntry(
                            incidentId,
                            actingUserId,
                            "Назначение ресурса",
                            string.IsNullOrWhiteSpace(comment)
                                ? "Назначен ресурс: " + resource.Name
                                : "Назначен ресурс: " + resource.Name + ". " + comment.Trim(),
                            connection,
                            transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ReleaseResource(int incidentId, int resourceId, int actingUserId, string comment = null)
        {
            using (var connection = _connectionFactory())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var resource = _resources.GetById(resourceId, connection, transaction);
                        if (resource == null)
                        {
                            throw new InvalidOperationException("Ресурс не найден.");
                        }

                        if (!_incidentResources.Exists(incidentId, resourceId, connection, transaction))
                        {
                            throw new InvalidOperationException("Ресурс не назначен на данный инцидент.");
                        }

                        _incidentResources.Delete(incidentId, resourceId, connection, transaction);
                        _resources.SetAvailability(resourceId, true, connection, transaction);
                        _eventLogService.AddEntry(
                            incidentId,
                            actingUserId,
                            "Снятие ресурса",
                            string.IsNullOrWhiteSpace(comment)
                                ? "Освобождён ресурс: " + resource.Name
                                : "Освобождён ресурс: " + resource.Name + ". " + comment.Trim(),
                            connection,
                            transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
