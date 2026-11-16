using System.Drawing;
using System.Windows.Forms;

namespace ComputerClubWinForms.Forms;

internal static class UiTheme
{
    internal static readonly Font BodyFont = new Font("Segoe UI", 9F);
    internal static readonly Font HeadingFont = new Font("Segoe UI", 9F, FontStyle.Bold);
    private static readonly Font TabFont = new Font("Segoe UI", 10.5F);
    internal static readonly Color Background = SystemColors.Control;
    internal static readonly Color Muted = Color.DimGray;

    internal static void Apply(Control root)
    {
        root.Font = BodyFont;
        root.BackColor = SystemColors.Control;
        root.ForeColor = SystemColors.ControlText;
        foreach (Control control in root.Controls)
            ApplyChild(control);
    }

    private static void ApplyChild(Control control)
    {
        control.Font = BodyFont;
        control.ForeColor = SystemColors.ControlText;
        switch (control)
        {
            case Button button:
                Action(button);
                break;
            case DataGridView grid:
                Grid(grid);
                return;
            case TextBox text:
                text.BorderStyle = BorderStyle.Fixed3D;
                text.BackColor = text.ReadOnly ? SystemColors.Control : SystemColors.Window;
                break;
            case ComboBox combo:
                combo.BackColor = SystemColors.Window;
                break;
            case GroupBox group:
                group.BackColor = SystemColors.Control;
                break;
            case TabPage page:
                page.BackColor = SystemColors.Control;
                break;
        }
        foreach (Control child in control.Controls)
            ApplyChild(child);
    }

    internal static void Action(Button button, bool primary = false, bool danger = false)
    {
        button.FlatStyle = FlatStyle.Standard;
        button.BackColor = SystemColors.Control;
        button.ForeColor = SystemColors.ControlText;
        button.UseVisualStyleBackColor = true;
        button.MinimumSize = new Size(0, 40);
        button.Padding = new Padding(8, 0, 8, 0);
        button.AutoEllipsis = true;
        button.Cursor = Cursors.Default;
    }

    internal static void Tabs(TabControl tabs)
    {
        tabs.Font = TabFont;
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.ItemSize = new Size(168, 34);
    }

    internal static void Grid(DataGridView grid)
    {
        grid.BackgroundColor = SystemColors.AppWorkspace;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.EnableHeadersVisualStyles = true;
        grid.RowHeadersVisible = true;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        grid.GridColor = SystemColors.ControlDark;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 38;
        grid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.WindowText;
        grid.ColumnHeadersDefaultCellStyle.Font = BodyFont;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
        grid.DefaultCellStyle.BackColor = SystemColors.Window;
        grid.DefaultCellStyle.ForeColor = SystemColors.ControlText;
        grid.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
        grid.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
        grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        grid.RowTemplate.Height = 34;
        grid.AllowUserToResizeRows = false;
    }

    internal static void ComputerButton(Button button, bool available, bool selected = false)
    {
        button.FlatStyle = selected ? FlatStyle.Popup : FlatStyle.Standard;
        button.Font = BodyFont;
        button.ForeColor = SystemColors.ControlText;
        button.BackColor = available ? Color.LightGreen : Color.LightCoral;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Default;
        button.AccessibleName = button.Text + (available ? ", свободен" : ", занят");
    }
}
