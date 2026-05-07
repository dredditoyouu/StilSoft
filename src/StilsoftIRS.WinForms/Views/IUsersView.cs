using System;
using System.Collections.Generic;
using StilsoftIRS.Models;

namespace StilsoftIRS.Views
{
    internal interface IUsersView
    {
        event EventHandler LoadRequested;
        event EventHandler AddRequested;
        event EventHandler EditRequested;
        event EventHandler ActivateRequested;
        event EventHandler DeactivateRequested;
        void BindUsers(IList<User> items);
        User GetSelectedUser();
        UserEditArgs ShowAddDialog();
        UserEditArgs ShowEditDialog(User user);
        void CloseView();
        void ShowError(string title, string message);
        void ShowAccessDenied(string message);
        void ShowWarning(string message);
    }

    internal sealed class UserEditArgs
    {
        public User User;
        public string PlainPassword;
    }
}
