using System.Drawing;
using System.Windows.Forms;
using SzervizApp.Client.Api;
using SzervizApp.Client.Models;
using SzervizApp.Client.UI;

namespace SzervizApp.Client.Forms;

internal abstract class EntryDialogBase : Form
{
    protected readonly ApiClient Api;
    protected readonly Panel Content = new() { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(28) };
    protected readonly Button SaveButton = AppTheme.Button("Mentés", true);

    protected EntryDialogBase(ApiClient api, string title, Size size)
    {
        Api = api;
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = size;
        BackColor = AppTheme.Background;
        Font = AppTheme.Font();
        Controls.Add(Content);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 66, BackColor = Color.White, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(22, 13, 22, 10), WrapContents = false };
        Controls.Add(footer);
        var cancel = AppTheme.Button("Mégse");
        AppTheme.StyleButton(cancel);
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        footer.Controls.Add(cancel);
        AppTheme.StyleButton(SaveButton, true);
        SaveButton.Click += async (_, _) => await SaveAsync();
        footer.Controls.Add(SaveButton);
    }

    protected static void AddField(Control parent, string caption, Control input, int top, int left = 0, int width = 430)
    {
        var label = AppTheme.Label(caption, 9, true, AppTheme.Muted);
        label.Location = new Point(left, top);
        parent.Controls.Add(label);
        input.Location = new Point(left, top + 22);
        input.Width = width;
        if (input is TextBox box) box.Height = 36;
        if (input is ComboBox combo) combo.Height = 36;
        parent.Controls.Add(input);
    }

    protected async Task SaveAsync()
    {
        try
        {
            SaveButton.Enabled = false;
            SaveButton.Text = "Mentés...";
            await SaveCoreAsync();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Nem sikerült menteni", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            if (!IsDisposed)
            {
                SaveButton.Enabled = true;
                SaveButton.Text = "Mentés";
            }
        }
    }

    protected abstract Task SaveCoreAsync();
}

internal sealed class CustomerDialog : EntryDialogBase
{
    private readonly TextBox _name = AppTheme.TextBox("Teljes név");
    private readonly TextBox _phone = AppTheme.TextBox("+36 ...");
    private readonly TextBox _email = AppTheme.TextBox("nev@email.hu");
    private readonly TextBox _address = AppTheme.TextBox("Irányítószám, város, utca, házszám");

    public CustomerDialog(ApiClient api) : base(api, "Új ügyfél", new Size(540, 510))
    {
        AddField(Content, "Ügyfél neve *", _name, 28);
        AddField(Content, "Telefonszám *", _phone, 105);
        AddField(Content, "E-mail cím", _email, 182);
        AddField(Content, "Lakcím / számlázási cím", _address, 259);
        var note = AppTheme.Label("Az ügyféladatok felvételéhez nem szükséges jelszó. Bejelentkezési fiókot a manager külön hozhat létre.", 8.5f, false, AppTheme.Muted);
        note.Location = new Point(0, 337);
        note.MaximumSize = new Size(450, 42);
        Content.Controls.Add(note);
    }

    protected override async Task SaveCoreAsync()
    {
        if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_phone.Text))
            throw new InvalidOperationException("Az ügyfél neve és telefonszáma kötelező.");

        await Api.CreateUgyfelAsync(new UgyfelRequest
        {
            Nev = _name.Text.Trim(),
            Telefon = _phone.Text.Trim(),
            Email = NullIfEmpty(_email.Text),
            Cim = NullIfEmpty(_address.Text),
            Aktiv = true
        });
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

