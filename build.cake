#addin "Cake.Powershell&version=0.4.7"

var target = Argument ("target", Argument ("t", "Build"));
var version = Argument ("package_version", "1.0.6");
var configuration = "Release"; 
var buildVerbosity = Verbosity.Minimal;
var outputDirectory = "bin";
var solution = "Dnn.KeyMaster.sln";
var projectDirectory = MakeAbsolute(Directory("./"));
var assemblies = new []
{
	"Dnn.KeyMaster.API",
	"Dnn.KeyMaster.Configuration",
	"Dnn.KeyMaster.Configuration.AzureKeyVault",
	"Dnn.KeyMaster.Exceptions",
	"Dnn.KeyMaster.PersonaBar",
	"Dnn.KeyMaster.Providers"
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

		
	CopyFile("./icon.png", "./src/Dnn.KeyMaster.PersonaBar/icon.png");
	var personaBarFiles = new []
	{
		"./src/Dnn.KeyMaster.PersonaBar/App_LocalResources/KeyMaster.resx",
		"./src/Dnn.KeyMaster.PersonaBar/css/KeyMaster.css",
		"./src/Dnn.KeyMaster.PersonaBar/scripts/KeyMaster.js",
		"./src/Dnn.KeyMaster.PersonaBar/KeyMaster.html",
		"./src/Dnn.KeyMaster.PersonaBar/icon.png"
	};

	Zip("./src/Dnn.KeyMaster.PersonaBar", "Resources.zip", personaBarFiles);

	files.Add("Resources.zip");

	Information("Adding License file");
	CopyFile("./LICENSE", "License.txt");
	files.Add("License.txt");

	Information("Adding Release Notes file");
	files.Add("./ReleaseNotes.txt");

	Zip("./", $"Dnn.KeyMaster_{version}_install.zip", files);
	Information($"Dnn Extension Installer Created - Dnn.KeyMaster_{version}_install.zip");

	DeleteFile("./Resources.zip");
	DeleteFile("./License.txt");
	DeleteFile("./src/Dnn.KeyMaster.PersonaBar/icon.png");
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

Task("Dnn Manifest")
	.Does(() =>
{
	Information("Updating Dnn Manifest File");
	StartPowershellFile("./BuildUtilities/buildDnnManifest.ps1", new PowershellSettings()
		.WithArguments(args => 
		{
			args.Append("Version", version);
		}));
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Dnn Manifest")
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

	var websiteModuleLocation = $"{websiteLocation}/DesktopModules/Admin/Dnn.PersonaBar/Modules/KeyMaster/";
	Information($"Copying html/js/css to {websiteModuleLocation}");
	CopyFile($"{projectDirectory}/src/Dnn.KeyMaster.PersonaBar/KeyMaster.html", $"{websiteModuleLocation}/KeyMaster.html");
	CopyFile($"{projectDirectory}/src/Dnn.KeyMaster.PersonaBar/scripts/KeyMaster.js", $"{websiteModuleLocation}/scripts/KeyMaster.js");
	CopyFile($"{projectDirectory}/src/Dnn.KeyMaster.PersonaBar/css/KeyMaster.css", $"{websiteModuleLocation}/css/KeyMaster.css");	
});

RunTarget(target);
