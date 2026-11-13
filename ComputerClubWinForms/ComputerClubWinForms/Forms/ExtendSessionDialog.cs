using System.Windows.Forms;

namespace ComputerClubWinForms.Forms
{
    public partial class ExtendSessionDialog : Form
    {
        public int AdditionalMinutes => (int)_numMinutes.Value;

        public ExtendSessionDialog()
        {
            InitializeComponent();
            UiTheme.Apply(this);
            _numMinutes.Font = new System.Drawing.Font("Segoe UI", 9F);
            UiTheme.Action(_btnOk, primary: true);
        }
    }
}
