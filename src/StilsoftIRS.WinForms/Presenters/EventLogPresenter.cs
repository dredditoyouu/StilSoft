using System;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Services;
using StilsoftIRS.Views;

namespace StilsoftIRS.Presenters
{
    internal sealed class EventLogPresenter
    {
        private readonly IEventLogView _view;
        private readonly AppServices _services;

        public EventLogPresenter(IEventLogView view, AppServices services)
        {
            _view = view;
            _services = services;
            _view.LoadRequested += (s, e) => LoadEntries();
        }

        public void LoadEntries()
        {
            try
            {
                var from = _view.FromDate.Date;
                var to = _view.ToDate.Date.AddDays(1).AddTicks(-1);
                _view.BindEntries(_services.EventLogService.GetEntries(from, to));
            }
            catch (Exception ex) { _view.ShowError("Журнал", ex.Message); }
        }
    }
}
