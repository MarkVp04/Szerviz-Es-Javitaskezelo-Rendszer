using System.Drawing;
using System.Windows.Forms;
using SzervizApp.Client.Api;
using SzervizApp.Client.Models;
using SzervizApp.Client.UI;

namespace SzervizApp.Client.Forms;

public sealed class LoginForm : Form
{
    private readonly TextBox _apiAddress = AppTheme.TextBox();
    private readonly TextBox _username = AppTheme.TextBox("Felhasználónév");
    private readonly TextBox _password = AppTheme.TextBox("Jelszó");
    private readonly Label _feedback = AppTheme.Label("", 9, false, AppTheme.Red);
    private readonly Button _loginButton = AppTheme.Button("Bejelentkezés", true);

    public ApiClient? Client { get; private set; }
    public LoginResponseDto? User { get; private set; }

    public LoginForm()
    {
        Text = "SzervizApp – Bejelentkezés";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(940, 610);
        BackColor = AppTheme.Background;
        Font = AppTheme.Font();

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        Controls.Add(root);

        var intro = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Sidebar, Padding = new Padding(42) };
        root.Controls.Add(intro, 0, 0);

        var logo = new Label { Text = "SZ", Size = new Size(54, 54), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(53, 82, 133), ForeColor = Color.White, Font = AppTheme.Font(17, true) };
        logo.Location = new Point(42, 48);
        intro.Controls.Add(logo);

        var brand = AppTheme.Label("SzervizApp", 22, true, Color.White);
        brand.Location = new Point(42, 127);
        intro.Controls.Add(brand);
        var tagline = new Label
        {
            Text = "Minden javítás\negy helyen.",
            AutoSize = true,
            Location = new Point(42, 190),
            Font = AppTheme.Font(25, true),
            ForeColor = Color.White,
            UseMnemonic = false
        };
        intro.Controls.Add(tagline);
        var detail = new Label
        {
            Text = "Ügyfelek, eszközök és szervizfeladatok\negyszerű, átlátható kezelése.",
            AutoSize = true,
            Location = new Point(44, 300),
            Font = AppTheme.Font(10),
            ForeColor = AppTheme.SidebarMuted,
            UseMnemonic = false
        };
        intro.Controls.Add(detail);
        var footer = new Label { Text = "SZERVIZ- ÉS JAVÍTÁSKEZELŐ RENDSZER", AutoSize = true, ForeColor = Color.FromArgb(124, 143, 172), Font = AppTheme.Font(8, true), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
        footer.Location = new Point(42, ClientSize.Height - 55);
        intro.Controls.Add(footer);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(54, 52, 54, 40) };
        root.Controls.Add(content, 1, 0);
        var title = AppTheme.Label("Üdv újra!", 23, true);
        title.Location = new Point(54, 68);
        content.Controls.Add(title);
        var subtitle = AppTheme.Label("Jelentkezz be a folytatáshoz.", 10, false, AppTheme.Muted);
        subtitle.Location = new Point(56, 111);
        content.Controls.Add(subtitle);

        //AddField(content, "API cím", _apiAddress, 176);
        _apiAddress.Text = "http://localhost:5203";
        AddField(content, "Felhasználónév", _username, 258);
        AddField(content, "Jelszó", _password, 340);
        _password.UseSystemPasswordChar = true;

        _feedback.Location = new Point(56, 426);
        _feedback.MaximumSize = new Size(350, 38);
        _feedback.AutoSize = true;
        content.Controls.Add(_feedback);

        _loginButton.Location = new Point(54, 474);
        _loginButton.Size = new Size(340, 46);
        _loginButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        AppTheme.StyleButton(_loginButton, true);
        _loginButton.Click += async (_, _) => await LoginAsync();
        content.Controls.Add(_loginButton);

        AcceptButton = _loginButton;
        _username.Select();
    }

    private static void AddField(Control parent, string label, TextBox textBox, int top)
    {
        var caption = AppTheme.Label(label, 9, true, AppTheme.Muted);
        caption.Location = new Point(56, top);
        parent.Controls.Add(caption);
        textBox.Location = new Point(54, top + 23);
        textBox.Width = 340;
        textBox.Height = 38;
        parent.Controls.Add(textBox);
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_username.Text) || string.IsNullOrWhiteSpace(_password.Text))
        {
            _feedback.Text = "Add meg a felhasználónevet és a jelszót.";
            return;
        }

        _loginButton.Enabled = false;
        _loginButton.Text = "Kapcsolódás...";
        _feedback.ForeColor = AppTheme.Muted;
        _feedback.Text = "Kapcsolódás a szerverhez...";
        var client = new ApiClient();
        try
        {
            client.SetBaseAddress(_apiAddress.Text);
            var user = await client.LoginAsync(_username.Text.Trim(), _password.Text);
            Client = client;
            User = user;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            client.Dispose();
            _feedback.ForeColor = AppTheme.Red;
            _feedback.Text = ex.Message;
        }
        finally
        {
            if (!IsDisposed)
            {
                _loginButton.Enabled = true;
                _loginButton.Text = "Bejelentkezés";
            }
        }
    }
}
