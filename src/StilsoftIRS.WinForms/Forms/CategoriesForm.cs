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
    internal sealed class CategoriesForm : Form, ICategoriesView
    {
        private readonly DataGridView _grid;
        private readonly CategoriesPresenter _presenter;

        public event EventHandler LoadRequested;
        public event EventHandler AddRequested;
        public event EventHandler EditRequested;
        public event EventHandler DeleteRequested;

        public CategoriesForm(AppServices services)
        {
            Text = "Категории";
            WindowState = FormWindowState.Maximized;
            BackColor = DesktopTheme.SurfaceBackground;

            var heroPanel = DesktopTheme.CreateHeroPanel("Категории", null);

            _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(IncidentCategory.Id), HeaderText = "ID", FillWeight = 15F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(IncidentCategory.Name), HeaderText = "Название", FillWeight = 30F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(IncidentCategory.Description), HeaderText = "Описание", FillWeight = 55F });

            var actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = System.Drawing.Color.Transparent
            };

            var addButton = DesktopTheme.CreateButton("Добавить", true);
            var editButton = DesktopTheme.CreateButton("Изменить");
            var deleteButton = DesktopTheme.CreateButton("Удалить", false, true);
            var refreshButton = DesktopTheme.CreateButton("Обновить");

            addButton.Click += (s, e) => AddRequested?.Invoke(this, EventArgs.Empty);
            editButton.Click += (s, e) => EditRequested?.Invoke(this, EventArgs.Empty);
            deleteButton.Click += (s, e) => DeleteRequested?.Invoke(this, EventArgs.Empty);
            refreshButton.Click += (s, e) => LoadRequested?.Invoke(this, EventArgs.Empty);

            actionsPanel.Controls.Add(addButton);
            actionsPanel.Controls.Add(editButton);
            actionsPanel.Controls.Add(deleteButton);
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

            _presenter = new CategoriesPresenter(this, services);
            Load += (s, e) => _presenter.Initialize();
        }

        public void BindCategories(IList<IncidentCategory> items)
        {
            _grid.DataSource = items;
        }

        public IncidentCategory GetSelectedCategory() =>
            _grid.CurrentRow?.DataBoundItem as IncidentCategory;

        public IncidentCategory ShowEditDialog(IncidentCategory existing)
        {
            using (var dialog = new CategoryEditDialog(existing))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return dialog.EditedCategory;
            }
        }

        public bool ConfirmDelete() =>
            MessageBox.Show(this, "Удалить выбранную категорию?", "Категории",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void CloseView() => Close();

        public void ShowError(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowAccessDenied(string message) =>
            MessageBox.Show(this, message, "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
