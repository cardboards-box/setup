namespace CardboardBox;

/// <summary>
/// Represents a verb that can be executed from the command line
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public interface IVerb<TOptions> where TOptions : class
{
	/// <summary>
	/// Executed when the command is run
	/// </summary>
	/// <param name="options">The command line argument options</param>
	/// <param name="token">A cancellation token that represents when a sigterm is received</param>
	/// <returns>The exit code</returns>
	Task<int> Run(TOptions options, CancellationToken token);
}
