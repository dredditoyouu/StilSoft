using System;
using System.Drawing;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Presenters;
using StilsoftIRS.Services;
using StilsoftIRS.Utilities;
using StilsoftIRS.Views;

namespace StilsoftIRS.Forms
{
    internal sealed class ReportsForm : Form, IReportsView
    {
        private readonly DateTimePicker _fromPicker;
        private readonly DateTimePicker _toPicker;
        private readonly Label _totalValueLabel;
        private readonly Label _escalatedValueLabel;
        private readonly Label _reactionValueLabel;
        private readonly Label _closureValueLabel;
        private readonly DataGridView _priorityGrid;
        private readonly DataGridView _categoryGrid;
        private readonly DataGridView _incidentsGrid;
        private readonly ReportsPresenter _presenter;

        public event EventHandler BuildRequested;
        public event EventHandler ExportRequested;

        public DateTime FromDate => _fromPicker.Value;
        public DateTime ToDate => _toPicker.Value;

        public ReportsForm(AppServices services)
        {
            Text = "Отчёты";
            WindowState = FormWindowState.Maximized;
            BackColor = DesktopTheme.SurfaceBackground;

            var heroPanel = DesktopTheme.CreateHeroPanel("Отчёты", null);

            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _fromPicker = new DateTimePicker { Width = 160, Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) };
            _toPicker = new DateTimePicker { Width = 160, Value = DateTime.Today };
            var buildButton = DesktopTheme.CreateButton("Сформировать", true);
            var exportButton = DesktopTheme.CreateButton("Экспорт");

            buildButton.Click += (s, e) => BuildRequested?.Invoke(this, EventArgs.Empty);
            exportButton.Click += (s, e) => ExportRequested?.Invoke(this, EventArgs.Empty);

            filterPanel.Controls.Add(new Label { Text = "С", AutoSize = true, Padding = new Padding(0, 10, 0, 0) });
            filterPanel.Controls.Add(_fromPicker);
            filterPanel.Controls.Add(new Label { Text = "По", AutoSize = true, Padding = new Padding(12, 10, 0, 0) });
            filterPanel.Controls.Add(_toPicker);
            filterPanel.Controls.Add(buildButton);
            filterPanel.Controls.Add(exportButton);

            var summaryLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                BackColor = DesktopTheme.SurfaceBackground
            };
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            summaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            summaryLayout.Controls.Add(CreateMetricCard("Всего", out _totalValueLabel, DesktopTheme.AccentColor), 0, 0);
            summaryLayout.Controls.Add(CreateMetricCard("Эскалировано", out _escalatedValueLabel, DesktopTheme.AccentDarkColor), 1, 0);
            summaryLayout.Controls.Add(CreateMetricCard("Реакция", out _reactionValueLabel, DesktopTheme.AccentWarmColor), 2, 0);
            summaryLayout.Controls.Add(CreateMetricCard("Закрытие", out _closureValueLabel, Color.FromArgb(126, 96, 184)), 3, 0);

