using System.Drawing;
using System.Windows.Forms;
using SzervizApp.Client.Api;
using SzervizApp.Client.Models;
using SzervizApp.Client.UI;

namespace SzervizApp.Client.Forms;

internal static class DialogUi
{
    public static Label Label(string text, int top, int left = 0, bool bold = false)
    {
        var label = AppTheme.Label(text, 9, bold, bold ? AppTheme.Ink : AppTheme.Muted);
        label.Location = new Point(left, top);
        return label;
    }

    public static TextBox TextBox(int top = 0, int left = 0, int width = 400, int height = 34, bool multiline = false)
    {
        var box = AppTheme.TextBox();
        box.Location = new Point(left, top);
        box.Size = new Size(width, height);
        box.Multiline = multiline;
        if (multiline) box.ScrollBars = ScrollBars.Vertical;
        return box;
    }

    public static void AddFooter(Form form, Action save)
    {
        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            BackColor = Color.White,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(18, 12, 18, 8),
            WrapContents = false
        };
        var cancel = AppTheme.Button("Mégse");
        AppTheme.StyleButton(cancel);
        cancel.Click += (_, _) => { form.DialogResult = DialogResult.Cancel; form.Close(); };
        var ok = AppTheme.Button("Mentés", true);
        AppTheme.StyleButton(ok, true);
        ok.Click += (_, _) => save();
        footer.Controls.Add(cancel);
        footer.Controls.Add(ok);
        form.Controls.Add(footer);
    }

    public static void Configure(Form form, string title, Size size)
    {
        form.Text = title;
        form.StartPosition = FormStartPosition.CenterParent;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.ShowInTaskbar = false;
        form.ClientSize = size;
        form.BackColor = AppTheme.Background;
        form.Font = AppTheme.Font();
    }
}

internal sealed class BevizsgalasDialog : Form
{
    private readonly TextBox _result = DialogUi.TextBox(54, 24, 520, 150, true);
    private readonly NumericUpDown _estimate = new() { Minimum = 0, Maximum = 100000000, DecimalPlaces = 2, ThousandsSeparator = true, Width = 210, Font = AppTheme.Font(10) };
    private readonly CheckBox _hasEstimate = new() { Text = "Becsült költség megadása", AutoSize = true, Font = AppTheme.Font(9), ForeColor = AppTheme.Ink };
    public BevizsgalasRequest Request { get; private set; } = new();

    public BevizsgalasDialog(BevizsgalasDto? existing)
    {
        DialogUi.Configure(this, "Bevizsgálás", new Size(590, 360));
        Controls.Add(DialogUi.Label("Bevizsgálás eredménye *", 24, 24, true));
        _result.Text = existing?.Eredmeny ?? "";
        Controls.Add(_result);
        _hasEstimate.Location = new Point(24, 226);
        _hasEstimate.Checked = existing?.BecsultKoltseg.HasValue == true;
        Controls.Add(_hasEstimate);
        _estimate.Location = new Point(24, 258);
        _estimate.Value = Math.Min(_estimate.Maximum, Math.Max(_estimate.Minimum, existing?.BecsultKoltseg ?? 0));
        _estimate.Enabled = _hasEstimate.Checked;
        _hasEstimate.CheckedChanged += (_, _) => _estimate.Enabled = _hasEstimate.Checked;
        Controls.Add(_estimate);
        DialogUi.AddFooter(this, Save);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_result.Text))
        {
            MessageBox.Show(this, "A bevizsgálás eredményét add meg.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Request = new BevizsgalasRequest { Eredmeny = _result.Text.Trim(), BecsultKoltseg = _hasEstimate.Checked ? _estimate.Value : null };
        DialogResult = DialogResult.OK;
        Close();
    }
}

internal sealed class WorkLogDialog : Form
{
    private readonly TextBox _description = DialogUi.TextBox(54, 24, 520, 92, true);
    private readonly DateTimePicker _start = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy. MM. dd. HH:mm", Width = 250, Font = AppTheme.Font(9) };
    private readonly DateTimePicker _end = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy. MM. dd. HH:mm", Width = 250, Font = AppTheme.Font(9) };
    private readonly NumericUpDown _hourlyRate = new() { Minimum = 0, Maximum = 10000000, DecimalPlaces = 2, ThousandsSeparator = true, Width = 210, Font = AppTheme.Font(10), Value = 5000 };
    public MunkanaploRequest Request { get; private set; } = new();

