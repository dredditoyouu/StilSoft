using System;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class ReportsPresenter
    {
        private readonly IReportsView _view;
        private readonly AppServices _services;
        private ReportData _reportData;

        public ReportsPresenter(IReportsView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.BuildRequested += (s, e) => BuildReport();
            _view.ExportRequested += (s, e) => ExportReport();
        }

        public void Initialize()
        {
            try
            {
                UserService.EnsureRole(SessionContext.CurrentUser, SystemConstants.AdministratorRole, SystemConstants.AnalystRole);
                BuildReport();
            }
            catch (UnauthorizedAccessException ex)
            {
                _view.ShowAccessDenied(ex.Message);
                _view.CloseView();
            }
        }

        private void BuildReport()
        {
            try
            {
                _reportData = _services.ReportService.BuildReport(_view.FromDate, _view.ToDate);
                _view.DisplayReport(_reportData);
            }
            catch (Exception ex) { _view.ShowError("Отчёты", ex.Message); }
        }

        private void ExportReport()
        {
            if (_reportData == null)
            {
                _view.ShowInfo("Сначала сформируйте отчёт.");
                return;
            }
            try
            {
                var path = _view.ShowSaveDialog();
                if (path == null) return;
                _services.ReportService.ExportToExcel(_reportData, path);
                _view.ShowInfo("Отчёт экспортирован.");
            }
            catch (Exception ex) { _view.ShowError("Экспорт", ex.Message); }
        }
    }
}
