#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class SettingsPage
{
    private Label _titleLabel = null!;
    private Label _lblCurrentTariff = null!;
    private Label _newTariffLabel = null!;
    private TextBox _txtTariff = null!;
    private Button _btnSaveTariff = null!;

    private void InitializeComponent()
    {
        _titleLabel = new Label();
        _lblCurrentTariff = new Label();
        _newTariffLabel = new Label();
        _txtTariff = new TextBox();
        _btnSaveTariff = new Button();
        SuspendLayout();
        _titleLabel.AutoSize = true;
        _titleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _titleLabel.Location = new Point(24, 20);
        _titleLabel.Name = "_titleLabel";
        _titleLabel.Text = "Настройки";
        _titleLabel.TabIndex = 0;
        _lblCurrentTariff.AutoSize = true;
        _lblCurrentTariff.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblCurrentTariff.Location = new Point(24, 80);
        _lblCurrentTariff.Name = "_lblCurrentTariff";
        _lblCurrentTariff.Size = new Size(192, 15);
        _lblCurrentTariff.TabIndex = 0;
        _lblCurrentTariff.Text = "Текущий тариф: 150,00 руб./час";
        _newTariffLabel.AutoSize = true;
        _newTariffLabel.Location = new Point(24, 140);
        _newTariffLabel.Name = "_newTariffLabel";
        _newTariffLabel.Size = new Size(135, 15);
        _newTariffLabel.TabIndex = 1;
        _newTariffLabel.Text = "Новый тариф, руб./час";
        _txtTariff.Location = new Point(24, 170);
        _txtTariff.Name = "_txtTariff";
        _txtTariff.PlaceholderText = "Например: 150,00";
        _txtTariff.Size = new Size(350, 27);
        _txtTariff.TabIndex = 2;
        _btnSaveTariff.Location = new Point(24, 218);
        _btnSaveTariff.Name = "_btnSaveTariff";
        _btnSaveTariff.Size = new Size(180, 44);
        _btnSaveTariff.TabIndex = 4;
        _btnSaveTariff.Text = "Сохранить";
        _btnSaveTariff.UseVisualStyleBackColor = true;
        _btnSaveTariff.Click += OnSaveTariffClick;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 9F);
        Controls.Add(_titleLabel);
        Controls.Add(_lblCurrentTariff);
        Controls.Add(_newTariffLabel);
        Controls.Add(_txtTariff);
        Controls.Add(_btnSaveTariff);
        Name = "SettingsPage";
        Padding = new Padding(24);
        Size = new Size(1000, 646);
        ResumeLayout(false);
        PerformLayout();
    }
}
