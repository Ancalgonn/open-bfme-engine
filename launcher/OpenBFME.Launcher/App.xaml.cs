namespace OpenBFME.Launcher;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        LauncherOptions options;
        try { options = LauncherOptions.Parse(e.Args); }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.Message);
            Shutdown(2);
            return;
        }
        try
        {
            if (LauncherSelfUpdate.RelaunchSelected(options, e.Args))
            {
                Shutdown(0);
                return;
            }
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Selected launcher update is invalid: {error.Message}");
            Shutdown(1);
            return;
        }
        if (!options.Headless)
        {
            new MainWindow().Show();
            return;
        }

        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        try
        {
            var service = new LauncherService(options);
            if (options.ImportGame is not null && options.RetailPath is not null)
            {
                var state = Path.Combine(options.InstallRoot, ".private", "retail-work");
                var exit = await service.Importer.RunAsync(
                    AppContext.BaseDirectory,
                    options.ImportGame,
                    options.RetailPath,
                    state,
                    null,
                    CancellationToken.None);
                if (exit != 0) throw new InvalidOperationException($"Importer exited with code {exit}.");
            }
            else if (!options.NoUpdate && options.ManifestUri is not null)
            {
                var manifest = await service.Installer.FetchManifestAsync(
                    options.ManifestUri, CancellationToken.None);
                await service.Installer.InstallAsync(
                    manifest, options.InstallRoot, null, CancellationToken.None,
                    expectedChannel: options.Channel);
            }
            if (options.VerifyOnly)
            {
                var current = service.Current
                    ?? throw new InvalidOperationException("No installed release is selected.");
                ReleaseInstaller.VerifyInstalledVersion(Path.Combine(
                    options.InstallRoot, "versions", current.CurrentVersion),
                    current.CurrentVersion, current.Commit);
            }
            Shutdown(0);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.Message);
            Shutdown(1);
        }
    }
}
