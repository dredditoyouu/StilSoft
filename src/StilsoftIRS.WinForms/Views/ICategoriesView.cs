using System;
using System.Collections.Generic;
using StilsoftIRS.Models;

namespace StilsoftIRS.Views
{
    internal interface ICategoriesView
    {
        event EventHandler LoadRequested;
        event EventHandler AddRequested;
        event EventHandler EditRequested;
        event EventHandler DeleteRequested;
        void BindCategories(IList<IncidentCategory> items);
        IncidentCategory GetSelectedCategory();
        IncidentCategory ShowEditDialog(IncidentCategory existing);
        bool ConfirmDelete();
        void CloseView();
        void ShowError(string title, string message);
        void ShowAccessDenied(string message);
    }
}
