using System;
using System.Collections.Generic;
using StilsoftIRS.Models;

namespace StilsoftIRS.Views
{
    internal interface IIncidentDetailView
    {
        int IncidentId { get; }
        string StatusComment { get; }
        string ResourceComment { get; }
        event EventHandler LoadRequested;
        event EventHandler ChangeStatusRequested;
        event EventHandler EscalateRequested;
        event EventHandler AssignResourceRequested;
        event EventHandler ReleaseResourceRequested;
        void DisplayIncident(Incident incident);
        void BindAllowedStatuses(IList<string> statuses, bool canEscalate);
        void BindAssignedResources(IList<IncidentResource> resources);
        void BindAvailableResources(IList<ResponseResource> resources);
        void BindEventLog(IList<EventLogEntry> entries);
        void SetOperationsEnabled(bool canOperate, bool hasStatuses, bool canEscalate);
        string GetSelectedTargetStatus();
        int? GetSelectedAvailableResourceId();
        int? GetSelectedAssignedResourceId();
        void ClearStatusComment();
        void ClearResourceComment();
        void CloseView();
        void ShowError(string title, string message);
        void ShowAccessDenied(string message);
    }
}
