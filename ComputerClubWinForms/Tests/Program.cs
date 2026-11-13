using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Forms;
using ComputerClubWinForms.Managers;
using ComputerClubWinForms.Models;
using Npgsql;

internal static class Program
{
    private static DatabaseHelper Db = null!;
    private static ClientManager Clients = null!;
    private static SessionManager Sessions = null!;
    private static int Passed;
    private static int Failed;
    private static readonly string TestDatabaseName = "club_smoke_test_" + Guid.NewGuid().ToString("N");
    private static string TestConnectionString = string.Empty;
    private static bool TestDatabaseCreated;

    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args.FirstOrDefault() == "connection-contract")
        {
            ApiContractTests();
            Console.WriteLine($"RESULT passed={Passed} failed={Failed}");
            return Failed == 0 ? 0 : 1;
        }
        var options = new NpgsqlConnectionStringBuilder(new DatabaseHelper().ConnectionString)
        {
            Database = TestDatabaseName,
            Pooling = true
        };
        TestConnectionString = options.ConnectionString;
        GuardTestDatabase(TestConnectionString);
        Db = NewHelper(TestConnectionString);
        try
        {
            var suite = args.FirstOrDefault() ?? "all";
            if (suite is "database-contract" or "all")
            {
                ApiContractTests();
                MissingDatabaseContract();
            }
            CreateTestDatabase();
            if (suite is "database-contract" or "all") DatabaseContractTests();
            ApplyFixture();
            Clients = new ClientManager(Db);
            Sessions = new SessionManager(Db, Clients, new TariffManager(Db, Clients));
            if (suite is "auth" or "all") AuthTests();
            if (suite is "clients" or "all") ClientTests();
            if (suite is "computers" or "all") ComputerTests();
            if (suite is "processes" or "all") ProcessTests();
            if (suite is "refresh" or "all") RefreshTests();
            if (suite is "auto-finish" or "all") AutoFinishTests();
            var captureIndex = Array.IndexOf(args, "--capture");
            if (captureIndex >= 0) CaptureScreens(args[captureIndex + 1]);
            Console.WriteLine($"RESULT passed={Passed} failed={Failed}");
            return Failed == 0 ? 0 : 1;
        }
        finally
        {
            if (TestDatabaseCreated) DropTestDatabase();
        }
    }

    private static void Test(string name, Action body, bool reset = true)
    {
        try { if (reset) Reset(); body(); Passed++; Console.WriteLine("PASS " + name); }
        catch (Exception ex) { Failed++; Console.WriteLine("FAIL " + name + ": " + (ex.InnerException?.Message ?? ex.Message)); }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static DatabaseHelper NewHelper(string connectionString)
    {
        var constructor = typeof(DatabaseHelper).GetConstructor(new[] { typeof(string) });
        Check(constructor is not null, "DatabaseHelper has no explicit connection-string constructor.");
        return (DatabaseHelper)constructor!.Invoke(new object[] { connectionString });
    }

    private static void GuardTestDatabase(string connectionString)
    {
        var database = new NpgsqlConnectionStringBuilder(connectionString).Database;
        Check(Regex.IsMatch(TestDatabaseName, @"\Aclub_smoke_test_[0-9a-f]{32}\z", RegexOptions.CultureInvariant)
            && database == TestDatabaseName, "Refusing to modify a database outside this test run's exact GUID name.");
    }

    private static NpgsqlConnection AdministrativeConnection()
    {
        GuardTestDatabase(TestConnectionString);
        var options = new NpgsqlConnectionStringBuilder(TestConnectionString) { Database = "postgres", Pooling = false };
        var connection = new NpgsqlConnection(options.ConnectionString);
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    private static void CreateTestDatabase()
    {
        GuardTestDatabase(TestConnectionString);
        using var connection = AdministrativeConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{TestDatabaseName}\"";
        command.ExecuteNonQuery();
        TestDatabaseCreated = true;
    }

    private static void DropTestDatabase()
    {
        GuardTestDatabase(TestConnectionString);
        NpgsqlConnection.ClearAllPools();
        using var connection = AdministrativeConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{TestDatabaseName}\" WITH (FORCE)";
        command.ExecuteNonQuery();
        TestDatabaseCreated = false;
        Console.WriteLine("Disposable test database removed.");
    }

    private static void ApplyFixture() => Sql(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "schema.sql")));

    private static object? Scalar(string sql)
    {
        GuardTestDatabase(TestConnectionString);
        using var connection = new NpgsqlConnection(TestConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand(sql, connection);
        return command.ExecuteScalar();
    }

    private static bool TestDatabaseExists()
    {
        GuardTestDatabase(TestConnectionString);
        using var connection = AdministrativeConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @name)";
        command.Parameters.AddWithValue("@name", TestDatabaseName);
        return (bool)command.ExecuteScalar()!;
    }

    private static void ApiContractTests()
    {
        Test("DatabaseHelper has no bootstrap or database-name API", () =>
        {
            Check(typeof(DatabaseHelper).GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance) is null, "DatabaseHelper still exposes Initialize.");
            Check(typeof(DatabaseHelper).GetProperty("DatabaseName") is null, "DatabaseHelper still exposes a separately resolved DatabaseName.");
        }, reset: false);
        Test("An explicit connection string is stored verbatim without parsing or opening", () =>
        {
            var constructor = typeof(DatabaseHelper).GetConstructor(new[] { typeof(string) });
            Check(constructor is not null, "DatabaseHelper has no explicit connection-string constructor.");
            const string unparsed = "not parsed until CreateConnection";
            var helper = (DatabaseHelper)constructor!.Invoke(new object[] { unparsed });
            Check(helper.ConnectionString == unparsed, "The constructor changed or ignored the supplied connection string.");
        }, reset: false);
        Test("The explicit constructor rejects null and blank connection strings", () =>
        {
            var constructor = typeof(DatabaseHelper).GetConstructor(new[] { typeof(string) });
            Check(constructor is not null, "DatabaseHelper has no explicit connection-string constructor.");
            foreach (var value in new string?[] { null, "", "   " })
            {
                var rejected = false;
                try { constructor!.Invoke(new object?[] { value }); }
                catch (TargetInvocationException ex) when (ex.InnerException is ArgumentException) { rejected = true; }
                Check(rejected, "A null or blank connection string was accepted.");
            }
        }, reset: false);
    }

    private static void MissingDatabaseContract()
    {
        Test("Opening a missing database fails without creating it", () =>
        {
            Check(!TestDatabaseExists(), "The generated missing-database fixture already exists.");
            var failed = false;
            try { using var connection = Db.CreateConnection(); }
            catch (PostgresException ex) when (ex.SqlState == "3D000") { failed = true; }
            Check(failed, "Opening a missing database did not report PostgreSQL invalid_catalog_name.");
            Check(!TestDatabaseExists(), "Opening the missing database created it.");
        }, reset: false);
    }

    private static void DatabaseContractTests()
    {
        Test("The complete explicit connection string reaches PostgreSQL unchanged", () =>
        {
            var options = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                ApplicationName = "club-connection-contract",
                SearchPath = "pg_catalog",
                Options = "-c default_transaction_read_only=on",
                Timeout = 4,
                CommandTimeout = 5,
                IncludeErrorDetail = false
            };
            var supplied = options.ConnectionString;
            var helper = NewHelper(supplied);
            Check(helper.ConnectionString == supplied, "The full explicit connection string was rewritten.");
            using var connection = helper.CreateConnection();
            Check(connection.State == System.Data.ConnectionState.Open, "CreateConnection returned a closed connection.");
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT current_database(), current_setting('application_name'), current_setting('search_path'), current_setting('default_transaction_read_only')";
            using var reader = command.ExecuteReader();
            Check(reader.Read() && reader.GetString(0) == TestDatabaseName
                && reader.GetString(1) == "club-connection-contract" && reader.GetString(2) == "pg_catalog"
                && reader.GetString(3) == "on", "Explicit database or connection options were ignored.");
        }, reset: false);
        Test("Opening an empty schema creates no tables or seed data", () =>
        {
            var before = PublicSchemaSnapshot();
            Check(Convert.ToInt64(Scalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'")) == 0, "The empty-schema fixture already contains tables.");
            using (var connection = Db.CreateConnection())
                Check(connection.State == System.Data.ConnectionState.Open, "The existing empty database cannot be opened.");
            Check(PublicSchemaSnapshot() == before, "Opening an empty database created schema objects.");
            Check(Convert.ToInt64(Scalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'")) == 0, "Opening an empty database added application tables or seed data.");
        }, reset: false);
        Test("An incompatible schema and sentinel row remain unchanged", () =>
        {
            Sql("CREATE TABLE Clients (Id INTEGER PRIMARY KEY); INSERT INTO Clients(Id) VALUES (725)");
            try
            {
                var before = PublicSchemaSnapshot();
                using (var connection = Db.CreateConnection())
                    Check(connection.State == System.Data.ConnectionState.Open, "The existing incompatible database cannot be opened.");
                var failed = false;
                try { new ClientManager(Db).GetAllClients(); }
                catch (PostgresException ex) when (ex.SqlState == "42703") { failed = true; }
                Check(failed, "The incompatible table was silently repaired instead of reporting the missing column.");
                Check(PublicSchemaSnapshot() == before && Convert.ToInt32(Scalar("SELECT Id FROM Clients")) == 725, "Opening or reading the incompatible database changed its schema or sentinel data.");
            }
            finally { Sql("DROP TABLE Clients"); }
        }, reset: false);
        Test("Opening and reading a ready database preserves schema and existing data", () =>
        {
            ApplyFixture();
            Sql("INSERT INTO Clients(FullName, Phone) VALUES ('Сохранённый клиент', '79001234567'); INSERT INTO Computers(Number, Name) VALUES (9, 'Существующий ПК'); DELETE FROM Settings");
            var schema = PublicSchemaSnapshot();
            var data = ReadyDataSnapshot();
            using (var first = Db.CreateConnection())
            using (var second = NewHelper(TestConnectionString).CreateConnection())
                Check(first.State == System.Data.ConnectionState.Open && second.State == System.Data.ConnectionState.Open, "Repeated opening failed.");
            var clients = new ClientManager(Db);
            var tariffs = new TariffManager(Db, clients);
            var sessions = new SessionManager(Db, clients, tariffs);
            Check(clients.GetAllClients().Single().Phone == "79001234567" && sessions.GetComputers().Single().Number == 9, "Existing application data cannot be read.");
            Check(tariffs.GetCurrentTariff() == 150m, "The existing read-only tariff fallback changed.");
            Check(new UserManager(Db).Login("absent-user", "fixture-only") is null, "An unseeded account unexpectedly appeared.");
            Check(PublicSchemaSnapshot() == schema && ReadyDataSnapshot() == data, "Opening or reading the ready database changed schema, inventory, users or settings.");
        }, reset: false);
    }

    private static string PublicSchemaSnapshot() => (string)Scalar("""
        SELECT json_build_object(
            'objects', (SELECT json_agg(x ORDER BY relname) FROM (
                SELECT c.relname, c.relkind::text, c.oid FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace WHERE n.nspname = 'public') x),
            'columns', (SELECT json_agg(x ORDER BY table_name, ordinal_position) FROM (
                SELECT table_name, column_name, ordinal_position, data_type, is_nullable, column_default
                FROM information_schema.columns WHERE table_schema = 'public') x)
        )::text
        """)!;

    private static string ReadyDataSnapshot() => (string)Scalar("""
        SELECT json_build_object(
            'users', (SELECT json_agg(x ORDER BY id) FROM Users x),
            'clients', (SELECT json_agg(x ORDER BY id) FROM Clients x),
            'computers', (SELECT json_agg(x ORDER BY number) FROM Computers x),
            'bookings', (SELECT json_agg(x ORDER BY id) FROM Bookings x),
            'sessions', (SELECT json_agg(x ORDER BY id) FROM Sessions x),
            'settings', (SELECT json_agg(x ORDER BY settingkey) FROM Settings x)
        )::text
        """)!;

    private static void AuthTests()
    {
        Test("A prepared administrator account can log in and log out", () =>
        {
            Sql("INSERT INTO Users(Username, Password, Role) VALUES ('fixture_admin', 'fixture-only-password', 'admin')");
            var users = new UserManager(Db);
            var user = users.Login("fixture_admin", "fixture-only-password");
            Check(user is { IsAdmin: true } && users.GetCurrentUser()?.Id == user.Id, "The prepared administrator could not log in.");
            users.Logout();
            Check(users.GetCurrentUser() is null, "Logging out retained the current user.");
        });
        Test("A missing Settings table leaves login visible with an inline error", () =>
        {
            Sql("INSERT INTO Users(Username, Password, Role) VALUES ('fixture_admin', 'fixture-only-password', 'admin'); DROP TABLE Settings");
            try
            {
                var schema = PublicSchemaSnapshot();
                using var auth = new AuthPage(Db)
                {
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(-32000, -32000),
                    ShowInTaskbar = false,
                    Opacity = 0
                };
                auth.Show();
                Application.DoEvents();
                Field<TextBox>(auth, "_usernameTextBox").Text = "fixture_admin";
                Field<TextBox>(auth, "_passwordTextBox").Text = "fixture-only-password";
                Exception? escaped = null;
                try { Field<Button>(auth, "_loginButton").PerformClick(); }
                catch (Exception ex) { escaped = ex; }
                Check(escaped is null, "A database exception escaped the login action instead of showing an inline error.");
                Check(auth.Visible && !string.IsNullOrWhiteSpace(Field<Label>(auth, "_errorLabel").Text), "The failed main-window load hid login or left its error empty.");
                Check(!Application.OpenForms.OfType<MainForm>().Any(), "A failed main-window load left a main form open.");
                Check(PublicSchemaSnapshot() == schema && Convert.ToInt64(Scalar("SELECT COUNT(*) FROM Users")) == 1, "Failed login repaired the schema or changed the prepared account.");
            }
            finally
            {
                foreach (var form in Application.OpenForms.OfType<MainForm>().ToArray()) form.Dispose();
                ApplyFixture();
            }
        });
    }

    private static void Reset()
    {
        GuardTestDatabase(Db.ConnectionString);
        Sql("TRUNCATE Sessions, Bookings, Clients, Computers, Users, Settings RESTART IDENTITY CASCADE");
        Sql("INSERT INTO Settings(SettingKey, Value) VALUES ('TariffPerHour', '150')");
    }
    private static void Sql(string sql)
    {
        GuardTestDatabase(TestConnectionString);
        using var connection = new NpgsqlConnection(TestConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private static long CountComputers()
    {
        using var connection = Db.CreateConnection();
        using var command = new NpgsqlCommand("SELECT COUNT(*) FROM Computers", connection);
        return (long)command.ExecuteScalar()!;
    }

    private static Client AddClient(string name = "Иванов Иван", string phone = "79001112233")
    {
        var client = new Client { FullName = name, Phone = phone };
        Check(Clients.AddClient(client), Clients.LastMessage);
        return Clients.GetAllClients().Single(c => c.Phone == phone);
    }

    private static void AddRawComputer(int number) => Sql($"INSERT INTO Computers(Number, Name, IsActive) VALUES ({number}, 'ПК {number}', TRUE)");
    private static T Call<T>(object target, string method, params object[] args)
    {
        var info = target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
        Check(info is not null, "Missing operation: " + method);
        return (T)info!.Invoke(target, args)!;
    }

    private static T Field<T>(object target, string name) where T : class
    {
        var info = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Check(info is not null, "Required user action is unavailable: " + name);
        return (T)info!.GetValue(target)!;
    }

    private static Form Host(Control page)
    {
        var form = new Form { ClientSize = new Size(1080, 730), StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000), ShowInTaskbar = false, Opacity = 0 };
        page.Dock = DockStyle.Fill;
        form.Controls.Add(page);
        form.Show();
        Application.DoEvents();
        return form;
    }

    private static void ClientTests()
    {
        Test("Adding a client returns its database identity for immediate selection", () =>
        {
            var client = new Client { FullName = "Новый клиент", Phone = "79001112233" };
            Check(Clients.AddClient(client), Clients.LastMessage);
            Check(client.Id > 0 && Clients.GetClientById(client.Id)?.Phone == client.Phone, "The created client has no returned ID.");
        });
        Test("Name lookup ignores letter case and phone lookup still works", () =>
        {
            AddClient();
            Check(Clients.GetAllClients("иванов").Count == 1, "Lowercase Cyrillic name was not found.");
            Check(Clients.GetAllClients("11122").Count == 1, "Phone fragment was not found.");
        });
        Test("Session page filters namesakes by phone and handles empty results", () =>
        {
            var first = AddClient();
            var second = AddClient("Иванов Иван", "79009998877");
            AddRawComputer(1);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var search = Field<TextBox>(page, "_txtClientSearch");
            var combo = Field<ComboBox>(page, "_cmbClients");
            search.Text = "9988";
            Check(combo.Items.Count == 1 && (int)combo.SelectedValue! == second.Id, "The filtered client is incorrect.");
            Check(combo.GetItemText(combo.Items[0])?.Contains("79009998877") == true, "Namesakes cannot be distinguished by phone.");
            search.Text = "не найден";
            Check(combo.Items.Count == 0 && !Field<Button>(page, "_btnNewBooking").Enabled, "Booking is enabled without a matching client.");
            search.Clear();
            Check(combo.Items.Count == 2, "Clearing the filter did not restore clients.");
        });
        Test("Quick creation selects the saved client and notifies the client page", () =>
        {
            AddClient(); AddRawComputer(1);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            Field<TextBox>(page, "_txtClientSearch").Text = "Новый";
            var notified = false;
            var notification = page.GetType().GetEvent("ClientsChanged");
            Check(notification is not null, "Other pages are not notified about client creation.");
            notification!.AddEventHandler(page, new EventHandler((_, _) => notified = true));
            void HideDialog(object? sender, EventArgs e)
            {
                var dialog = Application.OpenForms.OfType<ClientEditDialog>().FirstOrDefault();
                if (dialog is null) return;
                dialog.Opacity = 0;
                dialog.ShowInTaskbar = false;
                dialog.StartPosition = FormStartPosition.Manual;
                dialog.Location = new Point(-32000, -32000);
            }
            using var timer = new System.Windows.Forms.Timer { Interval = 50 };
            timer.Tick += (_, _) =>
            {
                var dialog = Application.OpenForms.OfType<ClientEditDialog>().FirstOrDefault();
                if (dialog is null) return;
                HideDialog(null, EventArgs.Empty);
                timer.Stop();
                Field<TextBox>(dialog, "_txtFullName").Text = "Новый посетитель";
                Field<TextBox>(dialog, "_txtPhone").Text = "79005556677";
                Field<Button>(dialog, "_btnOk").PerformClick();
            };
            Application.EnterThreadModal += HideDialog;
            try
            {
                timer.Start();
                Field<Button>(page, "_btnAddClient").PerformClick();
            }
            finally
            {
                timer.Stop();
                Application.EnterThreadModal -= HideDialog;
            }
            var saved = Clients.GetAllClients("79005556677").Single();
            Check((int)Field<ComboBox>(page, "_cmbClients").SelectedValue! == saved.Id, "New client was not selected.");
            Check(notified, "Client change event did not reach subscribers.");
        });
        Test("A long client name does not hide the phone in the dropdown", () =>
        {
            AddClient("Константинопольский Александр Александрович", "79001112233");
            AddRawComputer(1);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var combo = Field<ComboBox>(page, "_cmbClients");
            var width = TextRenderer.MeasureText(combo.GetItemText(combo.Items[0]), combo.Font).Width;
            Check(combo.DropDownWidth >= width + 20, "The phone is clipped for a long Russian name.");
        });
        Test("The booking grid shows a complete row at the minimum window size", () =>
        {
            AddClient(); AddRawComputer(1);
            using var form = new MainForm(Db, new User { Username = "admin", Role = "admin" });
            form.WindowState = FormWindowState.Normal;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-32000, -32000);
            form.ShowInTaskbar = false;
            form.Opacity = 0;
            form.Size = form.MinimumSize;
            form.Show();
            var tabs = Field<TabControl>(form, "_mainTabControl");
            tabs.SelectedIndex = 1;
            Application.DoEvents();
            var page = (SessionPage)tabs.SelectedTab!.Controls[0];
            var grid = Field<DataGridView>(page, "_gridBookings");
            Check(grid.ClientSize.Height >= grid.ColumnHeadersHeight + grid.RowTemplate.Height, "Booking grid cannot show its header and one complete row.");
        });
    }

    private static void ComputerTests()
    {
        Test("Opening an existing database preserves an intentionally empty computer list", () =>
        {
            using var connection = Db.CreateConnection();
            Check(CountComputers() == 0, "Opening the database invented computers.");
        });
        Test("Database additions appear on the map and can be selected without a computer page", () =>
        {
            AddClient();
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            Check(!Field<Button>(page, "_btnNewBooking").Enabled, "Booking is enabled with no computers.");
            AddRawComputer(9);
            WaitUntil(() => Field<Panel>(page, "_computerMap").Controls.Count == 1, "Database computer did not appear on the map.");
            var combo = Field<ComboBox>(page, "_cmbComputers");
            Check(combo.Items.Count == 1, "The database computer is visible but cannot be selected for booking.");
            Field<Panel>(page, "_computerMap").Controls.OfType<Button>().Single().PerformClick();
            Check((int)combo.SelectedValue! == 9, "Map click did not select the database computer.");
            Check(Field<Button>(page, "_btnNewBooking").Enabled, "Booking stayed disabled after adding a database computer.");
        });
        Test("Maps use the database list for five and thirty-five computers", () =>
        {
            for (var i = 1; i <= 5; i++) AddRawComputer(i);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            Check(Field<Panel>(page, "_computerMap").Controls.Count == 5, "Map does not show five computers.");
            for (var i = 6; i <= 35; i++) AddRawComputer(i);
            Call<object?>(page, "ReloadComputers");
            Check(Field<Panel>(page, "_computerMap").Controls.Count == 35, "Map does not show thirty-five database computers.");
            using var booking = new BookingDialog(Sessions, 1, 1);
            Check(Field<Panel>(booking, "_computerMap").Controls.Count == 35, "Booking map differs from the database list.");
            using var connection = Db.CreateConnection();
            Check(CountComputers() == 35, "Opening the database changed the manually configured computer list.");
        });
        Test("The administrator and manager have no computer administration tab", () =>
        {
            using var admin = new MainForm(Db, new User { Username = "admin", Role = "admin" });
            using var manager = new MainForm(Db, new User { Username = "manager", Role = "manager" });
            var adminTabs = Field<TabControl>(admin, "_mainTabControl").TabPages.Cast<TabPage>().Select(p => p.Text).ToArray();
            var managerTabs = Field<TabControl>(manager, "_mainTabControl").TabPages.Cast<TabPage>().Select(p => p.Text).ToArray();
            Check(adminTabs.SequenceEqual(new[] { "Клиенты", "Сеансы", "Отчёты", "Настройки" }), "Administrator still has a computer administration page.");
            Check(managerTabs.SequenceEqual(new[] { "Клиенты", "Сеансы" }), "Manager tabs changed.");
        });
    }

    private static void ProcessTests()
    {
        Test("Booking accepts adjacent intervals and rejects overlaps", () =>
        {
            var client = AddClient(); AddRawComputer(1);
            var start = DateTime.Today.AddDays(1).AddHours(18);
            Check(Sessions.CreateBooking(client.Id, 1, start, TimeSpan.FromHours(1)), Sessions.LastMessage);
            Check(!Sessions.CreateBooking(client.Id, 1, start.AddMinutes(30), TimeSpan.FromHours(1)), "Overlapping booking was accepted.");
            Check(Sessions.CreateBooking(client.Id, 1, start.AddHours(1), TimeSpan.FromHours(1)), "Adjacent booking was rejected.");
            Check(!Sessions.CreateBooking(client.Id, 1, DateTime.Now.AddHours(-1), TimeSpan.FromHours(1)), "Past booking was accepted.");
        });
        Test("A direct session can be extended and completed with saved client statistics", () =>
        {
            var client = AddClient(); AddRawComputer(1);
            var session = Sessions.StartSessionDirect(client.Id, 1, DateTime.Now, TimeSpan.FromMinutes(30));
            Check(session is not null, Sessions.LastMessage);
            Check(Sessions.ExtendSession(session!.Id, TimeSpan.FromMinutes(15)), Sessions.LastMessage);
            var preview = Sessions.PreviewCompletion(session.Id);
            Check(preview is not null && preview.TotalCost > 0, "Completion preview has no cost.");
            var completed = Sessions.CompleteSession(session.Id);
            Check(completed is { IsCompleted: true } && completed.TotalCost > 0, "Completion was not saved.");
            Check(Clients.GetClientById(client.Id)!.TotalSpent == completed!.TotalCost, "Client statistics differ from the completed session.");
            Check(Sessions.GetActiveSessions().Count == 0, "Completed session remains active.");
        });
    }

    private static void RefreshTests()
    {
        Test("Unchanged timer ticks retain PC buttons and map scroll", () =>
        {
            AddClient();
            for (var i = 1; i <= 35; i++) AddRawComputer(i);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var map = Field<Panel>(page, "_computerMap");
            var buttons = map.Controls.OfType<Button>().ToDictionary(button => (int)button.Tag!);
            Field<ComboBox>(page, "_cmbComputers").SelectedValue = 12;
            map.AutoScrollPosition = new Point(0, 30);
            var scroll = map.AutoScrollPosition;
            Check(scroll.Y < 0, "The fixture has no map scrolling to preserve.");
            var locationChanges = 0;
            var colorChanges = 0;
            foreach (var button in buttons.Values)
            {
                button.LocationChanged += (_, _) => locationChanges++;
                button.BackColorChanged += (_, _) => colorChanges++;
            }
            WaitForTicks(page, 2);
            Check(buttons.All(pair => ReferenceEquals(pair.Value, MapButton(page, pair.Key))), "The timer recreated unchanged PC buttons.");
            Check(map.AutoScrollPosition == scroll, "The timer reset map scrolling.");
            Check(locationChanges == 0 && colorChanges == 0, "Unchanged PCs were moved or recolored on a timer tick.");
            Check((int)Field<ComboBox>(page, "_cmbComputers").SelectedValue! == 12, "The timer changed the selected PC.");
        });
        Test("Availability changes update the existing PC button", () =>
        {
            var client = AddClient(); AddRawComputer(1);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var button = MapButton(page, 1);
            Check(button.BackColor == Color.LightGreen, "The PC did not start available.");
            Check(Sessions.StartSessionDirect(client.Id, 1, DateTime.Now, TimeSpan.FromHours(1)) is not null, Sessions.LastMessage);
            WaitUntil(() => MapButton(page, 1).BackColor == Color.LightCoral, "The timer did not show the PC becoming occupied.");
            Check(ReferenceEquals(button, MapButton(page, 1)), "Changing availability replaced the PC button.");
        });
        Test("Database PC list changes preserve retained buttons and dispose removed ones", () =>
        {
            AddClient(); AddRawComputer(1); AddRawComputer(2);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var removed = MapButton(page, 1);
            var retained = MapButton(page, 2);
            var combo = Field<ComboBox>(page, "_cmbComputers");
            combo.SelectedValue = 2;
            Sql("UPDATE Computers SET IsActive = FALSE WHERE Number = 1");
            AddRawComputer(3);
            WaitUntil(() => Field<Panel>(page, "_computerMap").Controls.OfType<Button>().Select(button => (int)button.Tag!).SequenceEqual(new[] { 2, 3 }), "The map did not follow the changed database list.");
            Check(ReferenceEquals(retained, MapButton(page, 2)), "A database list change replaced a retained PC button.");
            Check(removed.IsDisposed, "The removed PC button was not disposed.");
            Check((int)combo.SelectedValue! == 2 && combo.Items.Count == 2, "The PC dropdown lost its valid selection or database list.");
        });
        Test("Timer refresh preserves the selected session identity and current column", () =>
        {
            var client = AddClient();
            var secondClient = AddClient("Иванов Иван", "79002223344");
            var otherClient = AddClient("Петров Пётр", "79009998877");
            for (var i = 1; i <= 3; i++) AddRawComputer(i);
            var first = Sessions.StartSessionDirect(client.Id, 1, DateTime.Now, TimeSpan.FromMinutes(30));
            var second = Sessions.StartSessionDirect(secondClient.Id, 2, DateTime.Now, TimeSpan.FromMinutes(60));
            var third = Sessions.StartSessionDirect(otherClient.Id, 3, DateTime.Now, TimeSpan.FromMinutes(90));
            Check(first is not null && second is not null && third is not null, Sessions.LastMessage);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var grid = Field<DataGridView>(page, "_gridActiveSessions");
            var combo = Field<ComboBox>(page, "_cmbClients");
            combo.SelectedValue = otherClient.Id;
            grid.CurrentCell = grid.Rows.Cast<DataGridViewRow>().Single(row => Convert.ToInt32(row.Cells[0].Value) == second!.Id).Cells[3];
            WaitForTicks(page, 2);
            Check(Convert.ToInt32(grid.CurrentRow?.Cells[0].Value) == second!.Id && grid.CurrentCell?.ColumnIndex == 3, "A timer tick moved the selected session or column.");
            Check((int)combo.SelectedValue! == otherClient.Id, "The timer changed the independent client dropdown.");
            Check(Sessions.ExtendSession(second.Id, TimeSpan.FromMinutes(90)), Sessions.LastMessage);
            WaitUntil(() => Convert.ToInt32(grid.Rows[2].Cells[0].Value) == second.Id, "The timer did not display the reordered sessions.");
            Check(Convert.ToInt32(grid.CurrentRow?.Cells[0].Value) == second.Id && grid.CurrentCell?.ColumnIndex == 3, "Session selection followed a row index instead of its ID.");
        });
        Test("A removed session leaves no selected action target on later timer ticks", () =>
        {
            var client = AddClient(); AddRawComputer(1); AddRawComputer(2);
            var secondClient = AddClient("Иванов Иван", "79002223344");
            Check(Sessions.StartSessionDirect(client.Id, 1, DateTime.Now, TimeSpan.FromMinutes(30)) is not null, Sessions.LastMessage);
            var second = Sessions.StartSessionDirect(secondClient.Id, 2, DateTime.Now, TimeSpan.FromMinutes(60));
            Check(second is not null, Sessions.LastMessage);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var grid = Field<DataGridView>(page, "_gridActiveSessions");
            grid.CurrentCell = grid.Rows.Cast<DataGridViewRow>().Single(row => Convert.ToInt32(row.Cells[0].Value) == second!.Id).Cells[1];
            Check(Sessions.CompleteSession(second!.Id) is not null, Sessions.LastMessage);
            WaitUntil(() => grid.Rows.Count == 1, "The completed session remains on screen.");
            Check(grid.CurrentCell is null && grid.SelectedRows.Count == 0, "Removing the selected session silently selected a different session.");
            WaitForTicks(page, 2);
            Check(grid.CurrentCell is null && grid.CurrentRow is null && grid.SelectedRows.Count == 0, "A later timer tick selected a different session without user input.");
        });
        Test("Rebinding sessions preserves vertical and horizontal scrolling", () =>
        {
            using var page = new SessionPage();
            using var host = Host(page);
            var sessions = Enumerable.Range(1, 30).Select(id => new Session { Id = id, ClientName = "Клиент " + id, ComputerNumber = id, StartTime = DateTime.Now, PlannedEndTime = DateTime.Now.AddHours(1) }).ToList();
            page.ShowActiveSessions(sessions);
            var grid = Field<DataGridView>(page, "_gridActiveSessions");
            grid.Columns[1].MinimumWidth = 1000;
            grid.CurrentCell = grid.Rows[12].Cells[3];
            grid.FirstDisplayedScrollingRowIndex = 10;
            grid.HorizontalScrollingOffset = 100;
            var vertical = grid.FirstDisplayedScrollingRowIndex;
            var horizontal = grid.HorizontalScrollingOffset;
            Check(vertical > 0 && horizontal > 0, "The fixture has no grid scrolling to preserve.");
            page.ShowActiveSessions(sessions);
            Check(Convert.ToInt32(grid.CurrentRow?.Cells[0].Value) == 13 && grid.CurrentCell?.ColumnIndex == 3, "Rebinding changed the selected session.");
            Check(grid.FirstDisplayedScrollingRowIndex == vertical && grid.HorizontalScrollingOffset == horizontal, "Rebinding reset table scrolling.");
        });
        Test("Booking refresh preserves identity and keeps no selection after removal", () =>
        {
            using var page = new SessionPage();
            using var host = Host(page);
            var bookings = Enumerable.Range(1, 3).Select(id => new Booking { Id = id, ClientName = "Один клиент", ComputerNumber = id, PlannedStart = DateTime.Today.AddDays(1), Duration = TimeSpan.FromHours(1) }).ToList();
            page.ShowBookings(bookings);
            var grid = Field<DataGridView>(page, "_gridBookings");
            grid.CurrentCell = grid.Rows[1].Cells[4];
            page.ShowBookings(new List<Booking> { bookings[2], bookings[0], bookings[1] });
            Check(Convert.ToInt32(grid.CurrentRow?.Cells[0].Value) == 2 && grid.CurrentCell?.ColumnIndex == 4, "Booking selection followed a row position instead of its ID.");
            page.ShowBookings(new List<Booking> { bookings[0], bookings[2] });
            Check(grid.CurrentCell is null && grid.SelectedRows.Count == 0, "Removing the selected booking silently selected another booking.");
            page.ShowBookings(new List<Booking> { bookings[0], bookings[2] });
            Check(grid.CurrentCell is null && grid.CurrentRow is null, "A later booking refresh created an unintended action target.");
        });
        Test("An empty session refresh does not select a later different session", () =>
        {
            using var page = new SessionPage();
            using var host = Host(page);
            var first = new Session { Id = 1, ClientName = "Первый клиент", ComputerNumber = 1, StartTime = DateTime.Now, PlannedEndTime = DateTime.Now.AddHours(1) };
            var later = new Session { Id = 2, ClientName = "Другой клиент", ComputerNumber = 2, StartTime = DateTime.Now, PlannedEndTime = DateTime.Now.AddHours(1) };
            page.ShowActiveSessions(new List<Session> { first });
            var grid = Field<DataGridView>(page, "_gridActiveSessions");
            grid.CurrentCell = grid.Rows[0].Cells[1];
            page.ShowActiveSessions(new List<Session>());
            Check(grid.CurrentCell is null, "The removed session retained an action target.");
            page.ShowActiveSessions(new List<Session> { later });
            Check(grid.CurrentCell is null && grid.CurrentRow is null && grid.SelectedRows.Count == 0, "A session arriving after an empty refresh became an unintended action target.");
            page.ShowActiveSessions(new List<Session> { later });
            Check(grid.CurrentCell is null && grid.CurrentRow is null, "A later refresh selected the new session without user input.");
        });
        Test("An empty booking refresh does not select a later different booking", () =>
        {
            using var page = new SessionPage();
            using var host = Host(page);
            var first = new Booking { Id = 1, ClientName = "Первый клиент", ComputerNumber = 1, PlannedStart = DateTime.Today.AddDays(1), Duration = TimeSpan.FromHours(1) };
            var later = new Booking { Id = 2, ClientName = "Другой клиент", ComputerNumber = 2, PlannedStart = DateTime.Today.AddDays(1), Duration = TimeSpan.FromHours(1) };
            page.ShowBookings(new List<Booking> { first });
            var grid = Field<DataGridView>(page, "_gridBookings");
            grid.CurrentCell = grid.Rows[0].Cells[1];
            page.ShowBookings(new List<Booking>());
            Check(grid.CurrentCell is null, "The removed booking retained an action target.");
            page.ShowBookings(new List<Booking> { later });
            Check(grid.CurrentCell is null && grid.CurrentRow is null && grid.SelectedRows.Count == 0, "A booking arriving after an empty refresh became an unintended action target.");
            page.ShowBookings(new List<Booking> { later });
            Check(grid.CurrentCell is null && grid.CurrentRow is null, "A later refresh selected the new booking without user input.");
        });
    }

    private static void AutoFinishTests()
    {
        Test("Timer closes a session at its scheduled end and accounts for it once", () =>
        {
            var client = AddClient();
            var nextClient = AddClient("Другой клиент", "79009998877");
            AddRawComputer(1);
            AddRawComputer(2);
            var started = Sessions.StartSessionDirect(client.Id, 1, DateTime.Now, TimeSpan.FromMinutes(1));
            Check(started is not null, Sessions.LastMessage);
            var next = Sessions.StartSessionDirect(nextClient.Id, 2, DateTime.Now, TimeSpan.FromHours(1));
            Check(next is not null, Sessions.LastMessage);
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var grid = Field<DataGridView>(page, "_gridActiveSessions");
            Check(grid.Rows.Count == 2, "An unexpired session was closed before its scheduled end.");
            Sql($"""
                UPDATE Sessions
                SET StartTime = LOCALTIMESTAMP - INTERVAL '58 seconds',
                    PlannedEndTime = LOCALTIMESTAMP + INTERVAL '2 seconds'
                WHERE Id = {started!.Id}
                """);
            var plannedEnd = Sessions.GetActiveSessions().Single(session => session.Id == started.Id).PlannedEndTime;
            WaitUntil(() => grid.Rows.Count == 1, "The timer left the expired session active.");
            var completed = Sessions.GetCompletedSessionsBetweenDates(plannedEnd.AddMinutes(-1), plannedEnd.AddMinutes(1)).Single();
            Check(completed.Id == started!.Id && completed.EndTime == plannedEnd, "The saved end time differs from the scheduled end.");
            Check(completed.Duration == TimeSpan.FromMinutes(1) && completed.TotalCost == 2.50m, "Timer delay changed the billed duration or amount.");
            Check(Sessions.GetActiveSessions().Single().Id == next!.Id, "The later session was closed early.");
            Check(MapButton(page, 1).BackColor == Color.LightGreen && MapButton(page, 2).BackColor == Color.LightCoral, "The map does not reflect the completed and active sessions.");
            WaitForTicks(page, 2);
            Check(Sessions.CompleteSession(started.Id) is null, "Manual completion accepted an already closed session.");
            var after = Clients.GetClientById(client.Id)!;
            Check(Math.Abs(after.TotalHours - 1d / 60) < 0.000001 && after.TotalSpent == 2.50m, "The expired session was not accounted for exactly once.");
        });
        Test("Main window catches up on an expired session on the clients tab and preserves the booking", () =>
        {
            var client = AddClient();
            var bookingClient = AddClient("Клиент с бронью", "79009998877");
            AddRawComputer(1);
            var started = Sessions.StartSessionDirect(client.Id, 1, DateTime.Now.AddMinutes(-65), TimeSpan.FromHours(1));
            Check(started is not null, Sessions.LastMessage);
            var plannedEnd = Sessions.GetActiveSessions().Single().PlannedEndTime;
            Sql($"""
                INSERT INTO Bookings(ClientId, ComputerNumber, PlannedStart, DurationMinutes, Status, CreatedAt)
                SELECT {bookingClient.Id}, ComputerNumber, PlannedEndTime, 30, 'active', PlannedEndTime
                FROM Sessions WHERE Id = {started!.Id}
                """);
            using var main = new MainForm(Db, new User { Username = "manager", Role = "manager" })
            {
                StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000),
                ShowInTaskbar = false, Opacity = 0
            };
            main.Show();
            var tabs = Field<TabControl>(main, "_mainTabControl");
            Check(tabs.SelectedIndex == 0, "The test did not start on the clients tab.");
            var clientsPage = tabs.TabPages[0].Controls.OfType<ClientPage>().Single();
            var clientGrid = Field<DataGridView>(clientsPage, "_gridClients");
            var selected = clientGrid.Rows.Cast<DataGridViewRow>().Single(row => Convert.ToInt32(row.Cells[0].Value) == bookingClient.Id);
            clientGrid.CurrentCell = selected.Cells[1];
            WaitUntil(() => Sessions.GetActiveSessions().Count == 0, "The expired session was not closed on the clients tab.");
            Check(tabs.SelectedIndex == 0, "Automatic completion changed the active tab.");
            Check(Convert.ToInt32(clientGrid.CurrentRow?.Cells[0].Value) == bookingClient.Id, "Automatic completion changed the selected client.");
            var billedRow = clientGrid.Rows.Cast<DataGridViewRow>().Single(row => Convert.ToInt32(row.Cells[0].Value) == client.Id);
            Check(Convert.ToDecimal(billedRow.Cells["TotalSpent"].Value) == 150m, "Automatic completion did not refresh the visible client statistics.");
            var completed = Sessions.GetCompletedSessionsBetweenDates(plannedEnd.AddMinutes(-1), plannedEnd.AddMinutes(1)).Single();
            Check(completed.EndTime == plannedEnd && completed.TotalCost == 150m, "Catch-up charged for time after the scheduled end.");
            var booking = Sessions.GetActiveBookings().Single();
            Check(booking.ClientId == bookingClient.Id && booking.PlannedStart == plannedEnd, "Automatic completion changed the next booking.");
            main.SwitchToPage(1);
            Application.DoEvents();
            var page = tabs.TabPages[1].Controls.OfType<SessionPage>().Single();
            Check(MapButton(page, 1).BackColor == Color.LightCoral, "The map released a PC with an active booking.");
        });
        Test("Automatic completion rolls back on a statistics error and retries", () =>
        {
            var client = AddClient();
            AddRawComputer(1);
            var started = Sessions.StartSessionDirect(client.Id, 1, DateTime.Now.AddMinutes(-65), TimeSpan.FromHours(1));
            Check(started is not null, Sessions.LastMessage);
            Sql("ALTER TABLE Clients ADD CONSTRAINT no_test_spending CHECK (TotalSpent = 0)");
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            try
            {
                WaitForTicks(page, 1);
                Check(Sessions.GetActiveSessions().Single().Id == started!.Id, "A failed update still closed the session.");
                Check(Clients.GetClientById(client.Id)!.TotalSpent == 0, "A failed update changed client statistics.");
            }
            finally
            {
                Sql("ALTER TABLE Clients DROP CONSTRAINT no_test_spending");
            }
            WaitUntil(() => Sessions.GetActiveSessions().Count == 0, "The timer did not retry completion after the database became writable.");
            Check(Clients.GetClientById(client.Id)!.TotalSpent == 150m, "Retry did not save the scheduled amount once.");
        });
        Test("A locked client does not delay the timer or prevent other completions", () =>
        {
            var firstClient = AddClient();
            var secondClient = AddClient("Второй клиент", "79009998877");
            AddRawComputer(1);
            AddRawComputer(2);
            var blocked = Sessions.StartSessionDirect(firstClient.Id, 1, DateTime.Now.AddMinutes(-70), TimeSpan.FromHours(1));
            var other = Sessions.StartSessionDirect(secondClient.Id, 2, DateTime.Now.AddMinutes(-65), TimeSpan.FromHours(1));
            Check(blocked is not null && other is not null, Sessions.LastMessage);

            using var acquired = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            var blocker = Task.Run(() =>
            {
                using var connection = Db.CreateConnection();
                using var transaction = connection.BeginTransaction();
                using var command = new NpgsqlCommand("SELECT Id FROM Clients WHERE Id = @id FOR UPDATE", connection, transaction);
                command.Parameters.AddWithValue("id", firstClient.Id);
                command.ExecuteScalar();
                acquired.Set();
                release.Wait(TimeSpan.FromSeconds(5));
                transaction.Rollback();
            });
            using var page = new SessionPage(Clients, Sessions);
            using var host = Host(page);
            var timer = Field<System.Windows.Forms.Timer>(page, "_timer");
            try
            {
                Check(acquired.Wait(TimeSpan.FromSeconds(3)), "The test did not acquire the client lock.");
                var watch = Stopwatch.StartNew();
                // DoEvents drains pending ticks. Measure one tick independently of retries.
                EventHandler stopAfterTick = (_, _) => timer.Stop();
                timer.Tick += stopAfterTick;
                try { WaitForTicks(page, 1); }
                finally { timer.Tick -= stopAfterTick; }
                Check(watch.Elapsed < TimeSpan.FromSeconds(3), $"The timer waited for the locked client: {watch.ElapsedMilliseconds} ms.");
                Check(Sessions.GetActiveSessions().Single().Id == blocked!.Id, "A locked client prevented completion of a different session.");
                Check(Clients.GetClientById(secondClient.Id)!.TotalSpent == 150m, "The other client was not billed.");
            }
            finally
            {
                release.Set();
                blocker.GetAwaiter().GetResult();
            }
            timer.Start();
            WaitUntil(() => Sessions.GetActiveSessions().Count == 0, "Completion was not retried after the lock was released.");
            Check(Clients.GetClientById(firstClient.Id)!.TotalSpent == 150m, "The retried completion did not account for the session once.");
        });
    }

    private static Button MapButton(SessionPage page, int number) => Field<Panel>(page, "_computerMap").Controls.OfType<Button>().Single(button => (int)button.Tag! == number);

    private static void WaitForTicks(SessionPage page, int count)
    {
        var timer = Field<System.Windows.Forms.Timer>(page, "_timer");
        var ticks = 0;
        EventHandler handler = (_, _) => ticks++;
        timer.Tick += handler;
        try { WaitUntil(() => ticks >= count, "The session timer did not produce the required ticks."); }
        finally { timer.Tick -= handler; }
    }

    private static void WaitUntil(Func<bool> condition, string message)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            Application.DoEvents();
            Check(watch.Elapsed < TimeSpan.FromSeconds(8), message);
            Thread.Sleep(10);
        }
    }

    private static void CaptureScreens(string directory)
    {
        Directory.CreateDirectory(directory);
        Reset();
        var ivan = AddClient();
        var peter = AddClient("Петров Пётр", "79002223344");
        AddClient("Сидоров Сергей", "79003334455");
        for (var i = 1; i <= 12; i++) AddRawComputer(i);
        Sessions.StartSessionDirect(ivan.Id, 3, DateTime.Now.AddMinutes(-35), TimeSpan.FromMinutes(90));
        Sessions.CreateBooking(peter.Id, 6, DateTime.Today.AddDays(1).AddHours(18), TimeSpan.FromMinutes(120));
        var billableClient = Clients.GetAllClients().Single(c => c.FullName.StartsWith("Сидоров"));
        var paidSession = Sessions.StartSessionDirect(billableClient.Id, 2, DateTime.Now.AddHours(-2), TimeSpan.FromHours(2));
        Check(paidSession is not null, Sessions.LastMessage);
        Check(Sessions.CompleteSession(paidSession!.Id) is not null, Sessions.LastMessage);
        using var form = new MainForm(Db, new User { Username = "admin", Role = "admin" });
        form.WindowState = FormWindowState.Normal;
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-32000, -32000);
        form.ShowInTaskbar = false;
        form.Show();
        var tabs = Field<TabControl>(form, "_mainTabControl");
        foreach (TabPage tab in tabs.TabPages)
        {
            tabs.SelectedTab = tab;
            Application.DoEvents();
            if (tab.Controls[0] is ReportPage reports)
                Field<Button>(reports, "_btnGenerate").PerformClick();
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, form.ClientRectangle with { Width = form.Width, Height = form.Height });
            var filename = tab.Text switch { "Клиенты" => "clients", "Сеансы" => "sessions", "Отчёты" => "reports", _ => "settings" };
            bitmap.Save(Path.Combine(directory, filename + ".png"));
        }
        Console.WriteLine("Screenshots saved to " + Path.GetFullPath(directory));
    }
}
