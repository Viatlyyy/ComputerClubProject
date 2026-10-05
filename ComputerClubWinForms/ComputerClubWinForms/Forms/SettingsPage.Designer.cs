#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

partial class SettingsPage
{
    private Label _lblCurrentTariff = null!;
    private Label _newTariffLabel = null!;
    private TextBox _txtTariff = null!;
    private Label _noteLabel = null!;
    private Button _btnSaveTariff = null!;

    private void InitializeComponent()
    {
        _lblCurrentTariff = new Label();
        _newTariffLabel = new Label();
        _txtTariff = new TextBox();
        _noteLabel = new Label();
        _btnSaveTariff = new Button();
        SuspendLayout();
        _lblCurrentTariff.AutoSize = true;
        _lblCurrentTariff.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblCurrentTariff.Location = new Point(20, 20);
        _lblCurrentTariff.Name = "_lblCurrentTariff";
        _lblCurrentTariff.Size = new Size(192, 15);
        _lblCurrentTariff.TabIndex = 0;
        _lblCurrentTariff.Text = "Текущий тариф: 150,00 руб./час";
        _newTariffLabel.AutoSize = true;
        _newTariffLabel.Location = new Point(20, 64);
        _newTariffLabel.Name = "_newTariffLabel";
        _newTariffLabel.Size = new Size(135, 15);
        _newTariffLabel.TabIndex = 1;
        _newTariffLabel.Text = "Новый тариф, руб./час";
        _txtTariff.Location = new Point(210, 60);
        _txtTariff.Name = "_txtTariff";
        _txtTariff.PlaceholderText = "Например: 150,00";
        _txtTariff.Size = new Size(230, 23);
        _txtTariff.TabIndex = 2;
        _noteLabel.AutoSize = true;
        _noteLabel.ForeColor = Color.DimGray;
        _noteLabel.Location = new Point(20, 103);
        _noteLabel.Name = "_noteLabel";
        _noteLabel.Size = new Size(291, 15);
        _noteLabel.TabIndex = 3;
        _noteLabel.Text = "Старые завершённые сеансы не пересчитываются.";
        _btnSaveTariff.Location = new Point(210, 140);
        _btnSaveTariff.Name = "_btnSaveTariff";
        _btnSaveTariff.Size = new Size(120, 27);
        _btnSaveTariff.TabIndex = 4;
        _btnSaveTariff.Text = "Сохранить";
        _btnSaveTariff.UseVisualStyleBackColor = true;
        _btnSaveTariff.Click += OnSaveTariffClick;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(_lblCurrentTariff);
        Controls.Add(_newTariffLabel);
        Controls.Add(_txtTariff);
        Controls.Add(_noteLabel);
        Controls.Add(_btnSaveTariff);
        Name = "SettingsPage";
        Padding = new Padding(20);
        Size = new Size(1556, 755);
        ResumeLayout(false);
        PerformLayout();
    }
}
