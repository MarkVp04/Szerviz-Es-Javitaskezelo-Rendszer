using System.Drawing;
using System.Windows.Forms;
namespace SzervizApp.Client.UI;

internal static class AppTheme
{
    public static readonly Color Ink = Color.FromArgb(28, 39, 58);
    public static readonly Color Muted = Color.FromArgb(111, 125, 146);
    public static readonly Color Background = Color.FromArgb(245, 247, 251);
    public static readonly Color Surface = Color.White;
    public static readonly Color Sidebar = Color.FromArgb(24, 37, 58);
    public static readonly Color SidebarMuted = Color.FromArgb(167, 180, 200);
    public static readonly Color Accent = Color.FromArgb(62, 105, 220);
    public static readonly Color AccentLight = Color.FromArgb(235, 241, 255);
    public static readonly Color Border = Color.FromArgb(226, 231, 239);
    public static readonly Color Green = Color.FromArgb(26, 148, 105);
    public static readonly Color Orange = Color.FromArgb(205, 130, 34);
    public static readonly Color Red = Color.FromArgb(194, 71, 78);

    public static Font Font(float size = 10, bool bold = false)
        => new("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);

    public static Button Button(string text, bool primary = false)
        => new()
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(104, 38),
            Padding = new Padding(14, 0, 14, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Accent : Surface,
            ForeColor = primary ? Color.White : Ink,
            Font = Font(9.5f, true),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0),
            UseVisualStyleBackColor = false
        };

    public static void StyleButton(Button button, bool primary = false)
    {
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(48, 89, 197) : AccentLight;
        button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(39, 75, 172) : Border;
    }

    public static Label Label(string text, float size = 10, bool bold = false, Color? color = null)
        => new()
        {
            Text = text,
            AutoSize = true,
            Font = Font(size, bold),
            ForeColor = color ?? Ink,
            Margin = new Padding(0, 0, 0, 4)
        };

    public static TextBox TextBox(string placeholder = "")
        => new()
        {
            BorderStyle = BorderStyle.FixedSingle,
            Font = Font(10),
            ForeColor = Ink,
            BackColor = Color.White,
            PlaceholderText = placeholder,
            Height = 38,
            Margin = new Padding(0, 2, 0, 14)
        };

    public static Panel Card()
        => new()
        {
            BackColor = Surface,
            Padding = new Padding(20),
            Margin = new Padding(0, 0, 14, 14),
            BorderStyle = BorderStyle.FixedSingle
        };

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
        grid.ColumnHeadersDefaultCellStyle.Font = Font(9, true);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 8, 10, 8);
        grid.ColumnHeadersHeight = 42;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Ink;
        grid.DefaultCellStyle.Font = Font(9.5f);
        grid.DefaultCellStyle.Padding = new Padding(10, 8, 10, 8);
        grid.DefaultCellStyle.SelectionBackColor = AccentLight;
        grid.DefaultCellStyle.SelectionForeColor = Ink;
        grid.RowTemplate.Height = 44;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 252, 254);
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = true;
    }

    public static string StatusLabel(string status) => status switch
    {
        "Atvett" => "Átvett",
        "BevizsgalasAlatt" => "Bevizsgálás alatt",
        "JavitasAlatt" => "Javítás alatt",
        "AlkatreszreVar" => "Alkatrészre vár",
        "Lezart" => "Lezárt",
        "Visszaadva" => "Visszaadva",
        _ => status
    };

    public static Color StatusColor(string status) => status switch
    {
        "Atvett" => Color.FromArgb(80, 111, 175),
        "BevizsgalasAlatt" => Orange,
        "JavitasAlatt" => Accent,
        "AlkatreszreVar" => Color.FromArgb(146, 103, 174),
        "Lezart" => Green,
        "Visszaadva" => Muted,
        _ => Muted
    };
}
