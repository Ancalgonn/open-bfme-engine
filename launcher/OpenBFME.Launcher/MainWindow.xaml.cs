using System.Windows;

namespace OpenBFME.Launcher;

public partial class MainWindow : Window
{
    private readonly LauncherService _service;
    private CancellationTokenSource? _operation;

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            var options = LauncherOptions.Parse(Environment.GetCommandLineArgs().Skip(1));
            _service = new LauncherService(options);
            ChannelText.Text = $"Channel: {options.Channel}";
            RefreshState();
            Loaded += async (_, _) => await RunStartupFlagsAsync();
        }
        catch (Exception error)
        {
            MessageBox.Show(error.Message, "OpenBFME launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            _service = new LauncherService(LauncherOptions.Parse(Array.Empty<string>()));
        }
    }

    private void RefreshState()
    {
        var state = _service.Current;
        VersionText.Text = state is null ? "Not installed" : $"Version {state.CurrentVersion}";
        PlayButton.IsEnabled = state is not null;
    }

    private async Task RunStartupFlagsAsync()
    {
        if (_service.Options.ImportGame is not null && _service.Options.RetailPath is not null)
            await ImportAsync(_service.Options.ImportGame, _service.Options.RetailPath);
        else if (!_service.Options.NoUpdate && _service.Options.ManifestUri is not null)
            await UpdateAsync();
        if (_service.Options.VerifyOnly && _service.Current is not null)
        {
            ReleaseInstaller.VerifyInstalledVersion(Path.GetDirectoryName(_service.CurrentGamePath())!);
            StatusText.Text = "Installed version verified.";
        }
    }

    private async void Update_Click(object sender, RoutedEventArgs e) => await UpdateAsync();

    private async Task UpdateAsync()
    {
        if (_service.Options.ManifestUri is null)
        {
            StatusText.Text = "No release feed is configured. Use --manifest-url with the GitHub release manifest.";
            return;
        }
        await RunOperationAsync(async token =>
        {
            StatusText.Text = "Checking release manifest…";
            var manifest = await _service.Installer.FetchManifestAsync(_service.Options.ManifestUri, token);
            var progress = new Progress<TransferProgress>(item =>
            {
                Progress.Value = item.Percent;
                StatusText.Text = $"{item.Phase} — {item.Percent:0}%";
            });
            await _service.Installer.InstallAsync(
                manifest, _service.Options.InstallRoot, progress, token,
                expectedChannel: _service.Options.Channel);
            StatusText.Text = $"OpenBFME {manifest.Version} is ready.";
            RefreshState();
        });
    }

    private async void ImportBfme_Click(object sender, RoutedEventArgs e) =>
        await ImportAsync("bfme2", BfmePath.Text);

    private async void ImportRotwk_Click(object sender, RoutedEventArgs e) =>
        await ImportAsync("rotwk", RotwkPath.Text);

    private async Task ImportAsync(string game, string path)
    {
        await RunOperationAsync(async token =>
        {
            var progress = new Progress<ImportProgress>(item =>
            {
                if (item.Percent is double value) Progress.Value = value;
                StatusText.Text = $"{item.Phase}: {item.Message}";
            });
            var stateRoot = Path.Combine(_service.Options.InstallRoot, ".private", "retail-work");
            var code = await _service.Importer.RunAsync(AppContext.BaseDirectory, game, path,
                stateRoot, progress, token);
            if (code != 0) throw new InvalidOperationException($"Importer exited with code {code}.");
            Progress.Value = 100;
            StatusText.Text = game == "bfme2" ? "BFME II assets imported." : "RotWK Angmar assets imported.";
        });
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _service.LaunchGame();
            StatusText.Text = "OpenBFME started.";
        }
        catch (Exception error) { ShowError(error); }
    }

    private void Rollback_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var state = ReleaseInstaller.Rollback(_service.Options.InstallRoot);
            StatusText.Text = $"Rolled back to {state.CurrentVersion}.";
            RefreshState();
        }
        catch (Exception error) { ShowError(error); }
    }

    private async Task RunOperationAsync(Func<CancellationToken, Task> operation)
    {
        if (_operation is not null) return;
        _operation = new CancellationTokenSource();
        IsEnabled = false;
        try { await operation(_operation.Token); }
        catch (OperationCanceledException) { StatusText.Text = "Operation cancelled."; }
        catch (Exception error) { ShowError(error); }
        finally { IsEnabled = true; _operation.Dispose(); _operation = null; }
    }

    private void ShowError(Exception error)
    {
        StatusText.Text = error.Message;
        MessageBox.Show(error.Message, "OpenBFME launcher", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
