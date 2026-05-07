using System;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Services;
using StilsoftIRS.Views;
using StilsoftIRS.Forms;

namespace StilsoftIRS.Presenters
{
    internal sealed class MainPresenter
    {
        private readonly IMainView _view;
        private readonly AppServices _services;

        public MainPresenter(IMainView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.OpenIncidentsRequested += (s, e) => OpenIncidents();
            _view.OpenResourcesRequested += (s, e) => OpenSection(() => new ResourcesForm(_services), SystemConstants.AdministratorRole, SystemConstants.OperatorRole);
            _view.OpenCategoriesRequested += (s, e) => OpenSection(() => new CategoriesForm(_services), SystemConstants.AdministratorRole);
            _view.OpenUsersRequested += (s, e) => OpenSection(() => new UsersForm(_services), SystemConstants.AdministratorRole);
            _view.OpenEventLogRequested += (s, e) => OpenSingleton(() => new EventLogForm(_services));
            _view.OpenReportsRequested += (s, e) => OpenSection(() => new ReportsForm(_services), SystemConstants.AdministratorRole, SystemConstants.AnalystRole);
            _view.UserGuideRequested += (s, e) => OpenUserGuide();
        }

        public void Initialize()
        {
            var user = SessionContext.CurrentUser;
            _view.SetUserStatusText(user == null ? "Не авторизован" : $"{user.FullName} ({user.Role})");
            _view.SetMenuVisibility(
                incidents: user != null,
                resources: UserService.IsInRole(user, SystemConstants.AdministratorRole, SystemConstants.OperatorRole),
                categories: UserService.IsInRole(user, SystemConstants.AdministratorRole),
                users: UserService.IsInRole(user, SystemConstants.AdministratorRole),
                eventLog: user != null,
                reports: UserService.IsInRole(user, SystemConstants.AdministratorRole, SystemConstants.AnalystRole));
            OpenIncidents();
        }

        private void OpenIncidents() => OpenSingleton(() => new IncidentsForm(_services));

        private void OpenSection<TForm>(Func<TForm> factory, params string[] roles) where TForm : Form
        {
            try { UserService.EnsureRole(SessionContext.CurrentUser, roles); }
            catch (UnauthorizedAccessException ex) { _view.ShowAccessDenied(ex.Message); return; }
            OpenSingleton(factory);
        }

        private void OpenSingleton<TForm>(Func<TForm> factory) where TForm : Form
        {
            var existing = _view.FindMdiChild<TForm>();
            if (existing != null) { existing.Activate(); return; }
            _view.OpenMdiChild(factory());
        }

        private void OpenUserGuide()
        {
            // Delegate to view's UserGuideRequested - main form handles file resolution
        }
    }
}
