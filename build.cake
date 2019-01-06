var target = Argument ("target", Argument ("t", "Default"));
var configuration = "Release"; 
var buildVerbosity = Verbosity.Minimal;
var outputDirectory = "bin";
var outputLocation = $"../../{outputDirectory}";
var solution = "Dnn.AzureKeyVault.sln";
var projectDirectory = MakeAbsolute(Directory("./"));

var websiteLocation = "E:/RochesterXamarin/Website";
var assemblies = new []
{
	"HoeflingSoftware.DotNetNuke.Data.Providers.KeyVault",
	"HoeflingSoftware.Web.Security.KeyVault",
	"HoeflingSoftware.Web.Security"
};

Task("Default")
	.IsDependentOn("Debug Deploy");

Task("Build Only")
	.IsDependentOn("Build");

Task("Deploy")
	.IsDependentOn("Build")
	.IsDependentOn("Deploy DLLs");

Task("Debug Deploy")
	.IsDependentOn("Change Configuration to Debug")
	.IsDependentOn("Build")
	.IsDependentOn("Deploy DLLs")
	.IsDependentOn("Deploy PDBs");

Task("Change Configuration to Debug").Does(() =>
{
	configuration = "Debug";
});

Task("Clean").Does(() =>
{
	CleanDirectories("./src/**/bin");
	CleanDirectories("./src/**/obj");
	Information("Cleaned bin directory for each project");
	Information("Cleaned obj directory for each project");

	if (DirectoryExists(outputDirectory))
	{
		DeleteDirectory(outputDirectory, new DeleteDirectorySettings 
		{
			Recursive = true,
			Force = true
		});
		
		Information("Deleted root bin directory");
	}	
});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
{
	NuGetRestore(solution);
	MSBuild(solution, c => 
	{
		c.Configuration = "Debug";
		c.WithProperty("OutputPath", outputLocation);
	});
});

Task("Deploy to DNN")
	.Does(() =>
{
	if (string.IsNullOrEmpty(websiteLocation))
	{
		throw new Exception("No website location to deploy to. \"websiteLocation\" file: \"build.cake\"");
	}
});

Task("Deploy DLLs")
	.IsDependentOn("Deploy to DNN")
	.Does(() =>
{
	Information($"Copying dlls to {websiteLocation}/bin");
	foreach (var assembly in assemblies)
	{
		var currentFile = $"{websiteLocation}/bin/{assembly}.dll"; 
		if (FileExists(currentFile))
		{
			DeleteFile(currentFile);
		}

		CopyFile($"{projectDirectory}/{outputDirectory}/{assembly}.dll", currentFile);
		Information($"Copied {assembly}.dll");
	}
});

Task("Deploy PDBs")
	.IsDependentOn("Deploy to DNN")
	.Does(() =>
{
	Information($"Copying pdbs to {websiteLocation}/bin");
	foreach (var assembly in assemblies)
	{
		var currentFile = $"{websiteLocation}/bin/{assembly}.pdb"; 
		if (FileExists(currentFile))
		{
			DeleteFile(currentFile);
		}

		CopyFile($"{projectDirectory}/{outputDirectory}/{assembly}.pdb", currentFile);
		Information($"Copied {assembly}.pdb");
	}
});

RunTarget(target);
