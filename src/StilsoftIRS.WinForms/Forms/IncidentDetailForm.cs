using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Presenters;
using StilsoftIRS.Utilities;
using StilsoftIRS.Views;

namespace StilsoftIRS.Forms
{
    internal sealed class IncidentDetailForm : Form, IIncidentDetailView
    {
        private readonly Label _idValueLabel;
        private readonly Label _createdValueLabel;
        private readonly Label _categoryValueLabel;
        private readonly Label _priorityValueLabel;
        private readonly Label _statusValueLabel;
        private readonly Label _operatorValueLabel;
        private readonly Label _closedValueLabel;
        private readonly TextBox _descriptionTextBox;
        private ComboBox _nextStatusComboBox;
        private Button _changeStatusButton;
        private Button _escalateButton;
        private TextBox _statusCommentTextBox;
        private DataGridView _assignedResourcesGrid;
        private DataGridView _availableResourcesGrid;
        private TextBox _resourceCommentTextBox;
        private Button _assignResourceButton;
        private Button _releaseResourceButton;
        private DataGridView _eventLogGrid;
        private readonly IncidentDetailPresenter _presenter;

        public event EventHandler LoadRequested;
        public event EventHandler ChangeStatusRequested;
        public event EventHandler EscalateRequested;
        public event EventHandler AssignResourceRequested;
        public event EventHandler ReleaseResourceRequested;

        public int IncidentId { get; }

        public string StatusComment => _statusCommentTextBox.Text;
        public string ResourceComment => _resourceCommentTextBox.Text;

        public IncidentDetailForm(AppServices services, int incidentId)
        {
            IncidentId = incidentId;
            Tag = incidentId;
            WindowState = FormWindowState.Maximized;
            BackColor = DesktopTheme.SurfaceBackground;

            var heroPanel = DesktopTheme.CreateHeroPanel("Инцидент", null);

            var detailsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            _idValueLabel = AddInfoField(detailsGrid, 0, 0, "ID");
            _createdValueLabel = AddInfoField(detailsGrid, 0, 2, "Создан");
            _categoryValueLabel = AddInfoField(detailsGrid, 1, 0, "Категория");
            _priorityValueLabel = AddInfoField(detailsGrid, 1, 2, "Приоритет");
            _statusValueLabel = AddInfoField(detailsGrid, 2, 0, "Статус");
            _operatorValueLabel = AddInfoField(detailsGrid, 2, 2, "Оператор");
            _closedValueLabel = AddInfoField(detailsGrid, 3, 0, "Закрыт");

            _descriptionTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            var descriptionLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            descriptionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            descriptionLayout.Controls.Add(_descriptionTextBox, 0, 0);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreateStatusPage());
            tabs.TabPages.Add(CreateResourcesPage());
            tabs.TabPages.Add(CreateLogPage());

            var shellLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                RowCount = 3,
                BackColor = DesktopTheme.SurfaceBackground
            };
            shellLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
            shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var infoCard = DesktopTheme.CreateCardPanel(detailsGrid, new Padding(0, 0, 0, 16));
            infoCard.Dock = DockStyle.Top;
            infoCard.Height = 196;

            shellLayout.Controls.Add(infoCard, 0, 0);
            shellLayout.Controls.Add(DesktopTheme.CreateCardPanel(descriptionLayout, new Padding(0, 0, 0, 16)), 0, 1);
            shellLayout.Controls.Add(DesktopTheme.CreateCardPanel(tabs, new Padding(0)), 0, 2);

            Controls.Add(shellLayout);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyFormTheme(this);
            DesktopTheme.ApplyButtonStyle(_changeStatusButton, true);
            DesktopTheme.ApplyButtonStyle(_escalateButton, true);
            DesktopTheme.ApplyButtonStyle(_assignResourceButton, true);
            DesktopTheme.ApplyButtonStyle(_releaseResourceButton, false, true);

