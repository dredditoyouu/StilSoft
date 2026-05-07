using System;
using System.Collections.Generic;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class IncidentsPresenter
    {
        private readonly IIncidentsView _view;
        private readonly AppServices _services;

        public IncidentsPresenter(IIncidentsView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.LoadRequested += (s, e) => Load();
            _view.RefreshRequested += (s, e) => LoadIncidents();
            _view.ResetRequested += (s, e) => { _view.ResetFilters(); LoadIncidents(); };
            _view.CreateIncidentRequested += (s, e) => CreateIncident();
            _view.OpenSelectedRequested += (s, e) => OpenSelected();
        }

        public void Load()
        {
            LoadLookups();
            _view.SetCreateVisible(UserService.IsInRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.OperatorRole));
            LoadIncidents();
        }

        private void LoadLookups()
        {
            var statuses = new List<IncidentStatus>(_services.Statuses.GetAll());
            statuses.Insert(0, new IncidentStatus { Id = 0, Name = "Все статусы" });
            _view.BindStatuses(statuses);

            var categories = new List<IncidentCategory>(_services.Categories.GetAll());
            categories.Insert(0, new IncidentCategory { Id = 0, Name = "Все категории" });
            _view.BindCategories(categories);

            var priorities = new List<string> { "Все приоритеты" };
            priorities.AddRange(SystemConstants.Priorities);
            _view.BindPriorities(priorities);
        }

        public void LoadIncidents()
        {
            try
            {
                var query = new IncidentQuery
                {
                    StatusId = _view.SelectedStatusId,
                    CategoryId = _view.SelectedCategoryId,
                    Priority = _view.SelectedPriority,
                    SearchText = _view.SearchText
                };
                _view.BindIncidents(_services.IncidentService.GetIncidents(query));
            }
            catch (Exception ex) { _view.ShowError("Инциденты", ex.Message); }
        }

        private void CreateIncident()
        {
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.OperatorRole);
                var args = _view.ShowCreateDialog(_services.Categories.GetAll());
                if (args == null) return;
                var currentUser = SessionContext.CurrentUser;
                var id = _services.IncidentService.CreateIncident(args.Title, args.Description, args.Priority, args.CategoryId, currentUser.Id, currentUser.Id);
                LoadIncidents();
                _view.NavigateToIncident(id);
            }
            catch (UnauthorizedAccessException ex) { _view.ShowError("Доступ запрещён", ex.Message); }
            catch (Exception ex) { _view.ShowError("Создание инцидента", ex.Message); }
        }

        private void OpenSelected()
        {
            var id = _view.GetSelectedIncidentId();
            if (id.HasValue) _view.NavigateToIncident(id.Value);
        }
    }
}
