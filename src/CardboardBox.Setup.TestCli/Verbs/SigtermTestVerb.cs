using CommandLine;
using Microsoft.Extensions.Logging;

namespace CardboardBox.Setup.TestCli.Verbs;

[Verb("sigterm")]
public class SigtermTestVerbOptions
{
    [Option('m', "max-timeout", Default = 10, HelpText = "The maximum amount of time (in seconds) to wait before exiting")]
    public int MaxTimeout { get; set; } = 10;
}

public class SigtermTestVerb : BooleanVerb<SigtermTestVerbOptions>
{
    public SigtermTestVerb(ILogger<SigtermTestVerb> logger) : base(logger) { }

    public override async Task<bool> Execute(SigtermTestVerbOptions options, CancellationToken token)
    {
        try
        {
            var timeout = options.MaxTimeout * 1000;
            _logger.LogInformation("Waiting for sigterm or for {timeout}ms", timeout);
            await Task.Delay(timeout, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while waiting for sigterm - Operation was probably cancelled");
        }

        _logger.LogInformation("Finished. Was cancelled: {cancelled}", token.IsCancellationRequested);
        return true;
    }
}
