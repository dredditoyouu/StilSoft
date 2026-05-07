using System;
using System.Windows.Forms;

namespace StilsoftIRS.Views
{
    internal interface IMainView
    {
        event EventHandler OpenIncidentsRequested;
        event EventHandler OpenResourcesRequested;
        event EventHandler OpenCategoriesRequested;
        event EventHandler OpenUsersRequested;
        event EventHandler OpenEventLogRequested;
        event EventHandler OpenReportsRequested;
        event EventHandler UserGuideRequested;
        void SetUserStatusText(string text);
        void SetMenuVisibility(bool incidents, bool resources, bool categories, bool users, bool eventLog, bool reports);
        void ShowAccessDenied(string message);
        void OpenMdiChild(Form form);
        TForm FindMdiChild<TForm>() where TForm : Form;
    }
}
