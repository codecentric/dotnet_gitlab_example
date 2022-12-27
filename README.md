# Dotnet with Gitlab

While Dotnet integrates really well with Github, it is more or less a second class citizen in Gitlab.

In this example I want to show how to get important CI/CD features running:

1. See Unit Tests from Gitlab
2. Code Coverage
3. Surface Issues and Code Style in Gitlab
4. Enforce Code Styles
5. How to build it using CI/CD.
6. Installers, for the brave.

Please note that this Pipeline is build for Linux, but you can get it to run on windows, too - it's mostly just a bit of different syntax.

## Building

Any non-trivial Dotnet application likely has some unit tests and one or more other packages. We want to:
- test it
- build it
- create an installer (later...)

See gitlab-ci.yml for the gory details.

Some of the challenges:

### Private nuget repos

If you need to reference private nuget repos hosted on the same gitlab instance you can look at ci-nuget.config on how to reference and install the packages.
Please note that you will have to adjust group ids etc. to match your project.

### Dotnet Tools

We need various tools (Roslynator, coverage etc.) to get the information we need.
In your local environment, installing them with `dotnet tool install --global CodeQualityToGitlab --version 0.1.1` is ok. But in CI global tools are problematic. 
Fortunately you can install them locally using Tool manifests. See `.config/dotnet-tools.json` for the installed tools. You can restore them locally using `dotnet tool restore` and run them with `dotnet tool run <name>`.

### Enforcing common Analysis levels and warning files

Dotnet offers to surface issues and warnings through Code Analysis. You can configure that in each project separately but that gets problematic and inconsistent if you have more than a handful of projects.
You can apply the relevant settings to ALL projects using `Directory.Build.props` . It looks like this:


```xml
<Project>
  <PropertyGroup>
    <AnalysisLevel>latest-All</AnalysisLevel> # use newest warnings available
    <ErrorLog>codeanalysis.sarif.json</ErrorLog> # name the code analysis result file, otherwise it gets the name of the project. 
    <AnalysisLevel>preview-recommended</AnalysisLevel> # Use all recommended warnings
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild> # Show and create warnings during build
    <AnalysisMode>All</AnalysisMode>  # Use all Analysis
  </PropertyGroup>
</Project>
```

See <https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-7> for details.

### Code Style

To enforce a common code style, it is advisable to use an `.editorconfig` file at the root of your project. It's understood by Visual Studio, Visual Studio code and Rider. 
Most interestingly, there is an option to treat Code Style violations as warnings in the build.

```
dotnet_analyzer_diagnostic.category-Style.severity = warning
```

Combining this with the previous item, we enable the build to create a Sarif-Issues file per Project which contains both Warnings and Code Style Violations. 
Sadly, Gitlab cannot understand Sarif files, so we have written a small tool to convert both Roslynator and Sarif files to gitlab issues: <https://github.com/codecentric/dotnet_gitlab_code_quality>


## Gotchas:

- To get the issues in a Merge Request, you need the pipeline on your target branch first! E.g. if you've never run code analysis before and then run it in your MR `main <- newCICD` it will NOT show any issues.
  It will show issues if you merge the MR and create a new one which either fixes or adds new issues.

- There is the third parameter for `cq` `%DIR`. Gitlab only understands issues if the file paths are relative to the repository root. The tools all report absolute paths, e.g. `c:\dev\Repo\example.cs`. You can use the third parameter to chop off `c:\dev\Repo`. 
  Please note that any failure to do this will cause the reports to be ignored by Gitlab. If it does not work, download the created issues file and inspect the paths.