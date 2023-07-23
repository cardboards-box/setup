using CardboardBox;
using CardboardBox.Setup.TestCli.Verbs;
using Microsoft.Extensions.DependencyInjection;

return await new ServiceCollection()
    .AddSerilog()
    .Cli(args, c =>
    {
        c.Add<SigtermTestVerb>();
    });
