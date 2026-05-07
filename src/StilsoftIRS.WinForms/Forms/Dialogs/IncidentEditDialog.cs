using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Utilities;

namespace StilsoftIRS.Forms.Dialogs
{
    internal sealed class IncidentEditDialog : Form
    {
        private readonly ComboBox _priorityComboBox;
        private readonly ComboBox _categoryComboBox;
        private readonly TextBox _titleTextBox;
        private readonly TextBox _descriptionTextBox;

        public IncidentEditDialog(IList<IncidentCategory> categories)
        {
            Text = "Новый инцидент";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(620, 420);

            var heroPanel = DesktopTheme.CreateHeroPanel("Новый инцидент", null);
            heroPanel.Height = 76;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 2,
                RowCount = 5,
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _titleTextBox = new TextBox { Dock = DockStyle.Fill };
            _priorityComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _categoryComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _descriptionTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };

            _priorityComboBox.Items.AddRange(SystemConstants.Priorities.Cast<object>().ToArray());
            _priorityComboBox.SelectedIndex = 0;

            _categoryComboBox.DataSource = categories.ToList();
            _categoryComboBox.DisplayMember = nameof(IncidentCategory.Name);
            _categoryComboBox.ValueMember = nameof(IncidentCategory.Id);

            layout.Controls.Add(new Label { Text = "Заголовок", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            layout.Controls.Add(_titleTextBox, 1, 0);
            layout.Controls.Add(new Label { Text = "Приоритет", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(_priorityComboBox, 1, 1);
            layout.Controls.Add(new Label { Text = "Категория", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            layout.Controls.Add(_categoryComboBox, 1, 2);
            layout.Controls.Add(new Label { Text = "Описание", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            layout.Controls.Add(_descriptionTextBox, 1, 3);

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 18, 0, 0)
            };

            var okButton = DesktopTheme.CreateButton("Создать", true);
            var cancelButton = DesktopTheme.CreateButton("Отмена");
            cancelButton.DialogResult = DialogResult.Cancel;
            okButton.Click += OnOkButtonClick;

            buttonsPanel.Controls.Add(okButton);
            buttonsPanel.Controls.Add(cancelButton);

            layout.Controls.Add(buttonsPanel, 0, 4);
            layout.SetColumnSpan(buttonsPanel, 2);

            var card = DesktopTheme.CreateCardPanel(layout, new Padding(18));
            card.Dock = DockStyle.Fill;

            Controls.Add(card);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyDialogTheme(this);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        public string IncidentTitle => _titleTextBox.Text.Trim();

        public string IncidentDescription => string.IsNullOrWhiteSpace(_descriptionTextBox.Text) ? null : _descriptionTextBox.Text.Trim();

        public string SelectedPriority => Convert.ToString(_priorityComboBox.SelectedItem);

        public int SelectedCategoryId => _categoryComboBox.SelectedItem is IncidentCategory category ? category.Id : 0;

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(IncidentTitle))
            {
                MessageBox.Show(this, "Введите заголовок.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (SelectedCategoryId <= 0)
            {
                MessageBox.Show(this, "Выберите категорию.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // IncidentEditDialog
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "IncidentEditDialog";
            this.Load += new System.EventHandler(this.IncidentEditDialog_Load);
            this.ResumeLayout(false);

        }

        private void IncidentEditDialog_Load(object sender, EventArgs e)
        {

        }
    }
}
