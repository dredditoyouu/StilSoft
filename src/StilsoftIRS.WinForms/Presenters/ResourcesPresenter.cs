using System;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class ResourcesPresenter
    {
        private readonly IResourcesView _view;
        private readonly AppServices _services;

        public ResourcesPresenter(IResourcesView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.LoadRequested += (s, e) => LoadResources();
            _view.AddRequested += (s, e) => SaveResource(null);
            _view.EditRequested += (s, e) => SaveResource(_view.GetSelectedResource());
            _view.DeleteRequested += (s, e) => DeleteResource();
        }

        public void Initialize()
        {
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.OperatorRole);
                LoadResources();
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowAccessDenied(ex.Message);
                _view.CloseView();
            }
        }

        private void LoadResources()
        {
            try { _view.BindResources(_services.ResourceService.GetResources()); }
            catch (Exception ex) { _view.ShowError("Ресурсы", ex.Message); }
        }

        private void SaveResource(ResponseResource existing)
        {
            try
            {
                var result = _view.ShowEditDialog(existing);
                if (result == null) return;
                _services.ResourceService.SaveResource(result);
                LoadResources();
            }
            catch (Exception ex) { _view.ShowError("Ресурсы", ex.Message); }
        }

        private void DeleteResource()
        {
            var res = _view.GetSelectedResource();
            if (res == null || !_view.ConfirmDelete()) return;
            try
            {
                _services.ResourceService.DeleteResource(res.Id);
                LoadResources();
            }
            catch (Exception ex) { _view.ShowError("Ресурсы", ex.Message); }
        }
    }
}
