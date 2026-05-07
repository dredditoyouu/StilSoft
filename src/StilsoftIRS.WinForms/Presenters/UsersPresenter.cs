using System;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class UsersPresenter
    {
        private readonly IUsersView _view;
        private readonly AppServices _services;

        public UsersPresenter(IUsersView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.LoadRequested += (s, e) => LoadUsers();
            _view.AddRequested += (s, e) => AddUser();
            _view.EditRequested += (s, e) => EditUser();
            _view.ActivateRequested += (s, e) => SetActive(true);
            _view.DeactivateRequested += (s, e) => SetActive(false);
        }

        public void Initialize()
        {
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole);
                LoadUsers();
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowAccessDenied(ex.Message);
                _view.CloseView();
            }
        }

        private void LoadUsers()
        {
            try { _view.BindUsers(_services.UserService.GetUsers()); }
            catch (Exception ex) { _view.ShowError("Пользователи", ex.Message); }
        }

        private void AddUser()
        {
            try
            {
                var args = _view.ShowAddDialog();
                if (args == null) return;
                _services.UserService.SaveUser(args.User, args.PlainPassword);
                LoadUsers();
            }
            catch (Exception ex) { _view.ShowError("Пользователи", ex.Message); }
        }

        private void EditUser()
        {
            var user = _view.GetSelectedUser();
            if (user == null) return;
            try
            {
                var args = _view.ShowEditDialog(user);
                if (args == null) return;
                _services.UserService.SaveUser(args.User, args.PlainPassword);
                LoadUsers();
            }
            catch (Exception ex) { _view.ShowError("Пользователи", ex.Message); }
        }

        private void SetActive(bool isActive)
        {
            var user = _view.GetSelectedUser();
            if (user == null) return;
            if (user.Id == SessionContext.CurrentUser.Id && !isActive)
            {
                _view.ShowWarning("Нельзя деактивировать текущего пользователя.");
                return;
            }
            try
            {
                user.IsActive = isActive;
                _services.UserService.SaveUser(user, null);
                LoadUsers();
            }
            catch (Exception ex) { _view.ShowError("Пользователи", ex.Message); }
        }
    }
}
