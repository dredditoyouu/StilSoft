using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Repositories;

namespace StilsoftIRS.Services
{
    internal sealed class IncidentService
    {
        private static readonly IDictionary<string, string[]> Transitions =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { SystemConstants.NewStatus, new[] { SystemConstants.InProgressStatus, SystemConstants.EscalatedStatus } },
                { SystemConstants.InProgressStatus, new[] { SystemConstants.ResolvedStatus, SystemConstants.EscalatedStatus } },
                { SystemConstants.EscalatedStatus, new[] { SystemConstants.InProgressStatus, SystemConstants.ResolvedStatus } },
                { SystemConstants.ResolvedStatus, new[] { SystemConstants.ClosedStatus } },
                { SystemConstants.ClosedStatus, Array.Empty<string>() }
            };

        private readonly IIncidentRepository _incidents;
        private readonly IStatusRepository _statuses;
        private readonly EventLogService _eventLogService;
        private readonly Func<DbConnection> _connectionFactory;

        public IncidentService(
            IIncidentRepository incidents,
            IStatusRepository statuses,
            EventLogService eventLogService,
            Func<DbConnection> connectionFactory = null)
        {
            _incidents = incidents;
            _statuses = statuses;
            _eventLogService = eventLogService;
            _connectionFactory = connectionFactory ?? DbConnectionFactory.CreateConnection;
        }

        public IList<Incident> GetIncidents(IncidentQuery query)
        {
            return _incidents.GetAll(query);
        }

        public Incident GetIncident(int id)
        {
            return _incidents.GetById(id);
        }

        public IList<IncidentStatus> GetStatuses()
        {
            return _statuses.GetAll();
        }

        public bool CanTransition(string fromStatusName, string toStatusName)
        {
            return !string.IsNullOrWhiteSpace(fromStatusName) &&
                   !string.IsNullOrWhiteSpace(toStatusName) &&
                   Transitions.TryGetValue(fromStatusName, out var allowed) &&
                   Array.Exists(allowed, item => string.Equals(item, toStatusName, StringComparison.Ordinal));
        }

        public IList<string> GetAllowedTransitions(string fromStatusName)
        {
            return Transitions.TryGetValue(fromStatusName ?? string.Empty, out var allowed)
                ? new List<string>(allowed)
                : new List<string>();
        }

        public int CreateIncident(string title, string description, string priority, int categoryId, int operatorId, int actingUserId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new InvalidOperationException("Заголовок инцидента обязателен.");
            }

            if (Array.IndexOf(SystemConstants.Priorities, priority) < 0)
            {
                throw new InvalidOperationException("Указан недопустимый приоритет инцидента.");
            }

            var newStatus = _statuses.GetByName(SystemConstants.NewStatus);
            if (newStatus == null)
            {
                throw new InvalidOperationException("В базе данных отсутствует статус 'Новый'.");
            }

            using (var connection = _connectionFactory())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var incidentId = _incidents.Add(
                            new Incident
                            {
                                Title = title.Trim(),
                                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                                Priority = priority,
                                CategoryId = categoryId,
                                StatusId = newStatus.Id,
                                OperatorId = operatorId
                            },
                            connection,
                            transaction);

                        _eventLogService.AddEntry(
                            incidentId,
                            actingUserId,
                            "Создание инцидента",
                            "Инцидент зарегистрирован в системе.",
                            connection,
                            transaction);

                        transaction.Commit();
                        return incidentId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ChangeStatus(int incidentId, string targetStatusName, int actingUserId, string comment = null)
        {
            var incident = _incidents.GetById(incidentId);
            if (incident == null)
            {
                throw new InvalidOperationException("Инцидент не найден.");
            }

            var targetStatus = _statuses.GetByName(targetStatusName);
            if (targetStatus == null)
            {
                throw new InvalidOperationException("Целевой статус не найден.");
            }

            if (!CanTransition(incident.StatusName, targetStatus.Name))
            {
                throw new InvalidOperationException(
                    $"Переход из статуса '{incident.StatusName}' в статус '{targetStatus.Name}' запрещён.");
            }

            var closedAt = string.Equals(targetStatus.Name, SystemConstants.ClosedStatus, StringComparison.Ordinal)
                ? DateTime.Now
                : (DateTime?)null;
            var details = string.Format("{0} -> {1}", incident.StatusName, targetStatus.Name);
            var transitionComment = string.IsNullOrWhiteSpace(comment) ? details : details + ". " + comment.Trim();

            using (var connection = _connectionFactory())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        _incidents.UpdateStatus(incidentId, targetStatus.Id, closedAt, connection, transaction);

                        _eventLogService.AddEntry(
                            incidentId,
                            actingUserId,
                            "Смена статуса",
                            transitionComment,
                            connection,
                            transaction);

                        if (string.Equals(targetStatus.Name, SystemConstants.EscalatedStatus, StringComparison.Ordinal))
                        {
                            _eventLogService.AddEntry(
                                incidentId,
                                actingUserId,
                                "Эскалация",
                                string.IsNullOrWhiteSpace(comment) ? "Инцидент эскалирован." : comment.Trim(),
                                connection,
                                transaction);
                        }

                        if (string.Equals(targetStatus.Name, SystemConstants.ClosedStatus, StringComparison.Ordinal))
                        {
                            _eventLogService.AddEntry(
                                incidentId,
                                actingUserId,
                                "Закрытие инцидента",
                                string.IsNullOrWhiteSpace(comment) ? "Инцидент закрыт." : comment.Trim(),
                                connection,
                                transaction);
                        }

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
