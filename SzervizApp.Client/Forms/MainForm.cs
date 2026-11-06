using System.Drawing;
using System.Windows.Forms;
using SzervizApp.Client.Api;
using SzervizApp.Client.Models;
using SzervizApp.Client.UI;

namespace SzervizApp.Client.Forms;

public sealed class MainForm : Form
{
    private readonly ApiClient _api;
    private readonly LoginResponseDto _user;
    private readonly Panel _workspace = new() { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(28, 22, 28, 24) };
    private readonly Label _connectionLabel = AppTheme.Label("●  Kapcsolódva", 8.5f, true, AppTheme.Green);
    private readonly List<Button> _navButtons = [];

    private const string DashboardPage = "dashboard";
    private const string RepairsPage = "repairs";
    private const string CustomersPage = "customers";
    private const string DevicesPage = "devices";

    private bool IsManager => _user.Szerepkor.Equals("Manager", StringComparison.OrdinalIgnoreCase);
    private bool CanReception => IsManager || _user.Szerepkor.Equals("Recepcios", StringComparison.OrdinalIgnoreCase);
    private bool IsStaff => CanReception || _user.Szerepkor.Equals("Szerelo", StringComparison.OrdinalIgnoreCase);

    public MainForm(ApiClient api, LoginResponseDto user)
    {
        _api = api;
        _user = user;
        Text = "SzervizApp";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1120, 720);
        BackColor = AppTheme.Background;
        Font = AppTheme.Font();

