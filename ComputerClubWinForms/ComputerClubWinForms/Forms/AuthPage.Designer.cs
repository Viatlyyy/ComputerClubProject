#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class AuthPage
{
    private Label _titleLabel = null!;
    private Label _loginLabel = null!;
    private Label _passwordLabel = null!;
    private TextBox _usernameTextBox = null!;
    private TextBox _passwordTextBox = null!;
    private Label _errorLabel = null!;
    private Button _loginButton = null!;

    private void InitializeComponent()
    {
        _titleLabel = new Label();
        _loginLabel = new Label();
        _usernameTextBox = new TextBox();
        _passwordLabel = new Label();
        _passwordTextBox = new TextBox();
        _loginButton = new Button();
        _errorLabel = new Label();
        SuspendLayout();
        _titleLabel.AutoSize = true;
        _titleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _titleLabel.Location = new Point(26, 26);
        _titleLabel.Name = "_titleLabel";
        _titleLabel.Size = new Size(96, 15);
        _titleLabel.TabIndex = 0;
        _titleLabel.Text = "Вход в систему";
        _loginLabel.AutoSize = true;
        _loginLabel.Location = new Point(26, 64);
        _loginLabel.Name = "_loginLabel";
        _loginLabel.Size = new Size(41, 15);
        _loginLabel.TabIndex = 1;
        _loginLabel.Text = "Логин";
        _usernameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _usernameTextBox.Location = new Point(26, 86);
        _usernameTextBox.Name = "_usernameTextBox";
        _usernameTextBox.PlaceholderText = "Логин";
        _usernameTextBox.Size = new Size(358, 23);
        _usernameTextBox.TabIndex = 2;
        _passwordLabel.AutoSize = true;
        _passwordLabel.Location = new Point(26, 126);
        _passwordLabel.Name = "_passwordLabel";
        _passwordLabel.Size = new Size(49, 15);
        _passwordLabel.TabIndex = 3;
        _passwordLabel.Text = "Пароль";
        _passwordTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _passwordTextBox.Location = new Point(26, 148);
        _passwordTextBox.Name = "_passwordTextBox";
        _passwordTextBox.PlaceholderText = "Пароль";
        _passwordTextBox.Size = new Size(358, 23);
        _passwordTextBox.TabIndex = 4;
        _passwordTextBox.UseSystemPasswordChar = true;
        _loginButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _loginButton.Location = new Point(26, 194);
        _loginButton.Name = "_loginButton";
        _loginButton.Size = new Size(358, 36);
        _loginButton.TabIndex = 5;
        _loginButton.Text = "Войти";
        _loginButton.UseVisualStyleBackColor = true;
        _loginButton.Click += OnLoginClick;
        _errorLabel.AutoSize = true;
        _errorLabel.ForeColor = Color.DarkRed;
        _errorLabel.Location = new Point(26, 248);
        _errorLabel.Name = "_errorLabel";
        _errorLabel.Size = new Size(0, 15);
        _errorLabel.TabIndex = 6;
        AcceptButton = _loginButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(414, 320);
        Controls.Add(_titleLabel);
        Controls.Add(_loginLabel);
        Controls.Add(_usernameTextBox);
        Controls.Add(_passwordLabel);
        Controls.Add(_passwordTextBox);
        Controls.Add(_loginButton);
        Controls.Add(_errorLabel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimumSize = new Size(430, 350);
        Name = "AuthPage";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Авторизация — компьютерный клуб";
        ResumeLayout(false);
        PerformLayout();
    }
}
