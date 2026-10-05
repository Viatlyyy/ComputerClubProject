using System.Windows.Forms;

namespace ComputerClubWinForms.Forms
{
    public partial class ExtendSessionDialog : Form
    {
        public int AdditionalMinutes => (int)_numMinutes.Value;

        public ExtendSessionDialog()
        {
            InitializeComponent();
        }
    }
}
