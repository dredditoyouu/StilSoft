using System;
using System.Collections.Generic;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class IncidentDetailPresenter
    {
        private readonly IIncidentDetailView _view;
        private readonly AppServices _services;

        public IncidentDetailPresenter(IIncidentDetailView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.LoadRequested += (s, e) => LoadAll();
            _view.ChangeStatusRequested += (s, e) => ChangeSelectedStatus();
            _view.EscalateRequested += (s, e) => ChangeStatus(SystemConstants.EscalatedStatus);
            _view.AssignResourceRequested += (s, e) => AssignResource();
            _view.ReleaseResourceRequested += (s, e) => ReleaseResource();
        }

        public void LoadAll()
        {
            try
            {
                var incident = _services.IncidentService.GetIncident(_view.IncidentId);
                if (incident == null)
                {
                    _view.ShowError("Инцидент", "Инцидент не найден.");
                    _view.CloseView();
                    return;
                }
                _view.DisplayIncident(incident);
                var transitions = new List<string>(_services.IncidentService.GetAllowedTransitions(incident.StatusName));
                bool canEscalate = transitions.Contains(SystemConstants.EscalatedStatus);
                bool canOperate = UserService.IsInRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.OperatorRole);
                _view.BindAllowedStatuses(transitions, canEscalate);
                _view.BindAssignedResources(_services.ResourceService.GetAssignedResources(_view.IncidentId));
                _view.BindAvailableResources(_services.ResourceService.GetAvailableResources());
                _view.BindEventLog(_services.EventLogService.GetEntries(incidentId: _view.IncidentId));
                _view.SetOperationsEnabled(canOperate, transitions.Count > 0, canEscalate);
            }
            catch (Exception ex) { _view.ShowError("Инцидент", ex.Message); }
        }

        private void ChangeSelectedStatus()
        {
            var status = _view.GetSelectedTargetStatus();
            if (status == null) return;
            ChangeStatus(status);
        }

        private void ChangeStatus(string targetStatus)
        {
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.OperatorRole);
                _services.IncidentService.ChangeStatus(_view.IncidentId, targetStatus, SessionContext.CurrentUser.Id, _view.StatusComment);
                _view.ClearStatusComment();
                LoadAll();
            }
            catch (UnauthorizedAccessException ex) { _view.ShowAccessDenied(ex.Message); }
            catch (Exception ex) { _view.ShowError("Статус", ex.Message); }
        }

        private void AssignResource()
        {
            var resourceId = _view.GetSelectedAvailableResourceId();
            if (!resourceId.HasValue) return;
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.OperatorRole);
                _services.ResourceService.AssignResource(_view.IncidentId, resourceId.Value, SessionContext.CurrentUser.Id, _view.ResourceComment);
                _view.ClearResourceComment();
                LoadAll();
            }
            catch (UnauthorizedAccessException ex) { _view.ShowAccessDenied(ex.Message); }
            catch (Exception ex) { _view.ShowError("Ресурсы", ex.Message); }
        }

        private void ReleaseResource()
        {
            var resourceId = _view.GetSelectedAssignedResourceId();
            if (!resourceId.HasValue) return;
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.OperatorRole);
                _services.ResourceService.ReleaseResource(_view.IncidentId, resourceId.Value, SessionContext.CurrentUser.Id, _view.ResourceComment);
                _view.ClearResourceComment();
                LoadAll();
            }
            catch (UnauthorizedAccessException ex) { _view.ShowAccessDenied(ex.Message); }
            catch (Exception ex) { _view.ShowError("Ресурсы", ex.Message); }
        }
    }
}
