using System;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class LoginPresenter
    {
        private readonly ILoginView _view;
        private readonly AppServices _services;

        public LoginPresenter(ILoginView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.LoginRequested += OnLoginRequested;
        }

        private void OnLoginRequested(object sender, EventArgs e)
        {
            try
            {
                var user = _services.UserService.Authenticate(_view.Login, _view.Password);
                if (user == null)
                {
                    _view.ShowError("Неверный логин, пароль или пользователь отключен.");
                    return;
                }
                SessionContext.SetCurrentUser(user);
                _view.NavigateToMain();
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
        }
    }
}
