using System;
using System.IO;
using UnityEngine;
[Serializable]
public class BuildConfigPair
{
	public string name;
	public bool enable;
	public BuildConfigPair(string inName)
	{
		name = inName;
	}
}
[Serializable]
public class BuildConfigPairs
{
	[SerializeField]
	BuildConfigPair[] pairs;
	public BuildConfigPair[] Pairs => pairs;
	public BuildConfigPairs(string[] inNames)
	{
		pairs = new BuildConfigPair[inNames.Length];
		for(int i = 0; i < inNames.Length; ++i)
		{
			pairs[i] = new BuildConfigPair(inNames[i]);
		}
	}
	public void Set(string inName, bool inEnable)
	{
		if(pairs == null)
		{
			return;
		}
		foreach(var pair in pairs)
		{
			if(pair.name == inName)
			{
				pair.enable = inEnable;
				return;
			}
		}
	}
	public bool Get(string inName)
	{
		if(pairs == null)
		{
			return false;
		}
		foreach(var pair in pairs)
		{
			if(pair.name == inName)
			{
				return pair.enable;
			}
		}
		return false;
	}
}
[Serializable]
public class BuildConfigOS
{
	public string buildMachineOS;
	public BuildConfigPairs types;
	public BuildConfigPairs platforms;
	public BuildConfigOS(string inOS, string[] inType, string[] inPlatforms)
	{
		buildMachineOS = inOS;
		types = new BuildConfigPairs(inType);
		platforms = new BuildConfigPairs(inPlatforms);
	}
}
[Serializable]
public class BuildConfig
{
	const string configPath = "appbuilder.json";
	public BuildConfigOS[] configs;
	public BuildConfig(string[] inOS, string[] inTypes, string[] inPlatforms)
	{
		configs = new BuildConfigOS[inOS.Length];
		for(int i = 0; i < inOS.Length; ++i)
		{
			configs[i] = new BuildConfigOS(inOS[i], inTypes, inPlatforms);
		}
	}
	public BuildConfigOS GetCurrentConfigOS()
	{
		if(configs == null)
		{
			return null;
		}
		foreach(var config in configs)
		{
			if(Enum.TryParse<RuntimePlatform>(config.buildMachineOS, out var os) && Application.platform == os)
			{
				return config;
			}
		}
		return null;
	}
	public BuildConfigOS GetConfigOS(string inOS)
	{
		if(configs == null)
		{
			return null;
		}
		foreach(var config in configs)
		{
			if(config.buildMachineOS == inOS)
			{
				return config;
			}
		}
		return null;
	}
	public static void Save(BuildConfig inConfig)
	{
		if(inConfig == null)
		{
			return;
		}
		using(var sw = new StreamWriter(configPath))
		{
			sw.Write(JsonUtility.ToJson(inConfig, true));
		}
	}
	public static BuildConfig Load()
	{
		if(File.Exists(configPath))
		{
			using(var sr = new StreamReader(configPath))
			{
				var conf = JsonUtility.FromJson<BuildConfig>(sr.ReadToEnd());
				return conf;
			}
		}
		return null;
	}
	public static BuildConfig LoadAsNewConfig(string[] inOS, string[] inTypes, string[] inPlatforms)
	{
		var conf = Load();
		if(conf != null)
		{
			return conf;
		}
		var newConf = new BuildConfig(inOS, inTypes, inPlatforms);
		Save(newConf);
		return newConf;
	}
}
