using System;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class CategoriesPresenter
    {
        private readonly ICategoriesView _view;
        private readonly AppServices _services;

        public CategoriesPresenter(ICategoriesView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.LoadRequested += (s, e) => LoadCategories();
            _view.AddRequested += (s, e) => SaveCategory(null);
            _view.EditRequested += (s, e) => SaveCategory(_view.GetSelectedCategory());
            _view.DeleteRequested += (s, e) => DeleteCategory();
        }

        public void Initialize()
        {
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole);
                LoadCategories();
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowAccessDenied(ex.Message);
                _view.CloseView();
            }
        }

        private void LoadCategories()
        {
            try { _view.BindCategories(_services.Categories.GetAll()); }
            catch (Exception ex) { _view.ShowError("Категории", ex.Message); }
        }

        private void SaveCategory(IncidentCategory existing)
        {
            try
            {
                var result = _view.ShowEditDialog(existing);
                if (result == null) return;
                if (result.Id == 0) _services.Categories.Add(result);
                else _services.Categories.Update(result);
                LoadCategories();
            }
            catch (Exception ex) { _view.ShowError("Категории", ex.Message); }
        }

        private void DeleteCategory()
        {
            var cat = _view.GetSelectedCategory();
            if (cat == null || !_view.ConfirmDelete()) return;
            try
            {
                _services.Categories.Delete(cat.Id);
                LoadCategories();
            }
            catch (Exception ex) { _view.ShowError("Категории", ex.Message); }
        }
    }
}