    public WorkLogDialog(DateTime earliest)
    {
        DialogUi.Configure(this, "Munkanapló bejegyzés", new Size(590, 390));
        Controls.Add(DialogUi.Label("Elvégzett munka leírása *", 24, 24, true));
        Controls.Add(_description);
        Controls.Add(DialogUi.Label("Kezdés", 158, 24, true));
        Controls.Add(DialogUi.Label("Befejezés", 158, 300, true));
        _start.Location = new Point(24, 184); _end.Location = new Point(300, 184);
        var now = DateTime.Now;
        _start.Value = earliest > now.AddHours(-1) ? now.AddMinutes(-30) : now.AddHours(-1);
        if (_start.Value < earliest) _start.Value = earliest;
        _end.Value = now;
        Controls.Add(_start); Controls.Add(_end);
        Controls.Add(DialogUi.Label("Óradíj (Ft) *", 232, 24, true));
        _hourlyRate.Location = new Point(24, 258);
        Controls.Add(_hourlyRate);
        var hint = AppTheme.Label("Egyszerű, lezárt munkanapló-bejegyzést rögzítünk.", 8.5f, false, AppTheme.Muted);
        hint.Location = new Point(24, 300); Controls.Add(hint);
        DialogUi.AddFooter(this, Save);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_description.Text))
        {
            MessageBox.Show(this, "A munka leírása kötelező.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_end.Value < _start.Value || _end.Value > DateTime.Now.AddMinutes(1) || _start.Value > DateTime.Now.AddMinutes(1))
        {
            MessageBox.Show(this, "Ellenőrizd a kezdés és befejezés időpontját.", "Hibás időpont", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Request = new MunkanaploRequest
        {
            Leiras = _description.Text.Trim(),
            Kezdes = new DateTimeOffset(_start.Value),
            Befejezes = new DateTimeOffset(_end.Value),
            Oradij = _hourlyRate.Value
        };
        DialogResult = DialogResult.OK; Close();
    }
}

internal sealed class CostDialog : Form
{
    private readonly TextBox _name = DialogUi.TextBox(54, 24, 500);
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10), Width = 230 };
    private readonly NumericUpDown _amount = new() { Minimum = 0, Maximum = 100000000, DecimalPlaces = 2, ThousandsSeparator = true, Width = 230, Font = AppTheme.Font(10) };
    public KoltsegRequest Request { get; private set; } = new();

    public CostDialog()
    {
        DialogUi.Configure(this, "Egyéb költség rögzítése", new Size(570, 330));
        Controls.Add(DialogUi.Label("Megnevezés *", 24, 24, true)); Controls.Add(_name);
        Controls.Add(DialogUi.Label("Költség típusa", 104, 24, true));
        _type.Items.AddRange(["Egyeb", "Szallitas", "Bevizsgalas"]); _type.SelectedIndex = 0;
        _type.Location = new Point(24, 130); Controls.Add(_type);
        Controls.Add(DialogUi.Label("Összeg (Ft) *", 184, 24, true));
        _amount.Location = new Point(24, 210); Controls.Add(_amount);
        DialogUi.AddFooter(this, Save);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            MessageBox.Show(this, "A költség megnevezése kötelező.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Request = new KoltsegRequest { Megnevezes = _name.Text.Trim(), Tipus = Convert.ToString(_type.SelectedItem) ?? "Egyeb", Osszeg = _amount.Value };
        DialogResult = DialogResult.OK; Close();
    }
}

internal sealed class PartUseDialog : Form
{
    private readonly ComboBox _part = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(9), Width = 470 };
    private readonly NumericUpDown _quantity = new() { Minimum = 0.001m, Maximum = 100000m, DecimalPlaces = 3, Increment = 0.1m, Width = 180, Font = AppTheme.Font(10), Value = 1 };
    public AlkatreszFelhasznalasRequest Request { get; private set; } = new();

    public PartUseDialog(List<AlkatreszDto> parts)
    {
        DialogUi.Configure(this, "Alkatrész felhasználása", new Size(570, 270));
        Controls.Add(DialogUi.Label("Alkatrész *", 24, 24, true));
        _part.DataSource = parts; _part.DisplayMember = nameof(AlkatreszDto.Megjelenites); _part.ValueMember = nameof(AlkatreszDto.Id);
        _part.Location = new Point(24, 50); Controls.Add(_part);
        Controls.Add(DialogUi.Label("Mennyiség *", 112, 24, true)); _quantity.Location = new Point(24, 138); Controls.Add(_quantity);
        DialogUi.AddFooter(this, Save);
    }

    private void Save()
    {
        if (_part.SelectedValue is null || !int.TryParse(Convert.ToString(_part.SelectedValue), out var partId))
        {
            MessageBox.Show(this, "Nincs választható aktív alkatrész.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Request = new AlkatreszFelhasznalasRequest { AlkatreszId = partId, Mennyiseg = _quantity.Value };
        DialogResult = DialogResult.OK; Close();
    }
}

internal sealed class AssignMechanicDialog : Form
{
    private readonly ComboBox _employee = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10), Width = 430 };
    private readonly DateTimePicker _deadline = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Font = AppTheme.Font(10), Width = 220 };
    public FelelosRequest Request { get; private set; } = new();

    public AssignMechanicDialog(List<FelhasznaloDto> users, JavitasDto repair)
    {
        DialogUi.Configure(this, "Felelős munkatárs kijelölése", new Size(540, 290));
        Controls.Add(DialogUi.Label("Felelős szerelő / manager", 24, 24, true));
        _employee.DataSource = users.Where(x => x.Aktiv && (x.Szerepkor == "Szerelo" || x.Szerepkor == "Manager")).ToList();
        _employee.DisplayMember = nameof(FelhasznaloDto.Nev); _employee.ValueMember = nameof(FelhasznaloDto.Id);
        if (repair.FelelosFelhasznaloId.HasValue) _employee.SelectedValue = repair.FelelosFelhasznaloId.Value;
        _employee.Location = new Point(24, 50); Controls.Add(_employee);
        Controls.Add(DialogUi.Label("Tervezett befejezés (opcionális)", 116, 24, true));
        if (repair.TervezettBefejezes.HasValue) { _deadline.Checked = true; _deadline.Value = repair.TervezettBefejezes.Value.ToLocalTime(); }
        _deadline.Location = new Point(24, 142); Controls.Add(_deadline);
        DialogUi.AddFooter(this, Save);
    }

    private void Save()
    {
        if (_employee.SelectedValue is null || !int.TryParse(Convert.ToString(_employee.SelectedValue), out var id))
        {
            MessageBox.Show(this, "Válassz felelős munkatársat.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Request = new FelelosRequest { FelhasznaloId = id, TervezettBefejezes = _deadline.Checked ? new DateTimeOffset(_deadline.Value.Date.AddHours(17)) : null };
        DialogResult = DialogResult.OK; Close();
    }
}

internal sealed class CloseRepairDialog : Form
{
    private readonly ComboBox _result = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10), Width = 360 };
    private readonly TextBox _reason = DialogUi.TextBox(126, 24, 500, 88, true);
    public JavitasLezarasRequest Request { get; private set; } = new();

    public CloseRepairDialog()
    {
        DialogUi.Configure(this, "Javítás lezárása", new Size(570, 340));
        Controls.Add(DialogUi.Label("Javítás eredménye", 24, 24, true));
        _result.Items.AddRange(["Javitva", "NemJavithato", "JavitasNelkulLezarva"]); _result.SelectedIndex = 0;
        _result.Location = new Point(24, 50); Controls.Add(_result);
        Controls.Add(DialogUi.Label("Lezárás indoklása *", 100, 24, true)); Controls.Add(_reason);
        DialogUi.AddFooter(this, Save);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_reason.Text))
        {
            MessageBox.Show(this, "A lezárás indoklása kötelező.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Request = new JavitasLezarasRequest { Eredmeny = Convert.ToString(_result.SelectedItem) ?? "Javitva", LezarasIndoklasa = _reason.Text.Trim() };
        DialogResult = DialogResult.OK; Close();
    }
}

