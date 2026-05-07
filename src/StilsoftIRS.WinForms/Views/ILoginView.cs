using System;

namespace StilsoftIRS.Views
{
    internal interface ILoginView
    {
        string Login { get; }
        string Password { get; }
        event EventHandler LoginRequested;
        void ShowError(string message);
        void NavigateToMain();
    }
}
