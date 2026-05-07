using System;
using System.Collections.Generic;
using StilsoftIRS.Models;

namespace StilsoftIRS.Views
{
    internal interface IEventLogView
    {
        DateTime FromDate { get; }
        DateTime ToDate { get; }
        event EventHandler LoadRequested;
        void BindEntries(IList<EventLogEntry> items);
        void ShowError(string title, string message);
    }
}
