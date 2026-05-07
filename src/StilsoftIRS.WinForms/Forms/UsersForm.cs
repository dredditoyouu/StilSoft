using System;
using System.Collections.Generic;
using System.Windows.Forms;
using StilsoftIRS.Forms.Dialogs;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Presenters;
using StilsoftIRS.Utilities;
using StilsoftIRS.Views;

namespace StilsoftIRS.Forms
{
    internal sealed class UsersForm : Form, IUsersView
    {
        private readonly DataGridView _grid;
        private readonly UsersPresenter _presenter;

        public event EventHandler LoadRequested;
        public event EventHandler AddRequested;
        public event EventHandler EditRequested;
        public event EventHandler ActivateRequested;
        public event EventHandler DeactivateRequested;

        public UsersForm(AppServices services)
        {
            Text = "Пользователи";
            WindowState = FormWindowState.Maximized;
            BackColor = DesktopTheme.SurfaceBackground;

            var heroPanel = DesktopTheme.CreateHeroPanel("Пользователи", null);

            _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_grid);
            ConfigureGridColumns();

            var actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = System.Drawing.Color.Transparent
            };

            var addButton = DesktopTheme.CreateButton("Добавить", true);
            var editButton = DesktopTheme.CreateButton("Изменить");
            var activateButton = DesktopTheme.CreateButton("Активировать", true);
            var deactivateButton = DesktopTheme.CreateButton("Деактивировать", false, true);
            var refreshButton = DesktopTheme.CreateButton("Обновить");

            addButton.Click += (s, e) => AddRequested?.Invoke(this, EventArgs.Empty);
            editButton.Click += (s, e) => EditRequested?.Invoke(this, EventArgs.Empty);
            activateButton.Click += (s, e) => ActivateRequested?.Invoke(this, EventArgs.Empty);
            deactivateButton.Click += (s, e) => DeactivateRequested?.Invoke(this, EventArgs.Empty);
            refreshButton.Click += (s, e) => LoadRequested?.Invoke(this, EventArgs.Empty);

            actionsPanel.Controls.Add(addButton);
            actionsPanel.Controls.Add(editButton);
            actionsPanel.Controls.Add(activateButton);
            actionsPanel.Controls.Add(deactivateButton);
            actionsPanel.Controls.Add(refreshButton);

            var actionCard = DesktopTheme.CreateCardPanel(actionsPanel, new Padding(0, 0, 0, 16));
            actionCard.Dock = DockStyle.Top;
            actionCard.Height = 86;

            var shellPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = DesktopTheme.SurfaceBackground
            };
            shellPanel.Controls.Add(DesktopTheme.CreateCardPanel(_grid, new Padding(0)));
            shellPanel.Controls.Add(actionCard);

            Controls.Add(shellPanel);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyFormTheme(this);

            _presenter = new UsersPresenter(this, services);
            Load += (s, e) => _presenter.Initialize();
        }

        public void BindUsers(IList<User> items)
        {
            _grid.DataSource = items;
        }

        public User GetSelectedUser() =>
            _grid.CurrentRow?.DataBoundItem as User;

        public UserEditArgs ShowAddDialog()
        {
            using (var dialog = new UserEditDialog())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return new UserEditArgs { User = dialog.EditedUser, PlainPassword = dialog.PlainPassword };
            }
        }

        public UserEditArgs ShowEditDialog(User user)
        {
            using (var dialog = new UserEditDialog(user))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return new UserEditArgs { User = dialog.EditedUser, PlainPassword = dialog.PlainPassword };
            }
        }

        public void CloseView() => Close();

        public void ShowError(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowAccessDenied(string message) =>
            MessageBox.Show(this, message, "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void ShowWarning(string message) =>
            MessageBox.Show(this, message, "Пользователи", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void ConfigureGridColumns()
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(User.Id), HeaderText = "ID", FillWeight = 15F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(User.FirstName), HeaderText = "Имя", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(User.LastName), HeaderText = "Фамилия", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(User.Login), HeaderText = "Логин", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(User.Role), HeaderText = "Роль", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(User.IsActive), HeaderText = "Активен", FillWeight = 10F });
        }
    }
}
