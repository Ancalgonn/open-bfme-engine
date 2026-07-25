namespace OpenBFME.Launcher;

public sealed record LauncherOptions(
    string Channel,
    Uri? ManifestUri,
    string InstallRoot,
    bool NoUpdate,
    bool VerifyOnly,
    bool Headless,
    string? ImportGame,
    string? RetailPath)
{
    public static LauncherOptions Parse(IEnumerable<string> arguments)
    {
        var args = arguments.ToArray();
        string Value(string flag, string fallback)
        {
            var index = Array.IndexOf(args, flag);
            if (index < 0) return fallback;
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"{flag} requires a value.");
            return args[index + 1];
        }

        bool Has(string flag) => Array.IndexOf(args, flag) >= 0;
        var channel = Value("--channel", "stable").Trim().ToLowerInvariant();
        if (channel is not ("stable" or "playtest" or "nightly"))
            throw new ArgumentException("--channel must be stable, playtest, or nightly.");

        var manifestText = Value("--manifest-url",
            channel == "stable"
                ? "https://github.com/Ancalgonn/open-bfme-engine/releases/latest/download/release-manifest.json"
                : "");
        Uri? manifestUri = null;
        if (manifestText.Length > 0)
        {
            if (!Uri.TryCreate(manifestText, UriKind.Absolute, out manifestUri))
                throw new ArgumentException("--manifest-url must be an absolute URL.");
            ReleaseUriPolicy.Validate(manifestUri);
        }

        var bfme2 = Has("--import-bfme2");
        var rotwk = Has("--import-rotwk");
        if (bfme2 && rotwk) throw new ArgumentException("Select only one import game per run.");
        var game = bfme2 ? "bfme2" : rotwk ? "rotwk" : null;
        var retail = game == "bfme2" ? Value("--bfme2-path", "") :
            game == "rotwk" ? Value("--rotwk-path", "") : "";
        if (game is not null && retail.Length == 0)
            throw new ArgumentException($"--import-{game} requires its retail path flag.");

        var root = Path.GetFullPath(Value(
            "--install-root",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenBFME")));
        return new LauncherOptions(channel, manifestUri, root, Has("--no-update"),
            Has("--verify-only"), Has("--headless"), game,
            retail.Length == 0 ? null : Path.GetFullPath(retail));
    }
}

public static class ReleaseUriPolicy
{
    private const string RepositoryReleasePrefix =
        "/Ancalgonn/open-bfme-engine/releases/";
    private static readonly HashSet<string> RedirectHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "objects.githubusercontent.com",
        "github-releases.githubusercontent.com",
        "release-assets.githubusercontent.com"
    };

    public static void Validate(Uri uri)
    {
        ValidateCommon(uri);
        if (!uri.DnsSafeHost.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.StartsWith(RepositoryReleasePrefix, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(uri.Query))
            throw new InvalidOperationException("Release URL does not belong to the approved repository.");
    }

    public static void ValidateResponse(Uri uri)
    {
        ValidateCommon(uri);
        if (uri.DnsSafeHost.Equals("github.com", StringComparison.OrdinalIgnoreCase))
        {
            Validate(uri);
            return;
        }
        if (!RedirectHosts.Contains(uri.DnsSafeHost))
            throw new InvalidOperationException("Release redirect host is not approved.");
    }

    private static void ValidateCommon(Uri uri)
    {
        if (uri.Scheme != Uri.UriSchemeHttps || uri.UserInfo.Length != 0 ||
            !string.IsNullOrEmpty(uri.Fragment))
            throw new InvalidOperationException("Release URLs must use credential-free HTTPS.");
    }
}
