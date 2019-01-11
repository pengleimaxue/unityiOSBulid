
#if 	UNITY_EDITOR && UNITY_IPHONE

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public static class XCodePostProcess
{
	private	static	void	ParseFrameWork( string strPathVal, List< string > listVal )
	{
		string[] dirs = System.IO.Directory.GetDirectories( strPathVal );

		for( int lCnt = 0; lCnt < dirs.Length; ++lCnt )
		{
			if( dirs[lCnt].EndsWith( ".framework" ) )
			{
				if( ! listVal.Contains( dirs[lCnt] ) )
				{
					listVal.Add( dirs[lCnt] );
				}
			}
			else
			{
				ParseFrameWork( dirs[lCnt], listVal );
			}

		}
	}

	public	static	string	s_projModePath = null;


	[PostProcessBuild(100)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
		if( string.IsNullOrEmpty( s_projModePath ) )
		{
			return;
		}

		Debug.Log( "Export Path => " + pathToBuiltProject );

		if ( target != BuildTarget.iOS )
		{
			Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
			return;
		}

		// Create a new project object from build target
		XCProject project = new XCProject( pathToBuiltProject );

		List< string > ListFrameworks = new List< string >();

		XCMod mod = new XCMod( s_projModePath );

		foreach( string folderPath in mod.folders )
		{
			ParseFrameWork( folderPath, ListFrameworks );
		}
		
		for( int lCnt = 0; lCnt < ListFrameworks.Count; ++lCnt )
		{
			mod.files.Add( ListFrameworks[lCnt] );
		}

		project.ApplyMod( mod );

		// project.overwriteBuildSetting("CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Distribution", "Release");

		project.Save();

		s_projModePath = null;
	}

	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}

}

#endif
