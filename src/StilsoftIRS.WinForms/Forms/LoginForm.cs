using System;
using System.Drawing;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;
using StilsoftIRS.Presenters;
using StilsoftIRS.Utilities;
using StilsoftIRS.Views;

namespace StilsoftIRS.Forms
{
    internal sealed class LoginForm : Form, ILoginView
    {
        private readonly AppServices _services;
        private readonly TextBox _loginTextBox;
        private readonly TextBox _passwordTextBox;
        private readonly LoginPresenter _presenter;

        public event EventHandler LoginRequested;

        public string Login => _loginTextBox.Text;
        public string Password => _passwordTextBox.Text;

        public LoginForm(AppServices services)
        {
            _services = services;

            Text = "StilsoftIRS - Вход";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 290);

            var heroPanel = DesktopTheme.CreateHeroPanel("StilsoftIRS", null);

            var authLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            authLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            authLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            authLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            authLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            authLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _loginTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12) };
            _passwordTextBox = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Margin = new Padding(0, 0, 0, 12) };

            authLayout.Controls.Add(new Label { Text = "Логин", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            authLayout.Controls.Add(_loginTextBox, 1, 0);
            authLayout.Controls.Add(new Label { Text = "Пароль", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            authLayout.Controls.Add(_passwordTextBox, 1, 1);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 12, 0, 0)
            };
            var loginButton = DesktopTheme.CreateButton("Войти", true);
            var closeButton = DesktopTheme.CreateButton("Выход", false, true);
            loginButton.Click += (s, e) => LoginRequested?.Invoke(this, EventArgs.Empty);
            closeButton.Click += (s, e) => Close();
            buttonPanel.Controls.Add(loginButton);
            buttonPanel.Controls.Add(closeButton);

            authLayout.Controls.Add(buttonPanel, 0, 2);
            authLayout.SetColumnSpan(buttonPanel, 2);

            var card = DesktopTheme.CreateCardPanel(authLayout, new Padding(18));
            card.Dock = DockStyle.Fill;

            Controls.Add(card);
            Controls.Add(heroPanel);

            DesktopTheme.ApplyDialogTheme(this);
            AcceptButton = loginButton;
            CancelButton = closeButton;

            _presenter = new LoginPresenter(this, services);
        }

        public void ShowError(string message) =>
            MessageBox.Show(this, message, "Вход", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void NavigateToMain()
        {
            Hide();
            using (var mainForm = new MainForm(_services))
                mainForm.ShowDialog(this);
            Close();
        }
    }
}
