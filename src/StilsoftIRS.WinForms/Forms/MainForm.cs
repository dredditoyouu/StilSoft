using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Presenters;
using StilsoftIRS.Utilities;
using StilsoftIRS.Views;

namespace StilsoftIRS.Forms
{
    internal sealed class MainForm : Form, IMainView
    {
        private readonly ToolStripMenuItem _incidentsMenuItem;
        private readonly ToolStripMenuItem _resourcesMenuItem;
        private readonly ToolStripMenuItem _categoriesMenuItem;
        private readonly ToolStripMenuItem _usersMenuItem;
        private readonly ToolStripMenuItem _eventLogMenuItem;
        private readonly ToolStripMenuItem _reportsMenuItem;
        private readonly ToolStripStatusLabel _userStatusLabel;
        private readonly MainPresenter _presenter;

        public event EventHandler OpenIncidentsRequested;
        public event EventHandler OpenResourcesRequested;
        public event EventHandler OpenCategoriesRequested;
        public event EventHandler OpenUsersRequested;
        public event EventHandler OpenEventLogRequested;
        public event EventHandler OpenReportsRequested;
        public event EventHandler UserGuideRequested;

        public MainForm(AppServices services)
        {
            Text = "StilsoftIRS";
            WindowState = FormWindowState.Maximized;
            IsMdiContainer = true;
            BackColor = DesktopTheme.WorkspaceBackground;

            var menuStrip = new MenuStrip { Dock = DockStyle.Top };
            _incidentsMenuItem = new ToolStripMenuItem("Инциденты", null, (s, e) => OpenIncidentsRequested?.Invoke(this, EventArgs.Empty));
            _resourcesMenuItem = new ToolStripMenuItem("Ресурсы", null, (s, e) => OpenResourcesRequested?.Invoke(this, EventArgs.Empty));
            _categoriesMenuItem = new ToolStripMenuItem("Категории", null, (s, e) => OpenCategoriesRequested?.Invoke(this, EventArgs.Empty));
            _usersMenuItem = new ToolStripMenuItem("Пользователи", null, (s, e) => OpenUsersRequested?.Invoke(this, EventArgs.Empty));
            _eventLogMenuItem = new ToolStripMenuItem("Журнал", null, (s, e) => OpenEventLogRequested?.Invoke(this, EventArgs.Empty));
            _reportsMenuItem = new ToolStripMenuItem("Отчёты", null, (s, e) => OpenReportsRequested?.Invoke(this, EventArgs.Empty));

            var helpMenuItem = new ToolStripMenuItem("Справка");
            helpMenuItem.DropDownItems.Add(new ToolStripMenuItem("Руководство", null, (s, e) => UserGuideRequested?.Invoke(this, EventArgs.Empty)));
            helpMenuItem.DropDownItems.Add(new ToolStripSeparator());
            helpMenuItem.DropDownItems.Add(new ToolStripMenuItem("Выход", null, (s, e) => Close()));

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                _incidentsMenuItem, _resourcesMenuItem, _categoriesMenuItem,
                _usersMenuItem, _eventLogMenuItem, _reportsMenuItem, helpMenuItem
            });

            var headerPanel = DesktopTheme.CreateHeroPanel("StilsoftIRS", null);
            var statusStrip = new StatusStrip { Dock = DockStyle.Bottom };
            _userStatusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(_userStatusLabel);

            MainMenuStrip = menuStrip;
            Controls.Add(statusStrip);
            Controls.Add(headerPanel);
            Controls.Add(menuStrip);

            DesktopTheme.ApplyFormTheme(this);
            Shown += (s, e) => DesktopTheme.ApplyFormTheme(this);
            FormClosed += (s, e) => SessionContext.Clear();

            UserGuideRequested += (s, e) => OnUserGuideRequested();

            _presenter = new MainPresenter(this, services);
            Load += (s, e) => _presenter.Initialize();
        }

        public void SetUserStatusText(string text) => _userStatusLabel.Text = text;

        public void SetMenuVisibility(bool incidents, bool resources, bool categories, bool users, bool eventLog, bool reports)
        {
            _incidentsMenuItem.Visible = incidents;
            _resourcesMenuItem.Visible = resources;
            _categoriesMenuItem.Visible = categories;
            _usersMenuItem.Visible = users;
            _eventLogMenuItem.Visible = eventLog;
            _reportsMenuItem.Visible = reports;
        }

        public void ShowAccessDenied(string message) =>
            MessageBox.Show(this, message, "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void OpenMdiChild(Form form)
        {
            form.MdiParent = this;
            form.Show();
        }

        public TForm FindMdiChild<TForm>() where TForm : Form =>
            MdiChildren.OfType<TForm>().FirstOrDefault();

        private void OnUserGuideRequested()
        {
            try
            {
                var path = ResolveUserGuidePath();
                if (path == null)
                {
                    MessageBox.Show(this, "Файл не найден.", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Справка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string ResolveUserGuidePath()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                var p1 = Path.Combine(dir.FullName, "Docs", "UserGuide.html");
                if (File.Exists(p1)) return p1;
                var p2 = Path.Combine(dir.FullName, "docs", "user-guide.html");
                if (File.Exists(p2)) return p2;
                dir = dir.Parent;
            }
            return null;
        }
    }
}
