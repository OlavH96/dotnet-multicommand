#load "nuget:Dotnet.Build, 0.26.0"
#load "nuget:dotnet-steps, 0.0.2"

BuildContext.CodeCoverageThreshold = 80;

[StepDescription("Runs the tests with test coverage")]
Step TestWithCodeCoverage = () => DotNet.TestWithCodeCoverage();

[StepDescription("Runs all the tests")]
Step Test = () => DotNet.Test();

[StepDescription("Creates the NuGet packages")]
Step Pack = () =>
{
    Test();
    TestWithCodeCoverage();
    DotNet.Pack();
};

[DefaultStep]
[StepDescription("Deploys packages if we are on a tag commit in a secure environment.")]
AsyncStep Deploy = async () =>
{
    Pack();
    //await Artifacts.Deploy();
};

await StepRunner.Execute(Args);
return 0;
