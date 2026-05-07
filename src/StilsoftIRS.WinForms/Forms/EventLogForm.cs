using System;
using System.Collections.Generic;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Presenters;
using StilsoftIRS.Utilities;
using StilsoftIRS.Views;

namespace StilsoftIRS.Forms
{
    internal sealed class EventLogForm : Form, IEventLogView
    {
        private readonly DateTimePicker _fromPicker;
        private readonly DateTimePicker _toPicker;
        private readonly DataGridView _grid;
        private readonly EventLogPresenter _presenter;

        public event EventHandler LoadRequested;

        public DateTime FromDate => _fromPicker.Value;
        public DateTime ToDate => _toPicker.Value;

        public EventLogForm(AppServices services)
        {
            Text = "Журнал";
            WindowState = FormWindowState.Maximized;
            BackColor = DesktopTheme.SurfaceBackground;

            var heroPanel = DesktopTheme.CreateHeroPanel("Журнал", null);

            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = System.Drawing.Color.Transparent
            };

            _fromPicker = new DateTimePicker { Width = 160, Value = DateTime.Today.AddDays(-30) };
            _toPicker = new DateTimePicker { Width = 160, Value = DateTime.Today };
            var refreshButton = DesktopTheme.CreateButton("Обновить", true);
            refreshButton.Click += (s, e) => LoadRequested?.Invoke(this, EventArgs.Empty);

            filterPanel.Controls.Add(new Label { Text = "С", AutoSize = true, Padding = new Padding(0, 10, 0, 0) });
            filterPanel.Controls.Add(_fromPicker);
            filterPanel.Controls.Add(new Label { Text = "По", AutoSize = true, Padding = new Padding(12, 10, 0, 0) });
            filterPanel.Controls.Add(_toPicker);
            filterPanel.Controls.Add(refreshButton);

            _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            GridHelper.ConfigureReadOnlyGrid(_grid);
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OccurredAt",
                HeaderText = "Время",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm:ss" },
                FillWeight = 20F
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UserName", HeaderText = "Пользователь", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IncidentTitle", HeaderText = "Инцидент", FillWeight = 25F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Action", HeaderText = "Действие", FillWeight = 20F });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Comment", HeaderText = "Комментарий", FillWeight = 35F });

            var filterCard = DesktopTheme.CreateCardPanel(filterPanel, new Padding(0, 0, 0, 16));
            filterCard.Dock = DockStyle.Top;
            filterCard.Height = 86;

            var shellPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = DesktopTheme.SurfaceBackground
            };
            shellPanel.Controls.Add(DesktopTheme.CreateCardPanel(_grid, new Padding(0)));
            shellPanel.Controls.Add(filterCard);

            Controls.Add(shellPanel);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyFormTheme(this);

            _presenter = new EventLogPresenter(this, services);
            Load += (s, e) => LoadRequested?.Invoke(this, EventArgs.Empty);
        }

        public void BindEntries(IList<EventLogEntry> items)
        {
            _grid.DataSource = items;
        }

        public void ShowError(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