            _priorityGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_priorityGrid);
            _priorityGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReportMetric.Name), HeaderText = "Приоритет", FillWeight = 60F });
            _priorityGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReportMetric.Count), HeaderText = "Количество", FillWeight = 40F });

            _categoryGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_categoryGrid);
            _categoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReportMetric.Name), HeaderText = "Категория", FillWeight = 70F });
            _categoryGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReportMetric.Count), HeaderText = "Количество", FillWeight = 30F });

            var analyticsSplit = new SplitContainer { Dock = DockStyle.Fill };
            var priorityGroup = new GroupBox { Text = "Приоритеты", Dock = DockStyle.Fill };
            priorityGroup.Controls.Add(_priorityGrid);
            var categoryGroup = new GroupBox { Text = "Категории", Dock = DockStyle.Fill };
            categoryGroup.Controls.Add(_categoryGrid);
            analyticsSplit.Panel1.Controls.Add(priorityGroup);
            analyticsSplit.Panel2.Controls.Add(categoryGroup);

            _incidentsGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_incidentsGrid);
            _incidentsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.Id), HeaderText = "ID", FillWeight = 10F });
            _incidentsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.Title), HeaderText = "Заголовок", FillWeight = 35F });
            _incidentsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.Priority), HeaderText = "Приоритет", FillWeight = 15F });
            _incidentsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.CategoryName), HeaderText = "Категория", FillWeight = 20F });
            _incidentsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.StatusName), HeaderText = "Статус", FillWeight = 15F });
            _incidentsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.CreatedAt), HeaderText = "Создан", FillWeight = 15F, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
            _incidentsGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Incident.ClosedAt), HeaderText = "Закрыт", FillWeight = 15F, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });

            var incidentsGroup = new GroupBox { Text = "Инциденты", Dock = DockStyle.Fill };
            incidentsGroup.Controls.Add(_incidentsGrid);

            var splitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            splitContainer.Panel1.Controls.Add(DesktopTheme.CreateCardPanel(analyticsSplit, new Padding(0)));
            splitContainer.Panel2.Controls.Add(DesktopTheme.CreateCardPanel(incidentsGroup, new Padding(0)));

            var filterCard = DesktopTheme.CreateCardPanel(filterPanel, new Padding(0, 0, 0, 16));
            filterCard.Dock = DockStyle.Top;
            filterCard.Height = 86;

            var metricsCard = DesktopTheme.CreateCardPanel(summaryLayout, new Padding(0, 0, 0, 16));
            metricsCard.Dock = DockStyle.Top;
            metricsCard.Height = 120;

            var shellLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                RowCount = 3,
                BackColor = DesktopTheme.SurfaceBackground
            };
            shellLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));
            shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            shellLayout.Controls.Add(filterCard, 0, 0);
            shellLayout.Controls.Add(metricsCard, 0, 1);
            shellLayout.Controls.Add(splitContainer, 0, 2);

            Controls.Add(shellLayout);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyFormTheme(this);

            _presenter = new ReportsPresenter(this, services);
            Load += (s, e) => _presenter.Initialize();
        }

        public void DisplayReport(ReportData data)
        {
            _totalValueLabel.Text = data.TotalIncidents.ToString();
            _escalatedValueLabel.Text = data.EscalatedCount.ToString();
            _reactionValueLabel.Text = ReportService.FormatTimeSpan(data.AverageReactionTime);
            _closureValueLabel.Text = ReportService.FormatTimeSpan(data.AverageClosureTime);
            _priorityGrid.DataSource = data.PriorityBreakdown;
            _categoryGrid.DataSource = data.CategoryBreakdown;
            _incidentsGrid.DataSource = data.Incidents;
        }

        public string ShowSaveDialog()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel (*.xlsx)|*.xlsx";
                dialog.FileName = "StilsoftIRS_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return dialog.FileName;
            }
        }

        public void ShowInfo(string message) =>
            MessageBox.Show(this, message, "Отчёты", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void ShowError(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void CloseView() => Close();

        public void ShowAccessDenied(string message) =>
            MessageBox.Show(this, message, "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private static Panel CreateMetricCard(string caption, out Label valueLabel, Color accentColor)
        {
            var panel = DesktopTheme.CreateCardPanel(new Padding(6));
            panel.Padding = new Padding(18, 16, 18, 16);

            var accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = accentColor
            };
            var captionLabel = new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Bahnschrift SemiBold", 10F),
                ForeColor = DesktopTheme.MutedTextColor,
                Tag = DesktopTheme.SkipThemeTag
            };
            valueLabel = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                Font = new Font("Bahnschrift SemiBold", 20F),
                ForeColor = DesktopTheme.TextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = DesktopTheme.SkipThemeTag
            };

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(captionLabel);
            panel.Controls.Add(accent);
            return panel;
        }
    }
}
