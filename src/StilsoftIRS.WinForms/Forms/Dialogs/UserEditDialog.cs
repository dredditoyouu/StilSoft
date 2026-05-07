using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Models;
using StilsoftIRS.Utilities;

namespace StilsoftIRS.Forms.Dialogs
{
    internal sealed class UserEditDialog : Form
    {
        private readonly TextBox _firstNameTextBox;
        private readonly TextBox _lastNameTextBox;
        private readonly TextBox _loginTextBox;
        private readonly TextBox _passwordTextBox;
        private readonly ComboBox _roleComboBox;
        private readonly CheckBox _isActiveCheckBox;

        public UserEditDialog(User user = null)
        {
            EditedUser = user == null
                ? new User { IsActive = true, Role = SystemConstants.OperatorRole }
                : new User
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Login = user.Login,
                    PasswordHash = user.PasswordHash,
                    Role = user.Role,
                    IsActive = user.IsActive
                };

            Text = user == null ? "Новый пользователь" : "Пользователь";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(620, 400);

            var heroPanel = DesktopTheme.CreateHeroPanel(Text, null);
            heroPanel.Height = 76;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 2,
                RowCount = 7,
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _firstNameTextBox = new TextBox { Dock = DockStyle.Fill, Text = EditedUser.FirstName ?? string.Empty };
            _lastNameTextBox = new TextBox { Dock = DockStyle.Fill, Text = EditedUser.LastName ?? string.Empty };
            _loginTextBox = new TextBox { Dock = DockStyle.Fill, Text = EditedUser.Login ?? string.Empty };
            _passwordTextBox = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            _roleComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _roleComboBox.Items.AddRange(SystemConstants.Roles.Cast<object>().ToArray());
            _roleComboBox.SelectedItem = EditedUser.Role;
            _isActiveCheckBox = new CheckBox { Dock = DockStyle.Left, Checked = EditedUser.IsActive, Text = "Активен" };

            layout.Controls.Add(new Label { Text = "Имя", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            layout.Controls.Add(_firstNameTextBox, 1, 0);
            layout.Controls.Add(new Label { Text = "Фамилия", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            layout.Controls.Add(_lastNameTextBox, 1, 1);
            layout.Controls.Add(new Label { Text = "Логин", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            layout.Controls.Add(_loginTextBox, 1, 2);
            layout.Controls.Add(new Label { Text = "Пароль", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            layout.Controls.Add(_passwordTextBox, 1, 3);
            layout.Controls.Add(new Label { Text = "Роль", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 4);
            layout.Controls.Add(_roleComboBox, 1, 4);
            layout.Controls.Add(new Label { Text = "Состояние", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 5);
            layout.Controls.Add(_isActiveCheckBox, 1, 5);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 56,
                Padding = new Padding(18, 0, 18, 18),
                BackColor = DesktopTheme.SurfaceBackground
            };

            var okButton = DesktopTheme.CreateButton("Сохранить", true);
            var cancelButton = DesktopTheme.CreateButton("Отмена");
            cancelButton.DialogResult = DialogResult.Cancel;
            okButton.Click += OnOkButtonClick;
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);

            var card = DesktopTheme.CreateCardPanel(layout, new Padding(18));
            card.Dock = DockStyle.Fill;

            Controls.Add(card);
            Controls.Add(buttonPanel);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyDialogTheme(this);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        public User EditedUser { get; }

        public string PlainPassword => string.IsNullOrWhiteSpace(_passwordTextBox.Text) ? null : _passwordTextBox.Text;

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_firstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(_lastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(_loginTextBox.Text))
            {
                MessageBox.Show(this, "Заполните имя, фамилию и логин.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (EditedUser.Id == 0 && string.IsNullOrWhiteSpace(_passwordTextBox.Text))
            {
                MessageBox.Show(this, "Укажите пароль.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EditedUser.FirstName = _firstNameTextBox.Text.Trim();
            EditedUser.LastName = _lastNameTextBox.Text.Trim();
            EditedUser.Login = _loginTextBox.Text.Trim();
            EditedUser.Role = Convert.ToString(_roleComboBox.SelectedItem);
            EditedUser.IsActive = _isActiveCheckBox.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
