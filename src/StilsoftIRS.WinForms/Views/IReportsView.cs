using System;
using StilsoftIRS.Models;

namespace StilsoftIRS.Views
{
    internal interface IReportsView
    {
        DateTime FromDate { get; }
        DateTime ToDate { get; }
        event EventHandler BuildRequested;
        event EventHandler ExportRequested;
        void DisplayReport(ReportData data);
        string ShowSaveDialog();
        void ShowInfo(string message);
        void ShowError(string title, string message);
        void CloseView();
        void ShowAccessDenied(string message);
    }
}
