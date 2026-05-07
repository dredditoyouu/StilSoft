using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StilsoftIRS.Forms.Dialogs;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Presenters;
using StilsoftIRS.Utilities;
using StilsoftIRS.Views;

namespace StilsoftIRS.Forms
{
    internal sealed class IncidentsForm : Form, IIncidentsView
    {
        private readonly AppServices _services;
        private readonly DataGridView _grid;
        private readonly ComboBox _statusComboBox;
        private readonly ComboBox _categoryComboBox;
        private readonly ComboBox _priorityComboBox;
        private readonly TextBox _searchTextBox;
        private readonly Button _createButton;
        private readonly IncidentsPresenter _presenter;

        public event EventHandler LoadRequested;
        public event EventHandler ResetRequested;
        public event EventHandler CreateIncidentRequested;
        public event EventHandler OpenSelectedRequested;
        public event EventHandler RefreshRequested;

        public int? SelectedStatusId =>
            _statusComboBox.SelectedItem is IncidentStatus s && s.Id > 0 ? s.Id : (int?)null;

        public int? SelectedCategoryId =>
            _categoryComboBox.SelectedItem is IncidentCategory c && c.Id > 0 ? c.Id : (int?)null;

        public string SelectedPriority =>
            _priorityComboBox.SelectedItem is string p && p != "Все приоритеты" ? p : null;

        public string SearchText =>
            string.IsNullOrWhiteSpace(_searchTextBox.Text) ? null : _searchTextBox.Text.Trim();

        public IncidentsForm(AppServices services)
        {
            _services = services;
            Text = "Реестр инцидентов";
            WindowState = FormWindowState.Maximized;
            BackColor = DesktopTheme.SurfaceBackground;

            var heroPanel = DesktopTheme.CreateHeroPanel("Инциденты", null);

            var filterPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 8,
                BackColor = System.Drawing.Color.Transparent
            };
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

            _statusComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _categoryComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _priorityComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _searchTextBox = new TextBox { Dock = DockStyle.Fill };

            filterPanel.Controls.Add(new Label { Text = "Статус", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            filterPanel.Controls.Add(_statusComboBox, 1, 0);
            filterPanel.Controls.Add(new Label { Text = "Категория", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 0);
            filterPanel.Controls.Add(_categoryComboBox, 3, 0);
            filterPanel.Controls.Add(new Label { Text = "Приоритет", Anchor = AnchorStyles.Left, AutoSize = true }, 4, 0);
            filterPanel.Controls.Add(_priorityComboBox, 5, 0);
            filterPanel.Controls.Add(new Label { Text = "Поиск", Anchor = AnchorStyles.Left, AutoSize = true }, 6, 0);
            filterPanel.Controls.Add(_searchTextBox, 7, 0);

            var actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = System.Drawing.Color.Transparent,
                Margin = new Padding(0, 16, 0, 0)
            };
            var applyButton = DesktopTheme.CreateButton("Применить", true);
            var resetButton = DesktopTheme.CreateButton("Сбросить");
            var openButton = DesktopTheme.CreateButton("Открыть");
            _createButton = DesktopTheme.CreateButton("Создать", true);
            var refreshButton = DesktopTheme.CreateButton("Обновить");

            applyButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
            resetButton.Click += (s, e) => ResetRequested?.Invoke(this, EventArgs.Empty);
            openButton.Click += (s, e) => OpenSelectedRequested?.Invoke(this, EventArgs.Empty);
            _createButton.Click += (s, e) => CreateIncidentRequested?.Invoke(this, EventArgs.Empty);
            refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

            actionsPanel.Controls.Add(applyButton);
            actionsPanel.Controls.Add(resetButton);
            actionsPanel.Controls.Add(openButton);
            actionsPanel.Controls.Add(_createButton);
            actionsPanel.Controls.Add(refreshButton);

            var commandLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                BackColor = System.Drawing.Color.Transparent
            };
            commandLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            commandLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            commandLayout.Controls.Add(filterPanel, 0, 0);
            commandLayout.Controls.Add(actionsPanel, 0, 1);

            _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_grid);
            _grid.RowPrePaint += OnGridRowPrePaint;
            _grid.CellDoubleClick += (s, e) => OpenSelectedRequested?.Invoke(this, EventArgs.Empty);
            ConfigureGridColumns();

