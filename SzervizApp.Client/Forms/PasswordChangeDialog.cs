using System.Drawing;
using System.Windows.Forms;
using SzervizApp.Client.Models;
using SzervizApp.Client.UI;

namespace SzervizApp.Client.Forms;

internal sealed class PasswordChangeDialog : Form
{
    private readonly TextBox _current = DialogUi.TextBox(48, 24, 440);
    private readonly TextBox _newPassword = DialogUi.TextBox(126, 24, 440);
    private readonly TextBox _confirm = DialogUi.TextBox(204, 24, 440);

    public JelszoModositasRequest Request { get; private set; } = new("", "");

    public PasswordChangeDialog()
    {
        DialogUi.Configure(this, "Jelszó módosítása", new Size(510, 360));
        _current.UseSystemPasswordChar = true;
        _newPassword.UseSystemPasswordChar = true;
        _confirm.UseSystemPasswordChar = true;

        Controls.Add(DialogUi.Label("Jelenlegi jelszó", 22, 24, true));
        Controls.Add(_current);
        Controls.Add(DialogUi.Label("Új jelszó (legalább 10 karakter)", 100, 24, true));
        Controls.Add(_newPassword);
        Controls.Add(DialogUi.Label("Új jelszó megerősítése", 178, 24, true));
        Controls.Add(_confirm);

        var hint = AppTheme.Label("A mentés után a rendszer automatikusan frissíti a bejelentkezési munkamenetet.",
            8.2f, false, AppTheme.Muted);
        hint.Location = new Point(24, 250);
        hint.MaximumSize = new Size(450, 34);
        Controls.Add(hint);

        DialogUi.AddFooter(this, Save);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_current.Text))
        {
            MessageBox.Show(this, "Add meg a jelenlegi jelszavadat.", "Hiányzó adat",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_newPassword.Text.Length < 10 || _newPassword.Text.Length > 128)
        {
            MessageBox.Show(this, "Az új jelszó legalább 10, legfeljebb 128 karakter legyen.", "Hibás jelszó",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!string.Equals(_newPassword.Text, _confirm.Text, StringComparison.Ordinal))
        {
            MessageBox.Show(this, "A két új jelszó nem egyezik.", "Jelszó megerősítése",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.Equals(_current.Text, _newPassword.Text, StringComparison.Ordinal))
        {
            MessageBox.Show(this, "Az új jelszó különbözzön a jelenlegitől.", "Hibás jelszó",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Request = new JelszoModositasRequest(_current.Text, _newPassword.Text);
        DialogResult = DialogResult.OK;
        Close();
    }
}