            _presenter = new IncidentDetailPresenter(this, services);
            Load += (s, e) => LoadRequested?.Invoke(this, EventArgs.Empty);
        }

        public void DisplayIncident(Incident incident)
        {
            Text = $"Инцидент #{incident.Id}: {incident.Title}";
            _idValueLabel.Text = incident.Id.ToString();
            _createdValueLabel.Text = incident.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            _categoryValueLabel.Text = incident.CategoryName;
            _priorityValueLabel.Text = incident.Priority;
            _statusValueLabel.Text = incident.StatusName;
            _operatorValueLabel.Text = incident.OperatorName;
            _closedValueLabel.Text = incident.ClosedAt.HasValue
                ? incident.ClosedAt.Value.ToString("dd.MM.yyyy HH:mm")
                : "Не закрыт";
            _descriptionTextBox.Text = incident.Description ?? string.Empty;
            _priorityValueLabel.ForeColor = GetPriorityColor(incident.Priority);
            _statusValueLabel.ForeColor = DesktopTheme.AccentDarkColor;
        }

        public void BindAllowedStatuses(IList<string> statuses, bool canEscalate)
        {
            _nextStatusComboBox.DataSource = statuses;
            _changeStatusButton.Enabled = statuses.Count > 0;
            _escalateButton.Enabled = canEscalate;
        }

        public void BindAssignedResources(IList<IncidentResource> resources)
        {
            _assignedResourcesGrid.DataSource = resources;
        }

        public void BindAvailableResources(IList<ResponseResource> resources)
        {
            _availableResourcesGrid.DataSource = resources;
        }

        public void BindEventLog(IList<EventLogEntry> entries)
        {
            _eventLogGrid.DataSource = entries;
        }

        public void SetOperationsEnabled(bool canOperate, bool hasStatuses, bool canEscalate)
        {
            _nextStatusComboBox.Enabled = canOperate;
            _changeStatusButton.Enabled = canOperate && hasStatuses;
            _statusCommentTextBox.Enabled = canOperate;
            _escalateButton.Enabled = canOperate && canEscalate;
            _assignResourceButton.Enabled = canOperate;
            _releaseResourceButton.Enabled = canOperate;
            _resourceCommentTextBox.Enabled = canOperate;
        }

        public string GetSelectedTargetStatus() =>
            _nextStatusComboBox.SelectedItem as string;

        public int? GetSelectedAvailableResourceId() =>
            (_availableResourcesGrid.CurrentRow?.DataBoundItem as ResponseResource)?.Id;

        public int? GetSelectedAssignedResourceId() =>
            (_assignedResourcesGrid.CurrentRow?.DataBoundItem as IncidentResource)?.ResourceId;

        public void ClearStatusComment() => _statusCommentTextBox.Clear();

        public void ClearResourceComment() => _resourceCommentTextBox.Clear();

        public void CloseView() => Close();

        public void ShowError(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowAccessDenied(string message) =>
            MessageBox.Show(this, message, "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private TabPage CreateStatusPage()
        {
            var page = new TabPage("Статусы");
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 2,
                RowCount = 3,
                BackColor = DesktopTheme.CardBackground
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _nextStatusComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _statusCommentTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            _changeStatusButton = DesktopTheme.CreateButton("Сменить статус", true);
            _escalateButton = DesktopTheme.CreateButton("Эскалировать", true);

            _changeStatusButton.Click += (s, e) => ChangeStatusRequested?.Invoke(this, EventArgs.Empty);
            _escalateButton.Click += (s, e) => EscalateRequested?.Invoke(this, EventArgs.Empty);

            layout.Controls.Add(new Label { Text = "Следующий статус", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            layout.Controls.Add(_nextStatusComboBox, 1, 0);
            layout.Controls.Add(new Label { Text = "Комментарий", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            layout.Controls.Add(_statusCommentTextBox, 1, 1);

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 18, 0, 0)
            };
            buttonsPanel.Controls.Add(_changeStatusButton);
            buttonsPanel.Controls.Add(_escalateButton);
            layout.Controls.Add(buttonsPanel, 1, 2);

            page.Controls.Add(layout);
            return page;
        }

        private TabPage CreateResourcesPage()
        {
            var page = new TabPage("Ресурсы");
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 1,
                RowCount = 2,
                BackColor = DesktopTheme.CardBackground
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.None,
                BackColor = DesktopTheme.SurfaceBackground
            };

            _assignedResourcesGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_assignedResourcesGrid);
            ConfigureAssignedResourcesGrid();

            _availableResourcesGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_availableResourcesGrid);
            ConfigureAvailableResourcesGrid();

            var assignedGroup = new GroupBox { Text = "Назначенные", Dock = DockStyle.Fill };
            assignedGroup.Controls.Add(_assignedResourcesGrid);
            var availableGroup = new GroupBox { Text = "Доступные", Dock = DockStyle.Fill };
            availableGroup.Controls.Add(_availableResourcesGrid);

            splitContainer.Panel1.Controls.Add(assignedGroup);
            splitContainer.Panel2.Controls.Add(availableGroup);

            var actionsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _resourceCommentTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            _assignResourceButton = DesktopTheme.CreateButton("Назначить", true);
            _releaseResourceButton = DesktopTheme.CreateButton("Освободить", false, true);
            _assignResourceButton.Click += (s, e) => AssignResourceRequested?.Invoke(this, EventArgs.Empty);
            _releaseResourceButton.Click += (s, e) => ReleaseResourceRequested?.Invoke(this, EventArgs.Empty);
            buttonPanel.Controls.Add(_assignResourceButton);
            buttonPanel.Controls.Add(_releaseResourceButton);

            actionsLayout.Controls.Add(_resourceCommentTextBox, 0, 0);
            actionsLayout.Controls.Add(buttonPanel, 1, 0);

            layout.Controls.Add(splitContainer, 0, 0);
            layout.Controls.Add(actionsLayout, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage CreateLogPage()
        {
            var page = new TabPage("Журнал");
            _eventLogGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_eventLogGrid);
            _eventLogGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(EventLogEntry.OccurredAt),
                HeaderText = "Время",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm:ss" },
                FillWeight = 20F
            });
            _eventLogGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EventLogEntry.UserName), HeaderText = "Пользователь", FillWeight = 20F });
            _eventLogGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EventLogEntry.Action), HeaderText = "Действие", FillWeight = 20F });
            _eventLogGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(EventLogEntry.Comment), HeaderText = "Комментарий", FillWeight = 40F });
            page.Controls.Add(_eventLogGrid);
            return page;
        }

        private static Label AddInfoField(TableLayoutPanel panel, int rowIndex, int labelColumn, string labelText)
        {
            if (panel.RowStyles.Count <= rowIndex)
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = DesktopTheme.MutedTextColor
            };
            var value = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Bahnschrift SemiBold", 12F),
                ForeColor = DesktopTheme.TextColor,
                Tag = DesktopTheme.SkipThemeTag
            };

            panel.Controls.Add(label, labelColumn, rowIndex);
            panel.Controls.Add(value, labelColumn + 1, rowIndex);
            return value;
        }

        private void ConfigureAssignedResourcesGrid()
        {
            _assignedResourcesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(IncidentResource.ResourceName), HeaderText = "Ресурс", FillWeight = 30F });
            _assignedResourcesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(IncidentResource.ResourceType), HeaderText = "Тип", FillWeight = 20F });
            _assignedResourcesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(IncidentResource.Responsible), HeaderText = "Ответственный", FillWeight = 25F });
            _assignedResourcesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(IncidentResource.AssignedAt),
                HeaderText = "Назначен",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" },
                FillWeight = 20F
            });
        }

        private void ConfigureAvailableResourcesGrid()
        {
            _availableResourcesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ResponseResource.Name), HeaderText = "Ресурс", FillWeight = 30F });
            _availableResourcesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ResponseResource.ResourceType), HeaderText = "Тип", FillWeight = 20F });
            _availableResourcesGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ResponseResource.Responsible), HeaderText = "Ответственный", FillWeight = 25F });
            _availableResourcesGrid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(ResponseResource.IsAvailable), HeaderText = "Доступен", FillWeight = 10F });
        }

        private static Color GetPriorityColor(string priority)
        {
            switch (priority)
            {
                case SystemConstants.CriticalPriority:
                    return Color.FromArgb(186, 55, 52);
                case SystemConstants.HighPriority:
                    return Color.FromArgb(211, 124, 42);
                case SystemConstants.MediumPriority:
                    return Color.FromArgb(181, 142, 18);
                default:
                    return DesktopTheme.TextColor;
            }
        }
    }
}
