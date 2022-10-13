global using Amazon.S3;
global using Amazon.S3.Transfer;
global using Oakton;
global using System.Reflection;

var executor = CommandExecutor.For(_ =>
{
    _.RegisterCommands(typeof(Program).GetTypeInfo().Assembly);
});

return executor.Execute(args);
