using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace OpenBFME.Launcher;

public sealed record ImportProgress(string Phase, string Message, double? Percent);

public sealed class ImporterRunner
{
    public async Task<int> RunAsync(
        string launcherDirectory,
        string game,
        string retailPath,
        string stateRoot,
        IProgress<ImportProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (game is not ("bfme2" or "rotwk")) throw new ArgumentOutOfRangeException(nameof(game));
        BundleInventory.Verify(launcherDirectory);
        var script = Path.GetFullPath(Path.Combine(launcherDirectory, "tools", "openbfme_import.py"));
        if (!File.Exists(script)) throw new FileNotFoundException("Bundled importer entry point is missing.", script);
        var retail = Path.GetFullPath(retailPath);
        var state = Path.GetFullPath(stateRoot);
        var privateRoot = Directory.GetParent(state)
            ?? throw new InvalidOperationException("Importer state root has no parent.");
        var contentRoot = Path.Combine(privateRoot.FullName, "content-packs");
        var baseProfile = Path.GetFullPath(Path.Combine(
            launcherDirectory, "importer", "profiles", "men-fords-v1.json"));
        if (!Directory.Exists(retail)) throw new DirectoryNotFoundException("Retail installation is missing.");
        if (!File.Exists(baseProfile)) throw new FileNotFoundException("Bundled base import profile is missing.", baseProfile);
        Directory.CreateDirectory(state);

        progress?.Report(new ImportProgress("Tools", "Verifying pinned conversion tools.", null));
        var bootstrap = NewProcess(launcherDirectory);
        foreach (var argument in new[] { script, "--state-root", state, "--json", "bootstrap-tools" })
            bootstrap.ArgumentList.Add(argument);
        using (var bootstrapProcess = new Process { StartInfo = bootstrap, EnableRaisingEvents = true })
        {
            bootstrapProcess.Start();
            var bootstrapOut = PumpAsync(bootstrapProcess.StandardOutput, progress, cancellationToken);
            var bootstrapErr = PumpErrorsAsync(bootstrapProcess.StandardError, progress, cancellationToken);
            await bootstrapProcess.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(bootstrapOut, bootstrapErr);
            if (bootstrapProcess.ExitCode != 0) return bootstrapProcess.ExitCode;
        }

        var start = NewProcess(launcherDirectory);
        var firstCommand = game == "bfme2"
            ? new[]
            {
                script, "--state-root", state, "--json", "build",
                "--install", retail, "--game", game, "--profile", "men-fords-v0",
                "--godot-content-root", contentRoot
            }
            : new[]
            {
                script, "--state-root", state, "--json", "import-faction",
                "--install", retail, "--game", game, "--faction", "angmar", "--convert"
            };
        foreach (var argument in firstCommand)
            start.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = start, EnableRaisingEvents = true };
        process.Start();
        var stdout = PumpAsync(process.StandardOutput, progress, cancellationToken);
        var stderr = PumpErrorsAsync(process.StandardError, progress, cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        await Task.WhenAll(stdout, stderr);
        if (process.ExitCode != 0) return process.ExitCode;
        if (game == "bfme2") return 0;

        progress?.Report(new ImportProgress("Packaging", "Building and selecting the local content pack.", null));
        var publish = NewProcess(launcherDirectory);
        foreach (var argument in new[]
        {
            script, "--state-root", state, "--json", "publish-faction-to-slice",
            "--install", retail, "--game", game, "--faction", game == "rotwk" ? "angmar" : "men",
            "--base-profile", baseProfile, "--godot-content-root", contentRoot
        })
            publish.ArgumentList.Add(argument);
        using var publishProcess = new Process { StartInfo = publish, EnableRaisingEvents = true };
        publishProcess.Start();
        var publishOut = PumpAsync(publishProcess.StandardOutput, progress, cancellationToken);
        var publishErr = PumpErrorsAsync(publishProcess.StandardError, progress, cancellationToken);
        await publishProcess.WaitForExitAsync(cancellationToken);
        await Task.WhenAll(publishOut, publishErr);
        return publishProcess.ExitCode;
    }

    private static string ResolvePython(string launcherDirectory)
    {
        var bundled = Path.Combine(launcherDirectory, "python", "python.exe");
        if (!File.Exists(bundled))
            throw new FileNotFoundException("Bundled pinned Python runtime is missing.", bundled);
        return bundled;
    }

    private static ProcessStartInfo NewProcess(string launcherDirectory)
    {
        var start = new ProcessStartInfo
        {
            FileName = ResolvePython(launcherDirectory),
            WorkingDirectory = launcherDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }.WithImporterEnvironment();
        start.ArgumentList.Add("-X");
        start.ArgumentList.Add("utf8");
        start.ArgumentList.Add("-B");
        start.ArgumentList.Add("-I");
        start.ArgumentList.Add("-S");
        return start;
    }

    private static async Task PumpAsync(
        StreamReader reader,
        IProgress<ImportProgress>? progress,
        CancellationToken token)
    {
        while (await reader.ReadLineAsync(token) is { } line)
        {
            var parsed = ParseProgressLine(line);
            progress?.Report(parsed);
        }
    }

    internal static ImportProgress ParseProgressLine(string line)
    {
        if (line.Length > 64 * 1024)
            throw new InvalidDataException("Importer progress record is too large.");
        try
        {
            using var json = JsonDocument.Parse(line);
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return new ImportProgress("Importing", Redact(line), null);
            var phase = root.TryGetProperty("stage", out var p)
                ? p.GetString() ?? "Importing"
                : "Importing";
            var message = root.TryGetProperty("detail", out var m)
                ? m.GetString() ?? phase
                : phase;
            double? percent = root.TryGetProperty("fraction", out var n)
                && n.TryGetDouble(out var value)
                ? Math.Clamp(value * 100, 0, 100)
                : null;
            return new ImportProgress(phase, Redact(message), percent);
        }
        catch (JsonException)
        {
            return new ImportProgress("Importing", Redact(line), null);
        }
    }

    private static async Task PumpErrorsAsync(
        StreamReader reader,
        IProgress<ImportProgress>? progress,
        CancellationToken token)
    {
        while (await reader.ReadLineAsync(token) is { } line)
        {
            var message = Redact(line);
            if (progress is null)
                Console.Error.WriteLine(message);
            else
                progress.Report(new ImportProgress("Importer", message, null));
        }
    }

    internal static string Redact(string text)
    {
        var result = text;
        foreach (var prefix in new[] { @"[A-Za-z]:\\", @"\\\\[^\\\s]+\\[^\\\s]+\\" })
            result = System.Text.RegularExpressions.Regex.Replace(result,
                prefix + @"[^\r\n""]+", "<private-path>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return result.Length <= 400 ? result : result[..400] + "…";
    }
}

internal static class ProcessStartInfoExtensions
{
    internal static ProcessStartInfo WithImporterEnvironment(this ProcessStartInfo start)
    {
        start.Environment["PYTHONUTF8"] = "1";
        start.Environment["PYTHONDONTWRITEBYTECODE"] = "1";
        return start;
    }
}
