using System;
using System.Collections.Generic;
using StilsoftIRS.Models;

namespace StilsoftIRS.Views
{
    internal interface IResourcesView
    {
        event EventHandler LoadRequested;
        event EventHandler AddRequested;
        event EventHandler EditRequested;
        event EventHandler DeleteRequested;
        void BindResources(IList<ResponseResource> items);
        ResponseResource GetSelectedResource();
        ResponseResource ShowEditDialog(ResponseResource existing);
        bool ConfirmDelete();
        void CloseView();
        void ShowError(string title, string message);
        void ShowAccessDenied(string message);
    }
}
