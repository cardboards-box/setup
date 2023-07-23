using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CardboardBox.Setup.CliParser;

/// <summary>
/// An builder for adding command line verbs
/// </summary>
public interface ICommandLineBuilder
{
	/// <summary>
	/// A collection of all of the verbs registered
	/// </summary>
	IReadOnlyCollection<CommandLineBuilder.CommandVerb> Verbs { get; }

	/// <summary>
	/// The exit code to return when the process has executed successfully
	/// </summary>
	int ExitCodeSuccess { get; }

	/// <summary>
	/// The exit code to return when the process has failed to execute
	/// </summary>
	int ExitCodeFailure { get; }

	/// <summary>
	/// The cancellation token to use for the execution of the verb
	/// </summary>
	CancellationToken? Token { get; }

	/// <summary>
	/// Adds the given verb handler to the system
	/// </summary>
	/// <typeparam name="T">The class representing the verb handler</typeparam>
	/// <returns>The current instance of the builder for fluent chaining</returns>
	/// <exception cref="ArgumentNullException">Thrown if the class doesn't implement the <see cref="IVerb{TOptions}"/> interface</exception>
	ICommandLineBuilder Add<T>() where T : class;

    /// <summary>
    /// Adds the given verb handler to the system
    /// </summary>
    /// <typeparam name="T">The class representing the verb handler</typeparam>
    /// <typeparam name="TOpt">The class representing the options for the verb</typeparam>
    /// <returns>The current instance of the builder for fluent chaining</returns>
    ICommandLineBuilder Add<T, TOpt>() where T : class, IVerb<TOpt> where TOpt : class;

	/// <summary>
	/// Sets the expected exit codes for successes and failures
	/// </summary>
	/// <param name="success">The exit code for a successful execution (defaults to 0)</param>
	/// <param name="failure">The exit code for a failure (defaults to 1)</param>
	/// <returns>The current instance of the builder for fluent chaining</returns>
	ICommandLineBuilder ExitCode(int success = 0, int failure = 1);

	/// <summary>
	/// Sets the cancellation token to use for the execution of the verb
	/// </summary>
	/// <param name="token">The cancellation token</param>
	/// <returns>The current instance of the builder for fluent chaining</returns>
	ICommandLineBuilder CancelToken(CancellationToken? token = null);
}

/// <summary>
/// The implementation of the <see cref="ICommandLineBuilder"/>
/// </summary>
public class CommandLineBuilder : ICommandLineBuilder
{
	private readonly IServiceCollection _services;
	private readonly List<CommandVerb> _verbs = new();

	/// <summary>
	/// The exit code to return when the process has executed successfully
	/// </summary>
	public int ExitCodeSuccess { get; set; } = 0;

	/// <summary>
	/// The exit code to return when the process has failed to execute
	/// </summary>
	public int ExitCodeFailure { get; set; } = 1;

    /// <summary>
    /// The cancellation token to use for the execution of the verb
    /// </summary>
    public CancellationToken? Token { get; set; }

    /// <summary>
    /// A collection of all of the verbs registered
    /// </summary>
    public IReadOnlyCollection<CommandVerb> Verbs => _verbs.AsReadOnly();

    /// <summary>
    /// The implementation of the <see cref="ICommandLineBuilder"/>
    /// </summary>
    /// <param name="services">The service collection to use</param>
    /// <exception cref="ArgumentNullException">Thrown if the service collection is null</exception>
    public CommandLineBuilder(IServiceCollection services) 
	{
		_services = services ?? throw new ArgumentNullException(nameof(services));
	}

    /// <summary>
    /// Adds the given verb handler to the system
    /// </summary>
    /// <typeparam name="T">The class representing the verb handler</typeparam>
    /// <returns>The current instance of the builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown if the class doesn't implement the <see cref="IVerb{TOptions}"/> interface</exception>
    public ICommandLineBuilder Add<T>() where T : class
    {
        var type = typeof(T);
        var genArgs = type.BaseType.GetGenericArguments();
        if (genArgs.Length == 0)
            throw new ArgumentNullException(nameof(T), "The type must implement IVerb<TOptions>");

        var opts = genArgs.FirstOrDefault(t => t.GetCustomAttribute<VerbAttribute>() != null)
            ?? throw new ArgumentNullException(nameof(T), "The options type requires the [Verb] attribute.");

		var service = typeof(IVerb<>).MakeGenericType(opts);
        if (!service.IsAssignableFrom(type))
			throw new InvalidOperationException("Type needs to implement IVerb<TOptions>");

		_verbs.Add(new CommandVerb(opts, service));
		_services.AddTransient(service, type);
		return this;
    }

	/// <summary>
	/// Adds the given verb handler to the system
	/// </summary>
	/// <typeparam name="T">The class representing the verb handler</typeparam>
	/// <typeparam name="TOpt">The class representing the options for the verb</typeparam>
	/// <returns>The current instance of the builder for fluent chaining</returns>
	public ICommandLineBuilder Add<T, TOpt>() where T: class, IVerb<TOpt> where TOpt: class
	{
		_verbs.Add(new CommandVerb(typeof(TOpt), typeof(IVerb<TOpt>)));
		_services.AddTransient<IVerb<TOpt>, T>();
		return this;
	}

	/// <summary>
	/// Sets the expected exit codes for successes and failures
	/// </summary>
	/// <param name="success">The exit code for a successful execution (defaults to 0)</param>
	/// <param name="failure">The exit code for a failure (defaults to 1)</param>
	/// <returns>The current instance of the builder for fluent chaining</returns>
	public ICommandLineBuilder ExitCode(int success = 0, int failure = 1)
	{
		ExitCodeSuccess = success;
		ExitCodeFailure = failure;
		return this;
	}

	/// <summary>
	/// Sets the cancellation token to use for the execution of the verb
	/// </summary>
	/// <param name="token">The cancellation token</param>
	/// <returns>The current instance of the builder for fluent chaining</returns>
	public ICommandLineBuilder CancelToken(CancellationToken? token = null)
	{
        Token = token;
        return this;
    }

	/// <summary>
	/// Represents a verb and its options
	/// </summary>
	/// <param name="Options">The type that represents the options</param>
	/// <param name="VerbService">The type that represents the verb handler</param>
	public record class CommandVerb(Type Options, Type VerbService);
}
