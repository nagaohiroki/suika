using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets.Settings;
public class AppBuilder : EditorWindow
{
	enum BuildType
	{
		Release,
		Debug,
	}
	BuildTarget[] mBuildTargets = new[]
	{
		BuildTarget.StandaloneWindows64,
		BuildTarget.StandaloneOSX,
		BuildTarget.StandaloneLinux64,
		BuildTarget.iOS,
		BuildTarget.Android,
		BuildTarget.WebGL,
	};
	RuntimePlatform[] mBuildMachineOS = new[]
	{
		RuntimePlatform.WindowsEditor,
		RuntimePlatform.OSXEditor,
	};
	BuildConfig mConfig;
	int mOSIndex;
	[MenuItem("Window/AppBuilder")]
	static void Open()
	{
		EditorWindow.GetWindow<AppBuilder>(typeof(AppBuilder).Name);
	}
	void OnGUI()
	{
		var types = Enum.GetNames(typeof(BuildType));
		var os = mBuildMachineOS.Select(t => t.ToString()).ToArray();
		var platforms = mBuildTargets.Select(t => t.ToString()).ToArray();
		bool isDirty = false;
		if(mConfig == null)
		{
			mConfig = BuildConfig.LoadAsNewConfig(os, types, platforms);
		}
		var configOS = mConfig.GetConfigOS(os[mOSIndex]);
		if(configOS == null)
		{
			return;
		}
		EditorGUILayout.BeginVertical();
		mOSIndex = EditorGUILayout.Popup("BuildMachineOS", mOSIndex, os);
		EditorGUILayout.LabelField("BuildType -----------------------");
		isDirty |= GuiToggle(configOS.types, types);
		EditorGUILayout.LabelField("Platform ------------------------");
		isDirty |= GuiToggle(configOS.platforms, platforms);
		if(GUILayout.Button("Build"))
		{
			Build();
		}
		EditorGUILayout.EndVertical();
		if(isDirty)
		{
			BuildConfig.Save(mConfig);
		}
	}
	bool GuiToggle(BuildConfigPairs inPairs, string[] inNames)
	{
		bool isDirty = false;
		foreach(var n in inNames)
		{
			bool oldEnable = inPairs.Get(n);
			bool newEnable = EditorGUILayout.Toggle(n, oldEnable);
			inPairs.Set(n, newEnable);
			isDirty |= oldEnable != newEnable;
		}
		return isDirty;
	}
	static void Build()
	{
		const string mBuildDir = "Builds";
		if(Directory.Exists(mBuildDir))
		{
			Directory.Delete(mBuildDir, true);
		}
		var conf = BuildConfig.Load();
		if(conf == null)
		{
			return;
		}
		var conifgOS = conf.GetCurrentConfigOS();
		if(conifgOS == null)
		{
			return;
		}
		var old = EditorUserBuildSettings.activeBuildTarget;
		foreach(var type in conifgOS.types.Pairs)
		{
			if(!type.enable || !Enum.TryParse<BuildType>(type.name, out var buildType))
			{
				continue;
			}
			foreach(var platform in conifgOS.platforms.Pairs)
			{
				if(!platform.enable || !Enum.TryParse<BuildTarget>(platform.name, out var platformType))
				{
					continue;
				}
				BuildInternal(mBuildDir, platformType, buildType);
			}
		}
		EditorUserBuildSettings.SwitchActiveBuildTarget(ToBuildTargetGroup(old), old);
	}
	static void BuildInternal(string inBuildDir, BuildTarget inTarget, BuildType inBuildType)
	{
#if !UNITY_EDITOR_WIN
		if(inTarget == BuildTarget.StandaloneWindows64)
		{
			Debug.Log($"{inTarget} is Not Supported");
			return;
		}
#endif
#if !UNITY_EDITOR_OSX
		if(inTarget == BuildTarget.StandaloneOSX || inTarget == BuildTarget.iOS)
		{
			Debug.Log($"{inTarget} is Not Supported");
			return;
		}
#endif
		var buildName = $"{inTarget.ToString()}{inBuildType}";
		Debug.Log($"Build Start {buildName}");
		var dirName = Path.Combine(inBuildDir, buildName);
		Directory.CreateDirectory(dirName);
		var targetGroup = ToBuildTargetGroup(inTarget);
		var buildPlayerOptions = new BuildPlayerOptions();
		PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup, out var oldDefines);
		switch(inBuildType)
		{
			case BuildType.Debug:
				{
					buildPlayerOptions.options = BuildOptions.Development;
					PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, new string[] { "UNITY_DEBUG" });
					break;
				}
			case BuildType.Release:
				{
					buildPlayerOptions.options = BuildOptions.None;
					PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, new string[] { });
					break;
				}
		}
		var ext = string.Empty;
		if(inTarget == BuildTarget.StandaloneWindows64)
		{
			ext = ".exe";
		}
		buildPlayerOptions.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
		buildPlayerOptions.target = inTarget;
		buildPlayerOptions.targetGroup = targetGroup;
		buildPlayerOptions.locationPathName = Path.Combine(dirName, $"{PlayerSettings.productName}{ext}");
		AddressableAssetSettings.BuildPlayerContent();
		var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, oldDefines);
		DeleteDontShip(dirName);
		switch(report.summary.result)
		{
			case BuildResult.Succeeded:
				{
					Debug.Log($"Build Succeeded {buildName}");
					break;
				}
			case BuildResult.Failed:
				{
					Debug.Log($"Build Failed");
					Debug.Log(report.summary.totalErrors);
					break;
				}
		}
	}
	static void DeleteDontShip(string inDir)
	{
		var deleteDirs = new[]
		{
			"_BackUpThisFolder_ButDontShipItWithYourGame",
			"_BurstDebugInformation_DoNotShip"
		};
		foreach(var dir in deleteDirs)
		{
			var delDir = Path.Combine(inDir, $"{PlayerSettings.productName}{dir}");
			if(Directory.Exists(delDir))
			{
				Directory.Delete(delDir, true);
			}
		}
	}
	static BuildTargetGroup ToBuildTargetGroup(BuildTarget self)
	{
		switch(self)
		{
			case BuildTarget.StandaloneOSX:
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
			case BuildTarget.StandaloneLinux64:
				return BuildTargetGroup.Standalone;
			case BuildTarget.iOS: return BuildTargetGroup.iOS;
			case BuildTarget.Android: return BuildTargetGroup.Android;
			case BuildTarget.WebGL: return BuildTargetGroup.WebGL;
			case BuildTarget.WSAPlayer: return BuildTargetGroup.WSA;
			case BuildTarget.PS4: return BuildTargetGroup.PS4;
			case BuildTarget.XboxOne: return BuildTargetGroup.XboxOne;
			case BuildTarget.tvOS: return BuildTargetGroup.tvOS;
			case BuildTarget.Switch: return BuildTargetGroup.Switch;
			case BuildTarget.Stadia: return BuildTargetGroup.Stadia;
		}
		return BuildTargetGroup.Standalone;
	}
}
