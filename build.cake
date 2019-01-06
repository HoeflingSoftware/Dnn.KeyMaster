var target = Argument ("target", Argument ("t", "Default"));
var configuration = "Release"; 
var buildVerbosity = Verbosity.Minimal;
var outputDirectory = "bin";
var outputLocation = $"../../{outputDirectory}";
var solution = "Dnn.KeyMaster.sln";
var projectDirectory = MakeAbsolute(Directory("./"));

var websiteLocation = "E:/RochesterXamarin/Website";
var assemblies = new []
{
	"Dnn.KeyMaster.Data.Providers.KeyVault",
	"Dnn.KeyMaster.Web.Security.KeyVault",
	"Dnn.KeyMaster.Web.Security",
	"Dnn.KeyMaster.PersonaBar"
};

Task("Default")
	.IsDependentOn("Debug Deploy");

Task("Build Only")
	.IsDependentOn("Build");

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

	Information("Adding secrets.json File . . .");
	// TODO - we need to add the secrets.json file scaffolding

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
	Zip("./", "Dnn.KeyMaster_install.zip", files);
	Information("Dnn Extension Installer Created - Dnn.KeyMaster.zip");

	DeleteFile("./Resources.zip");
});

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
