using System;
using System.Drawing;
using System.Windows.Forms;
using StilsoftIRS.Models;
using StilsoftIRS.Utilities;

namespace StilsoftIRS.Forms.Dialogs
{
    internal sealed class ResourceEditDialog : Form
    {
        private readonly TextBox _nameTextBox;
        private readonly TextBox _typeTextBox;
        private readonly TextBox _responsibleTextBox;
        private readonly CheckBox _isAvailableCheckBox;

        public ResourceEditDialog(ResponseResource resource = null)
        {
            EditedResource = resource == null
                ? new ResponseResource { IsAvailable = true }
                : new ResponseResource
                {
                    Id = resource.Id,
                    Name = resource.Name,
                    ResourceType = resource.ResourceType,
                    Responsible = resource.Responsible,
                    IsAvailable = resource.IsAvailable
                };

            Text = resource == null ? "Новый ресурс" : "Ресурс";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(580, 320);
            MaximizeBox = false;
            MinimizeBox = false;

            var heroPanel = DesktopTheme.CreateHeroPanel(Text, null);
            heroPanel.Height = 76;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 2,
                RowCount = 5,
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _nameTextBox = new TextBox { Dock = DockStyle.Fill, Text = EditedResource.Name ?? string.Empty };
            _typeTextBox = new TextBox { Dock = DockStyle.Fill, Text = EditedResource.ResourceType ?? string.Empty };
            _responsibleTextBox = new TextBox { Dock = DockStyle.Fill, Text = EditedResource.Responsible ?? string.Empty };
            _isAvailableCheckBox = new CheckBox { Dock = DockStyle.Left, Text = "Доступен", Checked = EditedResource.IsAvailable };

            layout.Controls.Add(new Label { Text = "Название", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            layout.Controls.Add(_nameTextBox, 1, 0);
            layout.Controls.Add(new Label { Text = "Тип", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(_typeTextBox, 1, 1);
            layout.Controls.Add(new Label { Text = "Ответственный", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            layout.Controls.Add(_responsibleTextBox, 1, 2);
            layout.Controls.Add(new Label { Text = "Состояние", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            layout.Controls.Add(_isAvailableCheckBox, 1, 3);

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

            layout.Controls.Add(buttonPanel, 0, 4);
            layout.SetColumnSpan(buttonPanel, 2);

            var card = DesktopTheme.CreateCardPanel(layout, new Padding(18));
            card.Dock = DockStyle.Fill;

            Controls.Add(card);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyDialogTheme(this);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        public ResponseResource EditedResource { get; }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show(this, "Введите название.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EditedResource.Name = _nameTextBox.Text.Trim();
            EditedResource.ResourceType = string.IsNullOrWhiteSpace(_typeTextBox.Text) ? null : _typeTextBox.Text.Trim();
            EditedResource.Responsible = string.IsNullOrWhiteSpace(_responsibleTextBox.Text) ? null : _responsibleTextBox.Text.Trim();
            EditedResource.IsAvailable = _isAvailableCheckBox.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
