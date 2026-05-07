using System;
using System.Collections.Generic;
using System.Data.Common;
using StilsoftIRS.Models;
using StilsoftIRS.Repositories;

namespace StilsoftIRS.Services
{
    internal sealed class EventLogService
    {
        private readonly IEventLogRepository _eventLogs;

        public EventLogService(IEventLogRepository eventLogs)
        {
            _eventLogs = eventLogs;
        }

        public void AddEntry(
            int? incidentId,
            int userId,
            string action,
            string comment = null,
            DbConnection connection = null,
            DbTransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new InvalidOperationException("Действие для журнала событий обязательно.");
            }

            _eventLogs.Add(
                new EventLogEntry
                {
                    IncidentId = incidentId,
                    UserId = userId,
                    Action = action.Trim(),
                    Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
                },
                connection,
                transaction);
        }

        public IList<EventLogEntry> GetEntries(DateTime? from = null, DateTime? to = null, int? incidentId = null)
        {
            return _eventLogs.GetAll(from, to, incidentId);
        }
    }
}
