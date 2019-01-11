#if 	UNITY_EDITOR && UNITY_IPHONE

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class DMIosBuilder : EditorWindow
{
	private	static	bool	s_Showed = false;
	private	static	string	ExportPath = DateTime.Now.ToString("yyyyMMdd");
	private static  string  PluginsPath = "";
	private	static	bool	s_bExportType = true;
	
	[MenuItem("BuildApp/BuildXCode")]
	public	static void ShowBuildTool () 
	{
		if( DMIosBuilder.s_Showed || ! ProcessXCodeProj() )
		{
			return;
		}

		Vector2 v2Size = new Vector2( 400.0f, 600.0f );
		DMIosBuilder window = (DMIosBuilder) EditorWindow.GetWindow ( typeof (DMIosBuilder) );

		window.title = "Export XCode Project Panel";
		window.maxSize = v2Size;
		window.minSize = v2Size;

		DMIosBuilder.s_bExportType  = true;
		DMIosBuilder.s_Showed = true;

		window.Show();
	}
	
	[MenuItem("BuildApp/ProcessXCodeProject")]	
	private	static void ShowProcessTool () 
	{
		if( DMIosBuilder.s_Showed || ! ProcessXCodeProj() )
		{
			return;
		}

		Vector2 v2Size = new Vector2( 400.0f, 600.0f );
		DMIosBuilder window = (DMIosBuilder) EditorWindow.GetWindow ( typeof (DMIosBuilder) );
				
		window.title = "Process XCode Project Panel";
		window.maxSize = v2Size;
		window.minSize = v2Size;

		DMIosBuilder.s_bExportType  = false;
		DMIosBuilder.s_Showed = true;

		window.Show();
	}

	private	static bool ProcessXCodeProj () 
	{		
		string path = EditorUtility.OpenFilePanel
			(
				"Choose XCode ProjMode File",
				Application.dataPath,
				"projmods" );

		if ( string.IsNullOrEmpty( path ) )	
		{
			UnityEngine.Debug.Log( "[Error]: Can't Find XCde Proj Cfg!!!" );
			XCodePostProcess.s_projModePath = null;
			return false;
		}
		if (!string.IsNullOrEmpty (path)) 
		{
			PluginsPath = path;
		}
		XCodePostProcess.s_projModePath = path;

		return	true;
	}
	
	void OnDestroy()
	{
		DMIosBuilder.s_Showed = false;
	}
	
	void OnGUI () 
	{
		float fYLabor = 20.0f;
		
		GUI.Label( new Rect( 0.0f, 0.0f, 400.0f, 36.0f ), 
		          string.Format( "Export Direction => [{0}]", ExportPath ) );


		ExportPath = GUI.TextField( new Rect( 80.0f, fYLabor, 240.0f, 44.0f ), ExportPath );


		string showText = s_bExportType ? "Export" : "Process Proj";

		Color old = GUI.backgroundColor;
		GUI.backgroundColor = Color.red;

		if( 0 < ExportPath.Length && GUI.Button( new Rect( 80.0f, fYLabor + 60.0f, 240.0f, 44.0f ), showText ) )
		{
			string pathVal = Application.dataPath.Replace( '\\', '/' );
			int indexFind  = pathVal.LastIndexOf( '/' );
			pathVal = pathVal.Substring( 0, indexFind + 1 ) + ExportPath;

			if( s_bExportType)
			{
				DMCore.Build.CBuildIOHelp.DeleteDirectory( pathVal );

				BuildToXCode( pathVal );
			}
			else
			{
				XCodePostProcess.OnPostProcessBuild( BuildTarget.iOS, pathVal );
				
				XCodePostProcess.s_projModePath = null;
			}


			CopyPlistAndAppController(pathVal,PluginsPath);
			CopyImageAssest(pathVal,PluginsPath);
			UnityEngine.Debug.Log( "Build End!!!" );
			UnityEngine.Debug.Log( "Build End!!!" );

			Close();			
			DMIosBuilder.s_Showed = false;
		}

		GUI.backgroundColor = old;
	}


	private static bool BuildToXCode( string projectName )
	{
		try
		{
			AssetDatabase.Refresh();

			List< string > listEditorScenes = new List< string >();
			foreach ( EditorBuildSettingsScene scene in EditorBuildSettings.scenes )
			{
				if ( null != scene && scene.enabled )
				{
					listEditorScenes.Add( scene.path );
				}
			}
			
			string res = BuildPipeline.BuildPlayer( listEditorScenes.ToArray(), projectName, BuildTarget.iOS, BuildOptions.None );

			if ( res.Length > 0 )
			{
				UnityEngine.Debug.LogError( "Build Failed => " + res );
				return false;
			}
		}
		catch ( Exception ex )
		{
			UnityEngine.Debug.LogError(ex.StackTrace);
			return false;
		}
		
		return true;
	}

	public static void CopyEntireDir(string sourcePath, string destPath) { 
		string[] floderArray = System.IO.Directory.GetDirectories( destPath, "*", SearchOption.TopDirectoryOnly);
		for( int lCnt = 0; lCnt < floderArray.Length; ++lCnt )
		{
			System.IO.Directory.Delete( floderArray[lCnt], true );
		}
		//Now Create all of the directories 
		foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			Directory.CreateDirectory(dirPath.Replace(sourcePath, destPath)); 
		//Copy all the files & Replaces any files with the same name 
		foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {

			File.Copy(newPath, newPath.Replace(sourcePath, destPath), true); 
		} 
	   
	}
	
	public static void CopyFile(string sourcePath,string targetPath)
	{
		bool isrewrite=true; // true=覆盖已存在的同名文件,false则反之
		System.IO.File.Copy(sourcePath, targetPath, isrewrite); 
	}

	//替换指定icon和启动图
	public static void CopyImageAssest(string projectPath,string copyFilePath) {
		string directionaryPath = Path.GetDirectoryName(copyFilePath);
		string imagePath = projectPath + "/Unity-iPhone/Images.xcassets";
		CopyEntireDir (directionaryPath + "/Images.xcassets", imagePath);
	}
	public static void CopyPlistAndAppController(string projectPath,string copyFilePath)
	{
		string directionaryPath = Path.GetDirectoryName(copyFilePath);
		string classPath = projectPath + "/Classes";

		CopyFile(directionaryPath + "/Info.plist",projectPath + "/Info.plist");
		CopyFile(directionaryPath + "/UnityAppController.h",classPath + "/UnityAppController.h");
		CopyFile (directionaryPath + "/UnityAppController.mm", classPath + "/UnityAppController.mm");
		CopyFile (directionaryPath + "/il2cpp-codegen.h", projectPath + "/Libraries/libil2cpp/include/codegen/il2cpp-codegen.h");
		UnityEngine.Debug.Log( "CopyFile End!!!" );
	}

	[MenuItem("BuildApp/BuildIPA")]
	public	static void chooseProject () 
	{
		string floderPath = EditorUtility.OpenFolderPanel( "Choose Floder", Application.dataPath, "" );
		if (string.IsNullOrEmpty (floderPath)) 
		{

			return;
		}

		UnityEngine.Debug.Log ("Select Floder =>" + floderPath );
		buildIpa (floderPath);

	}

	public static void buildIpa(string ProjectPath)
	{
		if (string.IsNullOrEmpty( ProjectPath )) 
		{
			UnityEngine.Debug.Log( "projectPath is null!!" );
			return;
		}

		if (System.IO.File.Exists(Path.GetFullPath(ProjectPath + "/log.text")))  
		{  
			File.Delete(Path.GetFullPath(ProjectPath + "/log.text"));  
		}
		plBuildXcodeIpa (ProjectPath);
		return;
		string shell = Application.dataPath + "/ipa-build.sh";
		string argss = shell + " " + ProjectPath;
		Process process = new Process ();
		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.ErrorDialog = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.FileName = "/bin/bash";
		process.StartInfo.Arguments = argss;
		process.StartInfo.RedirectStandardOutput = true;
		process.Start();
		string output = process.StandardOutput.ReadToEnd();
		File.AppendAllText (ProjectPath + "/log.text", output); //将得到的信息写入文件中。
		UnityEngine.Debug.Log( "start build ipa!!!" );

		process.WaitForExit();
		process.Close();
	}
	//
	public static void plBuildXcodeIpa(string ProjectPath) {
		if (string.IsNullOrEmpty( ProjectPath )) 
		{
			UnityEngine.Debug.Log( "projectPath is null!!" );
			return;
		}
//		string command = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal"; 
//		string shell = ProjectPath +"/xcodebuild.sh";
//		string arg1 =  "cd" + " " +ProjectPath+" " +"&&"+" ";
//		string argss =  arg1 + shell;
//		System.Diagnostics.Process.Start(command,argss);
//		UnityEngine.Debug.Log(argss);

//      用于做测试 shell传递参数需要用下面方法
//		string shell = ProjectPath +"/shell.sh";
//		string arg1 =  "unity";
//		string arg2 =  ProjectPath +"/test.log";
//		string argss =  shell +" "+ arg1 +" " + arg2;
//		System.Diagnostics.Process.Start("/bin/bash", argss);
//		string shell = "/shell.sh";
//		string arg =  ProjectPath;
//		string arg1 =  "unity18";
//		string arg2 =   "/test.log";
//		string argss = shell+" "+ arg1 +" " + arg2;
//		Process process = new Process ();
//		process.StartInfo.CreateNoWindow = false;
//		process.StartInfo.ErrorDialog = true;
//		process.StartInfo.UseShellExecute = false;
//		process.StartInfo.FileName = "/bin/bash";
//		process.StartInfo.Arguments = argss;
//		process.StartInfo.RedirectStandardOutput = true;
//		process.StartInfo.RedirectStandardInput = true;
//		process.StartInfo.WorkingDirectory = ProjectPath;
//		process.Start();
//		UnityEngine.Debug.Log( "process start rojectPath = !!" +ProjectPath);
//		string output = process.StandardOutput.ReadToEnd();
//		File.AppendAllText (ProjectPath + "/log.text", output); //将得到的信息写入文件中。
//		process.WaitForExit();
//		process.Close();
		string shell = "shell.sh";
		Process process = new Process ();
		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.ErrorDialog = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.FileName = "/bin/bash";
		process.StartInfo.Arguments = shell;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardInput = true;
		process.StartInfo.WorkingDirectory = ProjectPath;
		process.Start();
		UnityEngine.Debug.Log( "process start rojectPath = !!" +ProjectPath);
		string output = process.StandardOutput.ReadToEnd();
		File.AppendAllText (ProjectPath + "/log.text", output); //将得到的信息写入文件中。
		process.WaitForExit();
		process.Close();
	}
}


#endif
