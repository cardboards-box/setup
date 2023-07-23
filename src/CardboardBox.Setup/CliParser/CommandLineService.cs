using CommandLine;
using Microsoft.Extensions.Logging;

namespace CardboardBox.Setup.CliParser;

/// <summary>
/// A service that handles running the command line verbs
/// </summary>
public interface ICommandLineService
{
    /// <summary>
    /// Executes the given command line args
    /// </summary>
    /// <param name="args">The command line arguments</param>
    /// <returns>The exit code of the executed command</returns>
    Task<int> Run(string[] args);
}

/// <summary>
/// The implementation of the <see cref="ICommandLineService"/>
/// </summary>
public class CommandLineService : ICommandLineService
{
	private readonly IServiceProvider _services;
	private readonly ICommandLineBuilder _builder;
	private readonly ILogger _logger;

    /// <summary>
    /// The implementation of the <see cref="ICommandLineService"/>
    /// </summary>
    /// <param name="services">The provider for fetching services</param>
    /// <param name="builder">The command line builder service</param>
    /// <param name="logger">The service that handles logging</param>
    public CommandLineService(
		IServiceProvider services,
		ICommandLineBuilder builder,
		ILogger<CommandLineService> logger)
	{
		_services = services;
		_builder = builder;
		_logger = logger;
	}

    /// <summary>
    /// Executes the given command line args
    /// </summary>
    /// <param name="args">The command line arguments</param>
    /// <returns>The exit code of the executed command</returns>
    public async Task<int> Run(string[] args)
	{
		try
		{
			return await RunWithArgs(args);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while running application");
			throw;
		}
	}

	/// <summary>
	/// Helper to handle configuration failures
	/// </summary>
	/// <param name="message">The message to log</param>
	/// <param name="args">The arguments for the message</param>
	/// <returns>The failure exit code</returns>
	public int Fail(string message, params object[] args)
	{
		_logger.LogWarning(message, args);
		return _builder.ExitCodeFailure;
	}

    /// <summary>
    /// Executes the given command
    /// </summary>
    /// <param name="args">The command line arguments</param>
    /// <returns>The exit code of the executed command</returns>
    public async Task<int> RunWithArgs(string[] args)
	{
		var verbs = _builder.Verbs.Select(t => t.Options).ToArray();

		var cli = Parser.Default.ParseArguments(args, verbs);
		if (cli.Tag == ParserResultType.NotParsed) 
			return Fail("Could not parse command line arguments (did you --help?)");

		var verbType = _builder.Verbs.FirstOrDefault(t => t.Options == cli.TypeInfo.Current);
		if (verbType == null) 
			return Fail("Could not deteremine verb type for: {0}", cli.TypeInfo.Current.Name);

		var service = _services.GetService(verbType.VerbService);
		if (service == null) 
			return Fail("Could not determine verb service for: {0}", verbType.VerbService.Name);

		var method = service
			.GetType()
			.GetMethod("Run",
				new Type[] { verbType.Options, typeof(CancellationToken) });

		if (method == null) 
			return Fail("Could not find Run method on verb service for: {0}", verbType.VerbService.Name);

		if (method.ReturnType != typeof(Task<int>)) 
			return Fail("Run method does not return a Task<int> for: {0}", verbType.VerbService.Name);

		var exe = method.Invoke(service, new[] { cli.Value, _builder.Token ?? CancellationToken.None });
		if (exe is not Task<int> execute) 
			return Fail("Could not cast Run method result to Task<int> for: {0}", verbType.VerbService.Name);

		return await execute;
	}
}
