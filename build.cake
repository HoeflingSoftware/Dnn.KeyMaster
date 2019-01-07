var target = Argument ("target", Argument ("t", "Build"));
var version = Argument ("package_version", "0.0.0");
var configuration = "Release"; 
var buildVerbosity = Verbosity.Minimal;
var outputDirectory = "bin";
var solution = "Dnn.KeyMaster.sln";
var projectDirectory = MakeAbsolute(Directory("./"));
var assemblies = new []
{
	"Dnn.KeyMaster.Providers",
	"Dnn.KeyMaster.API",
	"Dnn.KeyMaster.Web.Security.KeyVault",
	"Dnn.KeyMaster.PersonaBar"
};

// Assumes you have a DNN website installed at the following location
// Update to your path to use the deploy task.
var websiteLocation = "E:/AzureKeyVault/Website";

Task("Package")
	.IsDependentOn("Build")
	.Does(() =>
{
	Information("Creating Dnn Extension Installer. . .");
	Information("Packing Key Master Providers . . .");
	Information("Adding Dnn.KeyMaster Assemblies . . .");
	
	var files = new List<string>();
	foreach (var assembly in assemblies)
	{
		files.Add($"./{outputDirectory}/{assembly}.dll");
	}

	Information("Adding Dnn Manifest File . . .");
	files.Add("Dnn.KeyMaster.dnn");

	Information("\r\n");
	Information("Packing Key Master Persona Bar Admin Menu . . .");

		
	var personaBarFiles = new []
	{
		"./src/Dnn.KeyMaster.PersonaBar/App_LocalResources/KeyMaster.resx",
		"./src/Dnn.KeyMaster.PersonaBar/css/KeyMaster.css",
		"./src/Dnn.KeyMaster.PersonaBar/scripts/KeyMaster.js",
		"./src/Dnn.KeyMaster.PersonaBar/KeyMaster.html",
	};

	Zip("./src/Dnn.KeyMaster.PersonaBar", "Resources.zip", personaBarFiles);

	files.Add("Resources.zip");
	Zip("./", $"Dnn.KeyMaster_{version}_install.zip", files);
	Information($"Dnn Extension Installer Created - Dnn.KeyMaster_{version}_install.zip");

	DeleteFile("./Resources.zip");
});

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
	DotNetCoreRestore(solution);

	var msbuildSettings = new DotNetCoreMSBuildSettings();
	msbuildSettings.Properties.Add("Version", new [] { version} );

	DotNetCoreBuild(solution, new DotNetCoreBuildSettings
	{
		Configuration = configuration,
		OutputDirectory = outputDirectory,
		MSBuildSettings = msbuildSettings
	});
});

Task("Deploy")
	.IsDependentOn("Change Configuration to Debug")
	.IsDependentOn("Build")
	.Does(() =>
{
	if (string.IsNullOrEmpty(websiteLocation))
	{
		throw new Exception("No website location to deploy to. \"websiteLocation\" file: \"build.cake\"");
	}

	Information($"Copying dlls to {websiteLocation}/bin");
	foreach (var assembly in assemblies)
	{
		var currentDLL = $"{websiteLocation}/bin/{assembly}.dll"; 
		if (FileExists(currentDLL))
		{
			DeleteFile(currentDLL);
		}

		CopyFile($"{projectDirectory}/{outputDirectory}/{assembly}.dll", currentDLL);
		Information($"Copied {assembly}.dll");

		var currentPDB = $"{websiteLocation}/bin/{assembly}.pdb"; 
		if (FileExists(currentPDB))
		{
			DeleteFile(currentPDB);
		}

		CopyFile($"{projectDirectory}/{outputDirectory}/{assembly}.pdb", currentPDB);
		Information($"Copied {assembly}.pdb");
	}
});

RunTarget(target);