            var commandsCard = DesktopTheme.CreateCardPanel(commandLayout, new Padding(0, 0, 0, 16));
            commandsCard.Dock = DockStyle.Top;
            commandsCard.Height = 128;

            var shellPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = DesktopTheme.SurfaceBackground
            };
            shellPanel.Controls.Add(DesktopTheme.CreateCardPanel(_grid, new Padding(0)));
            shellPanel.Controls.Add(commandsCard);

            Controls.Add(shellPanel);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyFormTheme(this);

            Load += (s, e) => LoadRequested?.Invoke(this, EventArgs.Empty);
            Activated += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

            _presenter = new IncidentsPresenter(this, services);
        }

        public void BindStatuses(IList<IncidentStatus> items)
        {
            _statusComboBox.DataSource = items;
            _statusComboBox.DisplayMember = nameof(IncidentStatus.Name);
            _statusComboBox.ValueMember = nameof(IncidentStatus.Id);
        }

        public void BindCategories(IList<IncidentCategory> items)
        {
            _categoryComboBox.DataSource = items;
            _categoryComboBox.DisplayMember = nameof(IncidentCategory.Name);
            _categoryComboBox.ValueMember = nameof(IncidentCategory.Id);
        }

        public void BindPriorities(IList<string> items)
        {
            _priorityComboBox.DataSource = items;
        }

        public void BindIncidents(IList<Incident> items)
        {
            _grid.DataSource = items;
        }

        public void SetCreateVisible(bool visible)
        {
            _createButton.Visible = visible;
        }

        public void ResetFilters()
        {
            _statusComboBox.SelectedIndex = 0;
            _categoryComboBox.SelectedIndex = 0;
            _priorityComboBox.SelectedIndex = 0;
            _searchTextBox.Clear();
        }

        public int? GetSelectedIncidentId()
        {
            return _grid.CurrentRow?.DataBoundItem is Incident i ? i.Id : (int?)null;
        }

        public NewIncidentArgs ShowCreateDialog(IList<IncidentCategory> categories)
        {
            using (var dialog = new IncidentEditDialog(categories))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return new NewIncidentArgs
                {
                    Title = dialog.IncidentTitle,
                    Description = dialog.IncidentDescription,
                    Priority = dialog.SelectedPriority,
                    CategoryId = dialog.SelectedCategoryId
                };
            }
        }

        public void NavigateToIncident(int incidentId)
        {
            if (MdiParent == null)
                return;

            var existing = MdiParent.MdiChildren
                .OfType<IncidentDetailForm>()
                .FirstOrDefault(f => f.IncidentId == incidentId);

            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var form = new IncidentDetailForm(_services, incidentId)
            {
                MdiParent = MdiParent
            };
            form.Show();
        }

        public void ShowError(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void OnGridRowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count)
                return;

            if (_grid.Rows[e.RowIndex].DataBoundItem is Incident incident)
                GridHelper.ApplyPriorityRowColor(_grid.Rows[e.RowIndex], incident.Priority);
        }

        private void ConfigureGridColumns()
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.Id), HeaderText = "ID", Width = 60, FillWeight = 15F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.Title), HeaderText = "Заголовок", FillWeight = 40F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.Priority), HeaderText = "Приоритет", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.CategoryName), HeaderText = "Категория", FillWeight = 25F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.StatusName), HeaderText = "Статус", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.OperatorName), HeaderText = "Оператор", FillWeight = 25F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.CreatedAt), HeaderText = "Создан", FillWeight = 22F, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.ClosedAt), HeaderText = "Закрыт", FillWeight = 22F, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
        }
    }
}
