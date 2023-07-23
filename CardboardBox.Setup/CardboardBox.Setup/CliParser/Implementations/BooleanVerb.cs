using Microsoft.Extensions.Logging;

namespace CardboardBox;

/// <summary>
/// Represents a verb that can be executed from the command line, and handles exceptions
/// </summary>
/// <typeparam name="TOptions">The type of class that represents the options for this verb</typeparam>
public abstract class BooleanVerb<TOptions> : IVerb<TOptions> where TOptions : class
{
    /// <summary>
    /// The service that handles logging
    /// </summary>
    public readonly ILogger _logger;

    /// <summary>
    /// The exit code returned when the command fails or the <see cref="Execute(TOptions, CancellationToken)"/> method returns false
    /// </summary>
    public virtual int ExitCodeFailure => 1;
    /// <summary>
    /// The exit code returned when the <see cref="Execute(TOptions, CancellationToken)"/> method returns true
    /// </summary>
    public virtual int ExitCodeSuccess => 0;

    /// <summary>
    /// The name of the verb
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// Represents a verb that can be executed from the command line
    /// </summary>
    /// <param name="logger">The service that handles logging</param>
    public BooleanVerb(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executed when the command is run
    /// </summary>
    /// <param name="options">The command line options</param>
	/// <param name="token">A cancellation token that represents when a sigterm is received</param>
    /// <returns>Whether or not the execution was successful</returns>
    public abstract Task<bool> Execute(TOptions options, CancellationToken token);

    /// <summary>
    /// Executed when the command is run
    /// </summary>
    /// <param name="options">The command line argument options</param>
	/// <param name="token">A cancellation token that represents when a sigterm is received</param>
    /// <returns>The exit code</returns>
    public virtual async Task<int> Run(TOptions options, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Starting execute of {Name} with options: {options}", Name, options);
            var result = await Execute(options, token);
            _logger.LogInformation("Finished execute of {Name} with result: {result}", Name, result);
            return result ? ExitCodeSuccess : ExitCodeFailure;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while running {Name}", Name);
            return ExitCodeFailure;
        }
    }
}
