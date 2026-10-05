using System;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms
{
    public partial class ComparePeriodDialog : Form
    {
        public DateTime OtherStart => _dtpOtherStart.Value.Date;
        public DateTime OtherEnd => _dtpOtherEnd.Value.Date;

        public ComparePeriodDialog()
        {
            InitializeComponent();
            _dtpOtherStart.Value = DateTime.Now.Date.AddDays(-14);
            _dtpOtherEnd.Value = DateTime.Now.Date.AddDays(-8);
        }

        private void OnOkClick(object? sender, EventArgs e)
        {
            if (OtherStart > OtherEnd)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
