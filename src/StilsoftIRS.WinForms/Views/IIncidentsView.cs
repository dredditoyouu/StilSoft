using System;
using System.Collections.Generic;
using StilsoftIRS.Models;

namespace StilsoftIRS.Views
{
    internal interface IIncidentsView
    {
        int? SelectedStatusId { get; }
        int? SelectedCategoryId { get; }
        string SelectedPriority { get; }
        string SearchText { get; }
        event EventHandler LoadRequested;
        event EventHandler ResetRequested;
        event EventHandler CreateIncidentRequested;
        event EventHandler OpenSelectedRequested;
        event EventHandler RefreshRequested;
        void BindStatuses(IList<IncidentStatus> items);
        void BindCategories(IList<IncidentCategory> items);
        void BindPriorities(IList<string> items);
        void BindIncidents(IList<Incident> items);
        void SetCreateVisible(bool visible);
        void ResetFilters();
        int? GetSelectedIncidentId();
        NewIncidentArgs ShowCreateDialog(IList<IncidentCategory> categories);
        void NavigateToIncident(int incidentId);
        void ShowError(string title, string message);
    }

    internal sealed class NewIncidentArgs
    {
        public string Title;
        public string Description;
        public string Priority;
        public int CategoryId;
    }
}