internal sealed class DeviceDialog : EntryDialogBase
{
    private readonly ComboBox _owner = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10) };
    private readonly TextBox _type = AppTheme.TextBox("Pl. Laptop, mobiltelefon, nyomtató");
    private readonly TextBox _brand = AppTheme.TextBox("Gyártó");
    private readonly TextBox _serial = AppTheme.TextBox("Gyári szám");
    private readonly TextBox _note = AppTheme.TextBox("Egyéb tudnivalók");
    private readonly DateTimePicker _purchaseDate = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Font = AppTheme.Font(10) };

    public DeviceDialog(ApiClient api) : base(api, "Új eszköz rögzítése", new Size(560, 630))
    {
        AddField(Content, "Tulajdonos *", _owner, 22);
        AddField(Content, "Eszköz típusa *", _type, 95);
        AddField(Content, "Gyártó", _brand, 168);
        AddField(Content, "Sorozatszám", _serial, 241);
        AddField(Content, "Vásárlás dátuma", _purchaseDate, 314);
        AddField(Content, "Megjegyzés", _note, 387);
        Shown += async (_, _) => await LoadOwnersAsync();
    }

    private async Task LoadOwnersAsync()
    {
        try
        {
            var customers = (await Api.GetUgyfelekAsync()).Where(x => x.Aktiv).ToList();
            _owner.DataSource = customers;
            _owner.DisplayMember = nameof(UgyfelDto.Nev);
            _owner.ValueMember = nameof(UgyfelDto.Id);
            if (customers.Count == 0)
                MessageBox.Show(this, "Először rögzíts egy ügyfelet.", "Nincs választható ügyfél", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    protected override async Task SaveCoreAsync()
    {
        if (_owner.SelectedValue is null || !int.TryParse(_owner.SelectedValue.ToString(), out var ownerId))
            throw new InvalidOperationException("Válassz tulajdonost.");
        if (string.IsNullOrWhiteSpace(_type.Text))
            throw new InvalidOperationException("Az eszköz típusa kötelező.");

        await Api.CreateEszkozAsync(new EszkozRequest
        {
            TulajdonosId = ownerId,
            EszkozTipus = _type.Text.Trim(),
            Gyarto = NullIfEmpty(_brand.Text),
            Sorozatszam = NullIfEmpty(_serial.Text),
            VasarlasDatuma = _purchaseDate.Checked ? new DateTimeOffset(_purchaseDate.Value.Date) : null,
            Megjegyzes = NullIfEmpty(_note.Text)
        });
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

internal sealed class RepairDialog : EntryDialogBase
{
    private readonly ComboBox _customer = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10) };
    private readonly ComboBox _device = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10) };
    private readonly TextBox _issue = AppTheme.TextBox("Írd le röviden a tapasztalt hibát...");
    private readonly TextBox _note = AppTheme.TextBox("Opcionális megjegyzés");
    private readonly CheckBox _warranty = new() { Text = "Garanciális javítás", AutoSize = true, Font = AppTheme.Font(9.5f), ForeColor = AppTheme.Ink };
    private List<UgyfelDto> _customers = [];
    private List<EszkozListaDto> _devices = [];

    public RepairDialog(ApiClient api) : base(api, "Új javítás felvétele", new Size(610, 690))
    {
        AddField(Content, "Ügyfél *", _customer, 22);
        AddField(Content, "Eszköz *", _device, 95);
        AddField(Content, "Bejelentett hiba *", _issue, 168);
        _issue.Multiline = true;
        _issue.Height = 88;
        _issue.ScrollBars = ScrollBars.Vertical;
        AddField(Content, "Megjegyzés", _note, 286);
        _warranty.Location = new Point(0, 365);
        Content.Controls.Add(_warranty);
        var hint = AppTheme.Label("A mentés után a javítás 'Átvett' állapotban jön létre.", 8.5f, false, AppTheme.Muted);
        hint.Location = new Point(0, 410);
        Content.Controls.Add(hint);
        _customer.SelectedIndexChanged += (_, _) => FilterDevices();
        Shown += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var customerTask = Api.GetUgyfelekAsync();
            var deviceTask = Api.GetEszkozokAsync();
            await Task.WhenAll(customerTask, deviceTask);
            _customers = customerTask.Result.Where(x => x.Aktiv).ToList();
            _devices = deviceTask.Result;
            _customer.DataSource = _customers;
            _customer.DisplayMember = nameof(UgyfelDto.Nev);
            _customer.ValueMember = nameof(UgyfelDto.Id);
            FilterDevices();
            if (_customers.Count == 0)
                MessageBox.Show(this, "Nincs még ügyfél. Új javítás előtt rögzíts egy ügyfelet.", "Hiányzó ügyfél", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void FilterDevices()
    {
        if (_customer.SelectedValue is null || !int.TryParse(_customer.SelectedValue.ToString(), out var ownerId))
        {
            _device.DataSource = null;
            return;
        }
        var devices = _devices.Where(x => x.TulajdonosId == ownerId).ToList();
        _device.DataSource = devices;
        _device.DisplayMember = nameof(EszkozListaDto.Megjelenites);
        _device.ValueMember = nameof(EszkozListaDto.Id);
    }

    protected override async Task SaveCoreAsync()
    {
        if (_customer.SelectedValue is null || !int.TryParse(_customer.SelectedValue.ToString(), out var customerId))
            throw new InvalidOperationException("Válassz ügyfelet.");
        if (_device.SelectedValue is null || !int.TryParse(_device.SelectedValue.ToString(), out var deviceId))
            throw new InvalidOperationException("Ehhez az ügyfélhez még nincs eszköz rögzítve.");
        if (string.IsNullOrWhiteSpace(_issue.Text))
            throw new InvalidOperationException("A bejelentett hiba megadása kötelező.");

        await Api.CreateJavitasAsync(new UjJavitasRequest
        {
            UgyfelId = customerId,
            EszkozId = deviceId,
            BejelentettHiba = _issue.Text.Trim(),
            Garancialis = _warranty.Checked,
            Megjegyzes = string.IsNullOrWhiteSpace(_note.Text) ? null : _note.Text.Trim()
        });
    }
}
