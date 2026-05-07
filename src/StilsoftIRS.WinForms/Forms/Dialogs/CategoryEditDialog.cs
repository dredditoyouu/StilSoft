using System;
using System.Drawing;
using System.Windows.Forms;
using StilsoftIRS.Models;
using StilsoftIRS.Utilities;

namespace StilsoftIRS.Forms.Dialogs
{
    internal sealed class CategoryEditDialog : Form
    {
        private readonly TextBox _nameTextBox;
        private readonly TextBox _descriptionTextBox;

        public CategoryEditDialog(IncidentCategory category = null)
        {
            EditedCategory = category == null
                ? new IncidentCategory()
                : new IncidentCategory
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

            Text = category == null ? "Новая категория" : "Категория";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(560, 300);
            MaximizeBox = false;
            MinimizeBox = false;

            var heroPanel = DesktopTheme.CreateHeroPanel(Text, null);
            heroPanel.Height = 76;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _nameTextBox = new TextBox { Dock = DockStyle.Fill, Text = EditedCategory.Name ?? string.Empty };
            _descriptionTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Text = EditedCategory.Description ?? string.Empty };

            layout.Controls.Add(new Label { Text = "Название", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            layout.Controls.Add(_nameTextBox, 1, 0);
            layout.Controls.Add(new Label { Text = "Описание", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(_descriptionTextBox, 1, 1);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 18, 0, 0)
            };
            var okButton = DesktopTheme.CreateButton("Сохранить", true);
            var cancelButton = DesktopTheme.CreateButton("Отмена");
            cancelButton.DialogResult = DialogResult.Cancel;
            okButton.Click += OnOkButtonClick;
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);

            layout.Controls.Add(buttonPanel, 0, 2);
            layout.SetColumnSpan(buttonPanel, 2);

            var card = DesktopTheme.CreateCardPanel(layout, new Padding(18));
            card.Dock = DockStyle.Fill;

            Controls.Add(card);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyDialogTheme(this);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        public IncidentCategory EditedCategory { get; }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show(this, "Введите название.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EditedCategory.Name = _nameTextBox.Text.Trim();
            EditedCategory.Description = string.IsNullOrWhiteSpace(_descriptionTextBox.Text) ? null : _descriptionTextBox.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
