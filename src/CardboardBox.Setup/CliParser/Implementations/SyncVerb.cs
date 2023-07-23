namespace CardboardBox;

/// <summary>
/// Represents a synchronously executed command verb
/// </summary>
/// <typeparam name="TOptions">The type of class that represents the options for this verb</typeparam>
public abstract class SyncVerb<TOptions> : IVerb<TOptions> where TOptions : class
{
    /// <summary>
    /// Executed when the command is run
    /// </summary>
    /// <param name="options">The command line argument options</param>
    /// <param name="token">A cancellation token that represents when a sigterm is received</param>
    /// <returns>The exit code</returns>
    public virtual Task<int> Run(TOptions options, CancellationToken token)
	{
		var results = RunSync(options);
		return Task.FromResult(results);
	}

	/// <summary>
	/// Executed when the command is run
	/// </summary>
	/// <param name="options">The command line argument options</param>
	/// <returns>The exit code</returns>
	public abstract int RunSync(TOptions options);
}