internal sealed record StateChoice(string Value, string Label);

internal sealed class ChangeRepairStateDialog : Form
{
    private readonly ComboBox _state = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10), Width = 360 };
    public string SelectedState => Convert.ToString(_state.SelectedValue) ?? "";

    public ChangeRepairStateDialog(IEnumerable<StateChoice> states)
    {
        DialogUi.Configure(this, "Javítás állapotának módosítása", new Size(510, 230));
        Controls.Add(DialogUi.Label("Új állapot", 28, 24, true));
        _state.DataSource = states.ToList(); _state.DisplayMember = "Label"; _state.ValueMember = "Value";
        _state.Location = new Point(24, 54); Controls.Add(_state);
        DialogUi.AddFooter(this, () => { if (_state.SelectedValue is null) return; DialogResult = DialogResult.OK; Close(); });
    }
}

internal sealed class CustomerAccountDialog : Form
{
    private readonly ComboBox _customer = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.Font(10), Width = 450 };
    private readonly TextBox _username = DialogUi.TextBox(126, 24, 450);
    private readonly TextBox _password = DialogUi.TextBox(202, 24, 450);
    public UjFelhasznaloRequest Request { get; private set; } = new();

    public CustomerAccountDialog(List<UgyfelDto> customers, List<FelhasznaloDto> users)
    {
        var existingCustomerIds = users.Where(x => x.Szerepkor == "Ugyfel" && x.UgyfelId.HasValue).Select(x => x.UgyfelId!.Value).ToHashSet();
        var eligible = customers.Where(x => x.Aktiv && !existingCustomerIds.Contains(x.Id)).ToList();
        DialogUi.Configure(this, "Ügyfél bejelentkezési fiókja", new Size(560, 390));
        Controls.Add(DialogUi.Label("Ügyfél kiválasztása", 22, 24, true));
        _customer.DataSource = eligible; _customer.DisplayMember = nameof(UgyfelDto.Nev); _customer.ValueMember = nameof(UgyfelDto.Id);
        _customer.Location = new Point(24, 48); Controls.Add(_customer);
        Controls.Add(DialogUi.Label("Felhasználónév", 98, 24, true)); Controls.Add(_username);
        Controls.Add(DialogUi.Label("Kezdő jelszó (legalább 10 karakter)", 174, 24, true));
        _password.UseSystemPasswordChar = true; Controls.Add(_password);
        var hint = AppTheme.Label("A jelszót a szerviz állítja be, nem kell az ügyféltől elkérni. A belépési adatokat külön add át neki.", 8.5f, false, AppTheme.Muted);
        hint.Location = new Point(24, 254); hint.MaximumSize = new Size(490, 44); Controls.Add(hint);
        DialogUi.AddFooter(this, Save);
        if (eligible.Count == 0)
        {
            _customer.Enabled = _username.Enabled = _password.Enabled = false;
        }
    }

    private void Save()
    {
        if (_customer.SelectedItem is not UgyfelDto customer)
        {
            MessageBox.Show(this, "Nincs olyan aktív ügyfél, akihez még nem tartozik fiók.", "Nincs választható ügyfél", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(_username.Text) || _username.Text.Trim().Length > 80)
        {
            MessageBox.Show(this, "Adj meg egy legfeljebb 80 karakteres felhasználónevet.", "Hibás felhasználónév", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_password.Text.Length < 10 || _password.Text.Length > 128)
        {
            MessageBox.Show(this, "A kezdő jelszó legalább 10 karakter legyen.", "Hibás jelszó", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Request = new UjFelhasznaloRequest
        {
            Felhasznalonev = _username.Text.Trim(), Nev = customer.Nev, Email = customer.Email,
            Telefon = customer.Telefon, JogosultsagId = 4, UgyfelId = customer.Id, UjJelszo = _password.Text, Aktiv = true
        };
        DialogResult = DialogResult.OK; Close();
    }
}

internal sealed class RepairDetailsDialog : Form
{
    private readonly ApiClient _api;
    private readonly LoginResponseDto _user;
    private JavitasDto _repair;
    private BevizsgalasDto? _inspection;
    private List<MunkanaploDto> _workLogs = [];
    private List<AlkatreszFelhasznalasDto> _parts = [];
    private KoltsegOsszesitesDto? _costs;
    private readonly Label _status = AppTheme.Label("", 8.5f, true);
    private readonly Label _assigned = AppTheme.Label("", 9, false, AppTheme.Muted);
    private readonly Label _inspectionSummary = AppTheme.Label("", 9, false, AppTheme.Ink);
    private readonly Label _costSummary = AppTheme.Label("", 10, true, AppTheme.Ink);
    private readonly DataGridView _workGrid = NewGrid();
    private readonly DataGridView _partGrid = NewGrid();
    private readonly DataGridView _costGrid = NewGrid();
    private readonly Button _inspectionButton = AppTheme.Button("Bevizsgálás rögzítése", true);
    private readonly Button _workButton = AppTheme.Button("＋ Munkanapló-bejegyzés", true);
    private readonly Button _partButton = AppTheme.Button("＋ Alkatrész", true);
    private readonly Button _costButton = AppTheme.Button("＋ Egyéb költség");
    private readonly Button _finalizeCostsButton = AppTheme.Button("Költségek véglegesítése");
    private readonly Button _stateButton = AppTheme.Button("Állapot módosítása");
    private readonly Button _closeRepairButton = AppTheme.Button("Javítás lezárása", true);
    private readonly Button _assignButton = AppTheme.Button("Felelős kijelölése");
    private readonly Label _permissionHint = AppTheme.Label("", 8.5f, false, AppTheme.Orange);
    private readonly bool _isManager;
    private readonly bool _isMechanic;

    public RepairDetailsDialog(ApiClient api, JavitasDto repair, LoginResponseDto user)
    {
        _api = api; _repair = repair; _user = user;
        _isManager = user.Szerepkor.Equals("Manager", StringComparison.OrdinalIgnoreCase);
        _isMechanic = user.Szerepkor.Equals("Szerelo", StringComparison.OrdinalIgnoreCase);

        Text = $"Javítás #{repair.Id} – részletek";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = false; ShowInTaskbar = false;
        MinimumSize = new Size(900, 650); ClientSize = new Size(1080, 760);
        BackColor = AppTheme.Background; Font = AppTheme.Font();

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.Font(9) };
        tabs.TabPages.Add(BuildOverviewTab());
        tabs.TabPages.Add(BuildInspectionTab());
        tabs.TabPages.Add(BuildWorkTab());
        tabs.TabPages.Add(BuildCostsTab());

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 66, BackColor = Color.White,
            FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(18, 13, 18, 8), WrapContents = false
        };
        var close = AppTheme.Button("Bezárás"); AppTheme.StyleButton(close); close.Click += (_, _) => Close();
        footer.Controls.Add(close);
        StyleAction(_closeRepairButton, true); _closeRepairButton.Click += async (_, _) => await CloseRepairAsync(); footer.Controls.Add(_closeRepairButton);
        StyleAction(_stateButton); _stateButton.Click += async (_, _) => await ChangeStateAsync(); footer.Controls.Add(_stateButton);
        StyleAction(_assignButton); _assignButton.Click += async (_, _) => await AssignAsync(); footer.Controls.Add(_assignButton);
        var header = BuildHeader();
        Controls.Add(tabs); Controls.Add(footer); Controls.Add(header);
        Shown += async (_, _) => await ReloadAsync();
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = AppTheme.Sidebar, Padding = new Padding(26, 16, 26, 12) };
        var eyebrow = new Label { Text = "SZERVIZ MUNKALAP", AutoSize = true, ForeColor = AppTheme.SidebarMuted, Font = AppTheme.Font(8, true), Location = new Point(26, 13) };
        var title = new Label { Text = $"Javítás #{_repair.Id}", AutoSize = true, ForeColor = Color.White, Font = AppTheme.Font(20, true), Location = new Point(26, 35) };
        _status.Location = new Point(235, 40); _status.ForeColor = Color.White;
        _status.Text = AppTheme.StatusLabel(_repair.Allapot);
        header.Controls.Add(eyebrow); header.Controls.Add(title); header.Controls.Add(_status);
        return header;
    }

    private TabPage BuildOverviewTab()
    {
        var page = new TabPage("Áttekintés") { BackColor = AppTheme.Background, Padding = new Padding(18) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 4, AutoSize = true, BackColor = AppTheme.Background };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 125)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        AddInfoCard(layout, "Ügyfél azonosító", _repair.UgyfelId.ToString(), 0, 0);
        AddInfoCard(layout, "Eszköz azonosító", _repair.EszkozId.ToString(), 1, 0);
        AddInfoCard(layout, "Átvétel dátuma", _repair.AtvetelDatuma.ToString("yyyy. MM. dd. HH:mm"), 0, 1);
        AddInfoCard(layout, "Garanciális", _repair.Garancialis ? "Igen" : "Nem", 1, 1);
        var issue = AppTheme.Card(); issue.Dock = DockStyle.Fill; issue.Margin = new Padding(4);
        var issueTitle = DialogUi.Label("Bejelentett hiba", 14, 16, true); issue.Controls.Add(issueTitle);
        var issueText = AppTheme.Label(_repair.BejelentettHiba, 9.5f, false, AppTheme.Ink); issueText.Location = new Point(16, 42); issueText.MaximumSize = new Size(820, 64); issue.Controls.Add(issueText);
        layout.Controls.Add(issue, 0, 2); layout.SetColumnSpan(issue, 2);
        _assigned.Location = new Point(5, 8); layout.Controls.Add(_assigned, 0, 3); layout.SetColumnSpan(_assigned, 2);
        page.Controls.Add(layout);
        _permissionHint.Location = new Point(22, 385); _permissionHint.MaximumSize = new Size(850, 42); page.Controls.Add(_permissionHint);
        return page;
    }

    private TabPage BuildInspectionTab()
    {
        var page = new TabPage("Bevizsgálás") { BackColor = AppTheme.Background, Padding = new Padding(20) };
        var card = AppTheme.Card(); card.Dock = DockStyle.Top; card.Height = 210;
        var title = DialogUi.Label("Bevizsgálás eredménye", 18, 18, true); card.Controls.Add(title);
        _inspectionSummary.Location = new Point(18, 52); _inspectionSummary.MaximumSize = new Size(850, 92); card.Controls.Add(_inspectionSummary);
        _inspectionButton.Location = new Point(18, 150); StyleAction(_inspectionButton, true);
        _inspectionButton.Click += async (_, _) => await EditInspectionAsync(); card.Controls.Add(_inspectionButton);
        page.Controls.Add(card);
        return page;
    }

    private TabPage BuildWorkTab()
    {
        var page = new TabPage("Munkanapló") { BackColor = AppTheme.Background, Padding = new Padding(18) };
        var button = _workButton; StyleAction(button, true); button.Dock = DockStyle.Top; button.Height = 38;
        button.Click += async (_, _) => await AddWorkLogAsync();
        _workGrid.Dock = DockStyle.Fill; _workGrid.Margin = new Padding(0, 10, 0, 0);
        _workGrid.Columns.Add(Col("Leiras", "Elvégzett munka", 46));
        _workGrid.Columns.Add(Col("Idotartam", "Időtartam", 17));
        _workGrid.Columns.Add(Col("Oradij", "Óradíj", 17));
        _workGrid.Columns.Add(Col("Munkadij", "Munkadíj", 20));
        page.Controls.Add(_workGrid);
        page.Controls.Add(button);
        return page;
    }

    private TabPage BuildCostsTab()
    {
        var page = new TabPage("Alkatrészek és költségek") { BackColor = AppTheme.Background, Padding = new Padding(18) };
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, WrapContents = false, BackColor = AppTheme.Background };
        StyleAction(_partButton, true); _partButton.Click += async (_, _) => await AddPartAsync(); top.Controls.Add(_partButton);
        StyleAction(_costButton); _costButton.Click += async (_, _) => await AddCostAsync(); top.Controls.Add(_costButton);
        StyleAction(_finalizeCostsButton); _finalizeCostsButton.Click += async (_, _) => await FinalizeCostsAsync(); top.Controls.Add(_finalizeCostsButton);
        _costSummary.Dock = DockStyle.Top;
        _costSummary.Height = 34;
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 180, BackColor = AppTheme.Background, Panel1MinSize = 90, Panel2MinSize = 90 };
        var partLabel = DialogUi.Label("Felhasznált alkatrészek", 2, 0, true);
        partLabel.Dock = DockStyle.Top; partLabel.Height = 24;
        _partGrid.Dock = DockStyle.Fill;
        _partGrid.Columns.Add(Col("Cikkszam", "Cikkszám", 22)); _partGrid.Columns.Add(Col("Nev", "Alkatrész", 34));
        _partGrid.Columns.Add(Col("Mennyiseg", "Mennyiség", 20)); _partGrid.Columns.Add(Col("Osszeg", "Összeg", 24));
        split.Panel1.Controls.Add(_partGrid); split.Panel1.Controls.Add(partLabel);
        var costLabel = DialogUi.Label("Egyéb költségek", 2, 0, true);
        costLabel.Dock = DockStyle.Top; costLabel.Height = 24;
        _costGrid.Dock = DockStyle.Fill;
        _costGrid.Columns.Add(Col("Megnevezes", "Megnevezés", 48)); _costGrid.Columns.Add(Col("Tipus", "Típus", 24)); _costGrid.Columns.Add(Col("Osszeg", "Összeg", 28));
        split.Panel2.Controls.Add(_costGrid); split.Panel2.Controls.Add(costLabel);
        page.Controls.Add(split);
        page.Controls.Add(_costSummary);
        page.Controls.Add(top);
        return page;
    }

    private async Task ReloadAsync()
    {
        try
        {
            _repair = await _api.GetJavitasAsync(_repair.Id);
            _inspection = null;
            try { _inspection = await _api.GetBevizsgalasAsync(_repair.Id); }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) { }
            _workLogs = await _api.GetMunkanaplokAsync(_repair.Id);
            _parts = await _api.GetFelhasznaltAlkatreszekAsync(_repair.Id);
            _costs = await _api.GetKoltsegOsszesitesAsync(_repair.Id);

            _status.Text = AppTheme.StatusLabel(_repair.Allapot);
            _assigned.Text = $"Felelős: {(_repair.FelelosFelhasznaloId.HasValue ? $"munkatárs #{_repair.FelelosFelhasznaloId}" : "nincs kijelölve")}     Tervezett befejezés: {_repair.TervezettBefejezes?.ToString("yyyy. MM. dd.") ?? "–"}";
            _inspectionSummary.Text = _inspection is null
                ? "Még nincs bevizsgálási eredmény rögzítve."
                : $"{_inspection.Eredmeny}\nBecsült költség: {_inspection.BecsultKoltseg?.ToString("N0") ?? "nincs megadva"} Ft\nRögzítve: {_inspection.Datum:yyyy. MM. dd. HH:mm}";
            _workGrid.Rows.Clear();
            foreach (var item in _workLogs)
                _workGrid.Rows.Add(item.Leiras, $"{item.LedolgozottOra:N2} óra", $"{item.Oradij:N0} Ft", $"{item.Munkadij:N0} Ft");
            _partGrid.Rows.Clear();
            foreach (var item in _parts)
                _partGrid.Rows.Add(item.Cikkszam, item.Nev, $"{item.Mennyiseg:N3} {item.Egyseg}", $"{item.Osszeg:N0} Ft");
            _costGrid.Rows.Clear();
            foreach (var item in _costs.Tetelek)
                _costGrid.Rows.Add(item.Megnevezes, item.Tipus, $"{item.Osszeg:N0} Ft");
            _costSummary.Text = $"Munkadíj: {_costs.Munkadij:N0} Ft    Alkatrész: {_costs.AlkatreszDij:N0} Ft    Egyéb: {_costs.EgyebKoltseg:N0} Ft    Végösszeg: {_costs.Vegosszeg:N0} Ft";
            UpdateActions();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Nem sikerült betölteni a javítás részleteit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateActions()
    {
        var canEdit = CanEdit;
        var hasAssignee = _repair.FelelosFelhasznaloId.HasValue;
        var canWork = canEdit && hasAssignee;
        var open = _repair.Allapot is not ("Lezart" or "Visszaadva");
        _inspectionButton.Enabled = canWork && open;
        _workButton.Enabled = canWork && _repair.Allapot == "JavitasAlatt";
        _partButton.Enabled = canWork && _repair.Allapot == "JavitasAlatt" && !_costs!.KoltsegekVeglegesitve;
        _costButton.Enabled = canWork && open && !_costs!.KoltsegekVeglegesitve;
        _finalizeCostsButton.Enabled = canWork && open && !_costs!.KoltsegekVeglegesitve && _costs.FolyamatbanLevoMunkak == 0;
        _stateButton.Enabled = canWork && open;
        _closeRepairButton.Enabled = canWork && open;
        _assignButton.Visible = _isManager && open;
        _permissionHint.Text = canEdit ? "" : _isMechanic
            ? "Ehhez a javításhoz még nincs hozzád rendelve felelős munkatársként. Kérd a manager hozzárendelését."
            : "Ehhez a javításhoz csak a kijelölt szerelő vagy a manager végezhet módosításokat.";
        _permissionHint.Visible = !canEdit;
    }

    private bool CanEdit => _isManager || (_isMechanic && _repair.FelelosFelhasznaloId == _user.FelhasznaloId);

    private async Task EditInspectionAsync()
    {
        if (!CanEdit) return;
        using var dialog = new BevizsgalasDialog(_inspection);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await RunActionAsync(async () => await _api.SaveBevizsgalasAsync(_repair.Id, dialog.Request, _inspection is not null));
    }

    private async Task AddWorkLogAsync()
    {
        if (!CanEdit || _repair.Allapot != "JavitasAlatt") return;
        using var dialog = new WorkLogDialog(_repair.AtvetelDatuma);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await RunActionAsync(async () => await _api.CreateMunkanaploAsync(_repair.Id, dialog.Request));
    }

    private async Task AddPartAsync()
    {
        if (!CanEdit || _repair.Allapot != "JavitasAlatt") return;
        await RunActionAsync(async () =>
        {
            var parts = (await _api.GetAlkatreszekAsync()).Where(x => x.Aktiv).ToList();
            if (parts.Count == 0) { MessageBox.Show(this, "Nincs aktív alkatrész a katalógusban. A managernek először fel kell vennie az alkatrészt.", "Nincs alkatrész", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using var dialog = new PartUseDialog(parts);
            if (dialog.ShowDialog(this) == DialogResult.OK) await _api.CreateAlkatreszFelhasznalasAsync(_repair.Id, dialog.Request);
        });
    }

    private async Task AddCostAsync()
    {
        if (!CanEdit) return;
        using var dialog = new CostDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await RunActionAsync(async () => await _api.CreateKoltsegAsync(_repair.Id, dialog.Request));
    }

    private async Task FinalizeCostsAsync()
    {
        if (!CanEdit) return;
        var confirm = MessageBox.Show(this, "Véglegesíted a javítás költségeit? Ezután új költségtétel nem rögzíthető.", "Költségek véglegesítése", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;
        await RunActionAsync(async () => await _api.VeglegesitKoltsegeketAsync(_repair.Id));
    }

    private async Task ChangeStateAsync()
    {
        if (!CanEdit) return;
        var options = _repair.Allapot switch
        {
            "BevizsgalasAlatt" => new[] { new StateChoice("JavitasAlatt", "Javítás alatt"), new StateChoice("AlkatreszreVar", "Alkatrészre vár") },
            "JavitasAlatt" => new[] { new StateChoice("AlkatreszreVar", "Alkatrészre vár") },
            "AlkatreszreVar" => new[] { new StateChoice("JavitasAlatt", "Javítás alatt") },
            _ => Array.Empty<StateChoice>()
        };
        if (options.Length == 0)
        {
            MessageBox.Show(this, "Ebből az állapotból nincs közvetlen állapotváltás. Átvett javításnál először rögzíts bevizsgálást.", "Nincs elérhető állapotváltás", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new ChangeRepairStateDialog(options);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await RunActionAsync(async () => await _api.SetAllapotAsync(_repair.Id, new AllapotRequest { Allapot = dialog.SelectedState }));
    }

    private async Task CloseRepairAsync()
    {
        if (!CanEdit) return;
        using var dialog = new CloseRepairDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await RunActionAsync(async () => await _api.LezarJavitastAsync(_repair.Id, dialog.Request));
    }

    private async Task AssignAsync()
    {
        if (!_isManager) return;
        await RunActionAsync(async () =>
        {
            var users = await _api.GetFelhasznalokAsync();
            using var dialog = new AssignMechanicDialog(users, _repair);
            if (dialog.ShowDialog(this) == DialogResult.OK) await _api.SetFelelosAsync(_repair.Id, dialog.Request);
        });
    }

    private async Task RunActionAsync(Func<Task> action)
    {
        try { await action(); await ReloadAsync(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "A művelet nem sikerült", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private static void AddInfoCard(TableLayoutPanel parent, string caption, string value, int column, int row)
    {
        var card = AppTheme.Card(); card.Dock = DockStyle.Fill; card.Margin = new Padding(4);
        card.Controls.Add(DialogUi.Label(caption, 12, 14, true));
        var val = AppTheme.Label(value, 10, true, AppTheme.Ink); val.Location = new Point(14, 39); card.Controls.Add(val);
        parent.Controls.Add(card, column, row);
    }

    private static DataGridView NewGrid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true };
        AppTheme.StyleGrid(grid);
        return grid;
    }

    private static DataGridViewTextBoxColumn Col(string name, string title, float weight)
        => new() { Name = name, HeaderText = title, FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable };

    private static void StyleAction(Button button, bool primary = false)
    {
        AppTheme.StyleButton(button, primary);
        button.AutoSize = true;
        button.MinimumSize = new Size(0, 36);
    }
}
