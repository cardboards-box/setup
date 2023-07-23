using Microsoft.Extensions.DependencyInjection;

namespace CardboardBox;

using Setup.CliParser;

/// <summary>
/// Extensions for adding verb handlers to dependency injection
/// </summary>
public static class CliExtensions
{
    /// <summary>
    /// Watches <see cref="Console.CancelKeyPress"/> and <see cref="AppDomain.ProcessExit"/> and returns a cancellation token when either is triggered
    /// </summary>
    /// <returns>The cancellation token</returns>
    public static CancellationToken TokenFromSigterm()
    {
        var token = new CancellationTokenSource();

        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            if (!token.IsCancellationRequested)
                token.Cancel();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            if (!token.IsCancellationRequested)
                token.Cancel();
        };

        return token.Token;
    }

    /// <summary>
    /// Triggers the handling of command line verbs against the given service collection and command line arguments
    /// </summary>
    /// <param name="services">The service collection to use for dependency injection</param>
    /// <param name="args">The command line arguments to use</param>
    /// <param name="bob">The command line parser configuration builder</param>
    /// <param name="token">A cancellation token for the verb. If not specified, the token will be sourced from <see cref="TokenFromSigterm"/></param>
    /// <returns>The exit code returned by the executed verb</returns>
    public static Task<int> Cli(this IServiceCollection services, string[] args, Action<ICommandLineBuilder> bob, CancellationToken? token = null)
	{
        var builder = new CommandLineBuilder(services)
        {
            Token = token ?? TokenFromSigterm()
        };
        bob?.Invoke(builder);

		var provider = services
			.AddSingleton<ICommandLineBuilder>(builder)
			.AddTransient<ICommandLineService, CommandLineService>()
			.BuildServiceProvider();

		services.AddSingleton(provider);

		var srv = provider.GetRequiredService<ICommandLineService>();
		return srv.Run(args);
	}

    /// <summary>
    /// Triggers the handling of command line verbs against the given service collection and command line arguments
    /// </summary>
    /// <param name="services">The service collection to use for dependency injection</param>
    /// <param name="bob">The command line parser configuration builder</param>
    /// <param name="skipFirst">Whether or not to skip the first argument as it is usually the current executables path</param>
    /// <param name="token">A cancellation token for the verb</param>
    /// <returns>The exit code returned by the executed verb</returns>
    public static Task<int> Cli(this IServiceCollection services, Action<ICommandLineBuilder> bob, bool skipFirst = true, CancellationToken? token = null)
	{
		var args = Environment.GetCommandLineArgs();

		if (skipFirst)
			args = args.Skip(1).ToArray();
		return services.Cli(args, bob, token);
	}
}