        BuildShell();
        Shown += async (_, _) => await NavigateAsync(DashboardPage);
        FormClosed += (_, _) => _api.Dispose();
    }

    private void BuildShell()
    {
        // A Fill panelt adjuk hozzá először, a Left oldalsávot utána.
        // WinForms esetén a dokkolási sorrend a vezérlők Z-sorrendjétől függ;
        // fordított sorrendben a Fill panel a teljes ablakot elfoglalhatja,
        // és az oldalsáv csak ráfed a tartalomra.
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 238, BackColor = AppTheme.Sidebar, Padding = new Padding(18, 22, 18, 18) };

        var sidebarLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = AppTheme.Sidebar, Margin = new Padding(0) };
        sidebarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        sidebar.Controls.Add(sidebarLayout);

        var brandRow = new Panel { Dock = DockStyle.Fill };
        sidebarLayout.Controls.Add(brandRow, 0, 0);
        var logo = new Label { Text = "SZ", Size = new Size(42, 42), Location = new Point(2, 3), BackColor = Color.FromArgb(53, 82, 133), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = AppTheme.Font(12, true) };
        brandRow.Controls.Add(logo);
        var brand = new Label { Text = "SzervizApp", AutoSize = true, Location = new Point(54, 5), ForeColor = Color.White, Font = AppTheme.Font(14, true) };
        brandRow.Controls.Add(brand);
        var brandSub = new Label { Text = "SZERVIZKEZELŐ", AutoSize = true, Location = new Point(55, 29), ForeColor = AppTheme.SidebarMuted, Font = AppTheme.Font(7.5f, true) };
        brandRow.Controls.Add(brandSub);

        var groupLabel = new Label { Text = "MENÜ", AutoSize = true, ForeColor = Color.FromArgb(123, 143, 171), Font = AppTheme.Font(8, true), Dock = DockStyle.Fill, Padding = new Padding(7, 10, 0, 0) };
        sidebarLayout.Controls.Add(groupLabel, 0, 1);
        var navPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = AppTheme.Sidebar, Padding = new Padding(0), Margin = new Padding(0) };
        sidebarLayout.Controls.Add(navPanel, 0, 2);
        AddNav(navPanel, "▦", "Áttekintés", DashboardPage);
        AddNav(navPanel, "▤", "Javítások", RepairsPage);
        if (IsStaff)
        {
            AddNav(navPanel, "♙", "Ügyfelek", CustomersPage);
            AddNav(navPanel, "▣", "Eszközök", DevicesPage);
        }

        var bottom = new Panel { Dock = DockStyle.Fill };
        sidebarLayout.Controls.Add(bottom, 0, 3);
        var divider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(53, 67, 88) };
        bottom.Controls.Add(divider);
        var userName = new Label { Text = _user.Nev, AutoSize = false, Height = 24, Dock = DockStyle.Top, ForeColor = Color.White, Font = AppTheme.Font(9.5f, true), Padding = new Padding(7, 12, 0, 0) };
        bottom.Controls.Add(userName);
        var userRole = new Label { Text = RoleLabel(_user.Szerepkor), AutoSize = false, Height = 27, Dock = DockStyle.Top, ForeColor = AppTheme.SidebarMuted, Font = AppTheme.Font(8.5f), Padding = new Padding(7, 5, 0, 0) };
        bottom.Controls.Add(userRole);
        var logout = new Button { Text = "↪  Kijelentkezés", Dock = DockStyle.Bottom, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = AppTheme.SidebarMuted, Font = AppTheme.Font(9), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
        logout.FlatAppearance.BorderSize = 0;
        logout.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 58, 81);
        logout.Click += async (_, _) => await LogoutAsync();
        bottom.Controls.Add(logout);

        var right = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
        Controls.Add(right);
        Controls.Add(sidebar);
        right.Controls.Add(_workspace);

        var topbar = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Color.White, Padding = new Padding(28, 0, 28, 0) };
        right.Controls.Add(topbar);
        var appLabel = AppTheme.Label("Szerviz és javításkezelő rendszer", 9.5f, false, AppTheme.Muted);
        appLabel.Location = new Point(28, 25);
        topbar.Controls.Add(appLabel);
        _connectionLabel.Location = new Point(0, 25);
        _connectionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _connectionLabel.AutoSize = true;
        topbar.Controls.Add(_connectionLabel);
        topbar.Resize += (_, _) => _connectionLabel.Left = topbar.ClientSize.Width - _connectionLabel.Width - 28;
    }

    private void AddNav(FlowLayoutPanel navPanel, string icon, string text, string key)
    {
        var button = new Button
        {
            Text = $"  {icon}     {text}",
            Tag = key,
            Width = 198,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.Sidebar,
            ForeColor = AppTheme.SidebarMuted,
            Font = AppTheme.Font(9.5f, true),
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 4)
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 53, 77);
        button.Click += async (_, _) => await NavigateAsync(key);
        _navButtons.Add(button);
        navPanel.Controls.Add(button);
    }

    private async Task NavigateAsync(string page)
    {
        foreach (var button in _navButtons)
        {
            var selected = Equals(button.Tag, page);
            button.BackColor = selected ? Color.FromArgb(47, 68, 101) : AppTheme.Sidebar;
            button.ForeColor = selected ? Color.White : AppTheme.SidebarMuted;
            button.FlatAppearance.BorderColor = selected ? AppTheme.Accent : AppTheme.Sidebar;
        }

        try
        {
            switch (page)
            {
                case DashboardPage: await ShowDashboardAsync(); break;
                case RepairsPage: await ShowRepairsAsync(); break;
                case CustomersPage: await ShowCustomersAsync(); break;
                case DevicesPage: await ShowDevicesAsync(); break;
            }
            _connectionLabel.Text = "●  Kapcsolódva";
            _connectionLabel.ForeColor = AppTheme.Green;
        }
        catch (Exception ex)
        {
            _connectionLabel.Text = "●  Kapcsolati hiba";
            _connectionLabel.ForeColor = AppTheme.Red;
            ShowErrorPage(ex.Message, page);
        }
    }

    private void StartPage(string title, string subtitle, out Panel body, out FlowLayoutPanel actions)
    {
        _workspace.Controls.Clear();
        var root = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
        _workspace.Controls.Add(root);
        body = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(0, 16, 0, 0), AutoScroll = true };
        root.Controls.Add(body);
        var header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = AppTheme.Background };
        root.Controls.Add(header);
        var titleLabel = AppTheme.Label(title, 22, true);
        titleLabel.Location = new Point(0, 0);
        header.Controls.Add(titleLabel);
        var subtitleLabel = AppTheme.Label(subtitle, 9.5f, false, AppTheme.Muted);
        subtitleLabel.Location = new Point(2, 40);
        header.Controls.Add(subtitleLabel);
        // Az out paramétert nem használjuk közvetlenül az eseménykezelő lambdában.
        // A lokális változó biztonságosan rögzíthető a Resize eseményhez.
        var actionPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(Math.Max(0, header.Width - 330), 14),
            BackColor = AppTheme.Background
        };
        actions = actionPanel;
        header.Controls.Add(actionPanel);
        header.Resize += (_, _) => actionPanel.Left = header.ClientSize.Width - actionPanel.Width;
    }

    private async Task ShowDashboardAsync()
    {
        StartPage("Áttekintés", "Rövid összefoglaló a szerviz aktuális munkáiról.", out var body, out var actions);
        var refresh = AppTheme.Button("↻  Frissítés");
        AppTheme.StyleButton(refresh);
        refresh.Click += async (_, _) => await NavigateAsync(DashboardPage);
        actions.Controls.Add(refresh);

        var repairs = await _api.GetJavitasokAsync();
        List<FeladatDto> tasks = IsStaff ? await _api.GetFeladatokAsync(sajat: _user.Szerepkor.Equals("Szerelo", StringComparison.OrdinalIgnoreCase)) : [];

        var total = repairs.Count;
        var open = repairs.Count(x => x.Allapot is not ("Lezart" or "Visszaadva"));
        var inspection = repairs.Count(x => x.Allapot == "BevizsgalasAlatt");
        var repair = repairs.Count(x => x.Allapot == "JavitasAlatt");
        var returned = repairs.Count(x => x.Allapot == "Visszaadva");

        var cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 136, WrapContents = false, AutoScroll = true, BackColor = AppTheme.Background, Padding = new Padding(0, 0, 0, 8) };
        body.Controls.Add(cards);
        cards.Controls.Add(CreateStatCard("Összes javítás", total.ToString(), "Nyilvántartott munkalap", AppTheme.Accent));
        cards.Controls.Add(CreateStatCard("Folyamatban", open.ToString(), $"Ebből javítás alatt: {repair}", AppTheme.Orange));
        cards.Controls.Add(CreateStatCard("Bevizsgálás alatt", inspection.ToString(), "Szakmai ellenőrzés", Color.FromArgb(146, 103, 174)));
        cards.Controls.Add(CreateStatCard("Visszaadva", returned.ToString(), "Lezárt ügyek", AppTheme.Green));

        var section = AppTheme.Label("Legutóbbi javítások", 13, true);
        section.Location = new Point(0, 165);
        body.Controls.Add(section);
        var hint = AppTheme.Label("A legutóbb felvett szervizfeladatok.", 9, false, AppTheme.Muted);
        hint.Location = new Point(1, 190);
        body.Controls.Add(hint);

        var grid = CreateGrid();
        grid.Location = new Point(0, 222);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grid.Height = Math.Max(250, body.ClientSize.Height - 230);
        grid.Width = Math.Max(600, body.ClientSize.Width - 4);
        grid.Columns.Add(TextColumn("Id", "Azonosító", 12));
        grid.Columns.Add(TextColumn("Ugyfel", "Ügyfél", 23));
        grid.Columns.Add(TextColumn("Eszkoz", "Eszköz", 22));
        grid.Columns.Add(TextColumn("Allapot", "Állapot", 20));
        grid.Columns.Add(TextColumn("Datum", "Átvétel", 20));
        body.Controls.Add(grid);
        body.Resize += (_, _) => grid.Height = Math.Max(250, body.ClientSize.Height - 230);

        if (IsStaff)
        {
            foreach (var item in tasks.OrderByDescending(x => x.AtvetelDatuma).Take(8))
                grid.Rows.Add($"#{item.Id}", item.UgyfelNev, item.EszkozTipus, AppTheme.StatusLabel(item.Allapot), item.AtvetelDatuma.ToString("yyyy. MM. dd."));
        }
        else
        {
            foreach (var item in repairs.OrderByDescending(x => x.AtvetelDatuma).Take(8))
                grid.Rows.Add($"#{item.Id}", $"Ügyfél #{item.UgyfelId}", $"Eszköz #{item.EszkozId}", AppTheme.StatusLabel(item.Allapot), item.AtvetelDatuma.ToString("yyyy. MM. dd."));
        }
        ApplyStatusColors(grid, "Allapot");
    }

    private async Task ShowRepairsAsync()
    {
        var isMechanic = _user.Szerepkor.Equals("Szerelo", StringComparison.OrdinalIgnoreCase);
        if (isMechanic)
        {
            await ShowMechanicRepairsAsync();
            return;
        }

        StartPage("Javítások", "Keresd meg és kövesd nyomon a szervizbe érkezett eszközöket.", out var body, out var actions);
        if (CanReception)
        {
            var add = AppTheme.Button("＋  Új javítás", true);
            AppTheme.StyleButton(add, true);
            add.Click += async (_, _) =>
            {
                using var dialog = new RepairDialog(_api);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    await NavigateAsync(RepairsPage);
            };
            actions.Controls.Add(add);
        }
        var refresh = AppTheme.Button("↻");
        AppTheme.StyleButton(refresh);
        refresh.Click += async (_, _) => await NavigateAsync(RepairsPage);
        actions.Controls.Add(refresh);

        var search = AppTheme.TextBox("Keresés ügyfélre, hibára vagy eszközre...");
        search.Width = 350;
        search.Location = new Point(0, 0);
        body.Controls.Add(search);
        var searchButton = AppTheme.Button("Keresés");
        AppTheme.StyleButton(searchButton);
        searchButton.Location = new Point(362, 0);
        body.Controls.Add(searchButton);

        var grid = CreateGrid();
        grid.Location = new Point(0, 55);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 65));
        grid.Columns.Add(TextColumn("Id", "Azonosító", 11));
        grid.Columns.Add(TextColumn("Ugyfel", "Ügyfél", 19));
        grid.Columns.Add(TextColumn("Eszkoz", "Eszköz", 18));
        grid.Columns.Add(TextColumn("Hiba", "Bejelentett hiba", 34));
        grid.Columns.Add(TextColumn("Allapot", "Állapot", 17));
        grid.Columns.Add(TextColumn("Datum", "Átvétel", 18));
        body.Controls.Add(grid);
        body.Resize += (_, _) => grid.Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 65));
        ApplyStatusColors(grid, "Allapot");

        async Task LoadRowsAsync()
        {
            grid.Rows.Clear();
            var filter = search.Text.Trim();
            if (IsStaff)
            {
                var tasks = await _api.GetFeladatokAsync(sajat: false);
                foreach (var item in tasks.OrderByDescending(x => x.AtvetelDatuma)
                             .Where(x => Matches(filter, x.UgyfelNev, x.EszkozTipus, x.BejelentettHiba)))
                    grid.Rows.Add(item.Id, item.UgyfelNev, item.EszkozTipus, item.BejelentettHiba,
                        AppTheme.StatusLabel(item.Allapot), item.AtvetelDatuma.ToString("yyyy. MM. dd."));
            }
            else
            {
                var repairs = await _api.GetJavitasokAsync();
                foreach (var item in repairs.OrderByDescending(x => x.AtvetelDatuma)
                             .Where(x => Matches(filter, x.BejelentettHiba, x.Id.ToString())))
                    grid.Rows.Add(item.Id, $"Ügyfél #{item.UgyfelId}", $"Eszköz #{item.EszkozId}", item.BejelentettHiba,
                        AppTheme.StatusLabel(item.Allapot), item.AtvetelDatuma.ToString("yyyy. MM. dd."));
            }
        }

        searchButton.Click += async (_, _) => await SafeRunAsync(LoadRowsAsync);
        search.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await SafeRunAsync(LoadRowsAsync); } };
        grid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || !int.TryParse(Convert.ToString(grid.Rows[e.RowIndex].Cells["Id"].Value), out var id)) return;
            await ShowRepairDetailsAsync(id);
        };

        await LoadRowsAsync();
    }

    private async Task ShowMechanicRepairsAsync()
    {
        StartPage("Javítások", "Válaszd ki a saját feladataidat, vagy vállalj el egy szabad javítást.", out var body, out var actions);

        var refresh = AppTheme.Button("↻  Frissítés");
        AppTheme.StyleButton(refresh);
        actions.Controls.Add(refresh);

        var search = AppTheme.TextBox("Keresés ügyfélre, hibára vagy eszközre...");
        search.Width = 390;
        search.Location = new Point(0, 0);
        body.Controls.Add(search);

        var tabs = new TabControl
        {
            Location = new Point(0, 52),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 56)),
            Font = AppTheme.Font(9)
        };
        var ownPage = new TabPage("Saját feladataim") { BackColor = AppTheme.Background, Padding = new Padding(8) };
        var freePage = new TabPage("Szabad feladatok") { BackColor = AppTheme.Background, Padding = new Padding(8) };
        var ownGrid = CreateMechanicTaskGrid(includeClaimButton: false);
        var freeGrid = CreateMechanicTaskGrid(includeClaimButton: true);
        ApplyStatusColors(ownGrid, "Allapot");
        ApplyStatusColors(freeGrid, "Allapot");
        ownPage.Controls.Add(ownGrid);
        freePage.Controls.Add(freeGrid);
        tabs.TabPages.Add(ownPage);
        tabs.TabPages.Add(freePage);
        body.Controls.Add(tabs);
        body.Resize += (_, _) => tabs.Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 56));

        ownGrid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0 && int.TryParse(Convert.ToString(ownGrid.Rows[e.RowIndex].Cells["Id"].Value), out var id))
                await ShowRepairDetailsAsync(id);
        };
        freeGrid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0 && int.TryParse(Convert.ToString(freeGrid.Rows[e.RowIndex].Cells["Id"].Value), out var id))
                await ShowRepairDetailsAsync(id);
        };

        List<FeladatDto> cachedTasks = [];

        void PopulateRows()
        {
            var filter = search.Text.Trim();
            var ownTasks = cachedTasks
                .Where(x => x.FelelosFelhasznaloId == _user.FelhasznaloId)
                .OrderByDescending(x => x.AtvetelDatuma)
                .Where(x => Matches(filter, x.UgyfelNev, x.EszkozTipus, x.BejelentettHiba));
            var freeTasks = cachedTasks
                .Where(x => !x.FelelosFelhasznaloId.HasValue && x.Allapot is not ("Lezart" or "Visszaadva"))
                .OrderByDescending(x => x.AtvetelDatuma)
                .Where(x => Matches(filter, x.UgyfelNev, x.EszkozTipus, x.BejelentettHiba));

            ownGrid.Rows.Clear();
            foreach (var task in ownTasks)
                ownGrid.Rows.Add(task.Id, task.UgyfelNev, task.EszkozTipus, task.BejelentettHiba,
                    AppTheme.StatusLabel(task.Allapot), task.AtvetelDatuma.ToString("yyyy. MM. dd."));

            freeGrid.Rows.Clear();
            foreach (var task in freeTasks)
                freeGrid.Rows.Add(task.Id, task.UgyfelNev, task.EszkozTipus, task.BejelentettHiba,
                    AppTheme.StatusLabel(task.Allapot), task.AtvetelDatuma.ToString("yyyy. MM. dd."), "Feladat vállalása");

        }

        async Task LoadRowsAsync()
        {
            cachedTasks = await _api.GetFeladatokAsync(sajat: false);
            PopulateRows();
        }

        freeGrid.CellContentClick += async (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || freeGrid.Columns[e.ColumnIndex].Name != "Vallalas") return;
            if (!int.TryParse(Convert.ToString(freeGrid.Rows[e.RowIndex].Cells["Id"].Value), out var id)) return;
            var answer = MessageBox.Show(this, $"Szeretnéd vállalni a(z) #{id} azonosítójú javítást? A vállalás után te leszel a felelős szerelő.",
                "Feladat vállalása", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            await SafeRunAsync(async () =>
            {
                await _api.VallalJavitastAsync(id);
                MessageBox.Show(this, $"A(z) #{id} javítás mostantól hozzád van rendelve.", "Sikeres vállalás", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadRowsAsync();
            });
        };

        search.TextChanged += (_, _) => PopulateRows();
        refresh.Click += async (_, _) => await SafeRunAsync(LoadRowsAsync);
        await LoadRowsAsync();
    }

    private static DataGridView CreateMechanicTaskGrid(bool includeClaimButton)
    {
        var grid = CreateGrid();
        grid.Dock = DockStyle.Fill;
        grid.Columns.Add(TextColumn("Id", "Azonosító", 10));
        grid.Columns.Add(TextColumn("Ugyfel", "Ügyfél", 17));
        grid.Columns.Add(TextColumn("Eszkoz", "Eszköz", 15));
        grid.Columns.Add(TextColumn("Hiba", "Bejelentett hiba", 31));
        grid.Columns.Add(TextColumn("Allapot", "Állapot", 15));
        grid.Columns.Add(TextColumn("Datum", "Átvétel", 12));
        if (includeClaimButton)
        {
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Vallalas",
                HeaderText = "Művelet",
                Text = "Feladat vállalása",
                UseColumnTextForButtonValue = false,
                FillWeight = 20,
                FlatStyle = FlatStyle.Flat
            });
        }
        return grid;
    }

    private async Task ShowCustomersAsync()
    {
        StartPage("Ügyfelek", "Az ügyfelek adatainak és elérhetőségeinek kezelése.", out var body, out var actions);
        if (CanReception)
        {
            var add = AppTheme.Button("＋  Új ügyfél", true);
            AppTheme.StyleButton(add, true);
            add.Click += async (_, _) =>
            {
                using var dialog = new CustomerDialog(_api);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    await NavigateAsync(CustomersPage);
            };
            actions.Controls.Add(add);
        }
        if (IsManager)
        {
            var account = AppTheme.Button("Ügyfélfiók létrehozása");
            AppTheme.StyleButton(account);
            account.Click += async (_, _) => await CreateCustomerAccountAsync();
            actions.Controls.Add(account);
        }
        var refresh = AppTheme.Button("↻");
        AppTheme.StyleButton(refresh);
        refresh.Click += async (_, _) => await NavigateAsync(CustomersPage);
        actions.Controls.Add(refresh);

        var search = AppTheme.TextBox("Keresés név, telefonszám vagy e-mail alapján...");
        search.Width = 380;
        search.Location = new Point(0, 0);
        body.Controls.Add(search);
        var grid = CreateGrid();
        grid.Location = new Point(0, 54);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 60));
        grid.Columns.Add(TextColumn("Nev", "Ügyfél neve", 27));
        grid.Columns.Add(TextColumn("Telefon", "Telefonszám", 20));
        grid.Columns.Add(TextColumn("Email", "E-mail", 25));
        grid.Columns.Add(TextColumn("Cim", "Cím", 23));
        grid.Columns.Add(TextColumn("Allapot", "Állapot", 12));
        body.Controls.Add(grid);
        body.Resize += (_, _) => grid.Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 60));

        List<UgyfelDto> all = await _api.GetUgyfelekAsync();
        void Populate()
        {
            grid.Rows.Clear();
            foreach (var item in all.Where(x => Matches(search.Text, x.Nev, x.Telefon, x.Email, x.Cim)))
                grid.Rows.Add(item.Nev, item.Telefon, item.Email ?? "–", item.Cim ?? "–", item.Aktiv ? "Aktív" : "Inaktív");
        }
        search.TextChanged += (_, _) => Populate();
        Populate();
    }

    private async Task CreateCustomerAccountAsync()
    {
        try
        {
            var customersTask = _api.GetUgyfelekAsync();
            var usersTask = _api.GetFelhasznalokAsync();
            await Task.WhenAll(customersTask, usersTask);
            using var dialog = new CustomerAccountDialog(customersTask.Result, usersTask.Result);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await _api.CreateFelhasznaloAsync(dialog.Request);
                MessageBox.Show(this, "Az ügyfél bejelentkezési fiókja elkészült. A kezdő jelszót külön add át az ügyfélnek.",
                    "Ügyfélfiók létrehozva", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Nem sikerült létrehozni az ügyfélfiókot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task ShowDevicesAsync()
    {
        StartPage("Eszközök", "A szervizbe korábban bekerült eszközök nyilvántartása.", out var body, out var actions);
        if (CanReception)
        {
            var add = AppTheme.Button("＋  Új eszköz", true);
            AppTheme.StyleButton(add, true);
            add.Click += async (_, _) =>
            {
                using var dialog = new DeviceDialog(_api);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    await NavigateAsync(DevicesPage);
            };
            actions.Controls.Add(add);
        }
        var refresh = AppTheme.Button("↻");
        AppTheme.StyleButton(refresh);
        refresh.Click += async (_, _) => await NavigateAsync(DevicesPage);
        actions.Controls.Add(refresh);

        var search = AppTheme.TextBox("Keresés eszközre, gyártóra vagy sorozatszámra...");
        search.Width = 380;
        search.Location = new Point(0, 0);
        body.Controls.Add(search);
        var grid = CreateGrid();
        grid.Location = new Point(0, 54);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 60));
        grid.Columns.Add(TextColumn("Eszkoz", "Eszköz", 29));
        grid.Columns.Add(TextColumn("Ugyfel", "Tulajdonos", 25));
        grid.Columns.Add(TextColumn("Sorozatszam", "Sorozatszám", 20));
        grid.Columns.Add(TextColumn("Vasarlas", "Vásárlás dátuma", 18));
        body.Controls.Add(grid);
        body.Resize += (_, _) => grid.Size = new Size(Math.Max(700, body.ClientSize.Width - 4), Math.Max(300, body.ClientSize.Height - 60));

        var devices = await _api.GetEszkozokAsync();
        var customers = await _api.GetUgyfelekAsync();
        var names = customers.ToDictionary(x => x.Id, x => x.Nev);
        void Populate()
        {
            grid.Rows.Clear();
            foreach (var item in devices.Where(x => Matches(search.Text, x.EszkozTipus, x.Gyarto, x.Sorozatszam)))
                grid.Rows.Add(item.Megjelenites, names.GetValueOrDefault(item.TulajdonosId, $"Ügyfél #{item.TulajdonosId}"), item.Sorozatszam ?? "–", item.VasarlasDatuma?.ToString("yyyy. MM. dd.") ?? "–");
        }
        search.TextChanged += (_, _) => Populate();
        Populate();
    }

    private async Task ShowRepairDetailsAsync(int id)
    {
        try
        {
            var repair = await _api.GetJavitasAsync(id);
            using var dialog = new RepairDetailsDialog(_api, repair, _user);
            dialog.ShowDialog(this);
            await NavigateAsync(RepairsPage);
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Nem sikerült betölteni", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private Panel CreateStatCard(string title, string value, string detail, Color accent)
    {
        var card = AppTheme.Card();
        card.Size = new Size(190, 116);
        var stripe = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent };
        card.Controls.Add(stripe);
        var titleLabel = AppTheme.Label(title, 9, true, AppTheme.Muted);
        titleLabel.Location = new Point(19, 16);
        card.Controls.Add(titleLabel);
        var valueLabel = AppTheme.Label(value, 24, true);
        valueLabel.Location = new Point(19, 38);
        card.Controls.Add(valueLabel);
        var detailLabel = AppTheme.Label(detail, 8.5f, false, AppTheme.Muted);
        detailLabel.Location = new Point(20, 79);
        card.Controls.Add(detailLabel);
        return card;
    }

    private static DataGridView CreateGrid()
    {
        var grid = new DataGridView { Dock = DockStyle.None };
        AppTheme.StyleGrid(grid);
        return grid;
    }

    private static DataGridViewTextBoxColumn TextColumn(string name, string header, float fill)
        => new() { Name = name, HeaderText = header, FillWeight = fill, SortMode = DataGridViewColumnSortMode.NotSortable };

    private static void ApplyStatusColors(DataGridView grid, string columnName)
    {
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex >= 0 && grid.Columns.Contains(columnName) && e.ColumnIndex == grid.Columns[columnName].Index)
            {
                var raw = Convert.ToString(e.Value) ?? "";
                var original = raw switch
                {
                    "Átvett" => "Atvett",
                    "Bevizsgálás alatt" => "BevizsgalasAlatt",
                    "Javítás alatt" => "JavitasAlatt",
                    "Alkatrészre vár" => "AlkatreszreVar",
                    "Lezárt" => "Lezart",
                    "Visszaadva" => "Visszaadva",
                    _ => raw
                };
                if (e.CellStyle is not null)
                {
                    e.CellStyle.ForeColor = AppTheme.StatusColor(original);
                    e.CellStyle.Font = AppTheme.Font(9, true);
                }
            }
        };
    }

    private static bool Matches(string filter, params string?[] values)
        => string.IsNullOrWhiteSpace(filter) || values.Any(x => x?.Contains(filter.Trim(), StringComparison.CurrentCultureIgnoreCase) == true);

    private static string RoleLabel(string role) => role switch
    {
        "Manager" => "Rendszergazda",
        "Recepcios" => "Recepciós",
        "Szerelo" => "Szerelő",
        "Ugyfel" => "Ügyfél",
        _ => role
    };

    private void ShowErrorPage(string message, string currentPage)
    {
        StartPage("Nem sikerült betölteni", "Ellenőrizd az API kapcsolatot, majd próbáld újra.", out var body, out var actions);
        var retry = AppTheme.Button("↻  Újrapróbálás", true);
        AppTheme.StyleButton(retry, true);
        retry.Click += async (_, _) => await NavigateAsync(currentPage);
        actions.Controls.Add(retry);
        var card = AppTheme.Card();
        card.Dock = DockStyle.Top;
        card.Height = 110;
        var label = AppTheme.Label(message, 10, false, AppTheme.Red);
        label.MaximumSize = new Size(850, 70);
        label.Location = new Point(20, 20);
        card.Controls.Add(label);
        body.Controls.Add(card);
    }

    private async Task SafeRunAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private async Task LogoutAsync()
    {
        try { await _api.LogoutAsync(); }
        catch { /* A helyi munkamenetet kijelentkezéskor akkor is lezárjuk, ha a szerver nem elérhető. */ }
        Close();
    }
}
