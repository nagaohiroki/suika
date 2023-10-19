using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
[Serializable]
public class SaveData
{
	[SerializeField] List<int> mScoreRanking;
	public List<int> scoreRanking => mScoreRanking;
	public void Init(int inCount)
	{
		mScoreRanking = new List<int>();
		for(int i = 0; i < inCount; ++i)
		{
			mScoreRanking.Add(0);
		}
	}
	public void AddScore(int inNewScore)
	{
		int count = mScoreRanking.Count;
		mScoreRanking.Add(inNewScore);
		mScoreRanking.Sort();
		mScoreRanking.Reverse();
		if(mScoreRanking.Count > count)
		{
			mScoreRanking.RemoveAt(mScoreRanking.Count - 1);
		}
	}
}
public static class SaveUtility
{
	static string mSavePath => Path.Combine(Application.persistentDataPath, "sv.json");
	public static SaveData Load()
	{
		try
		{
			using(var sr = new StreamReader(mSavePath))
			{
				var json = sr.ReadToEnd();
				return JsonUtility.FromJson<SaveData>(json);
			}
		}
		catch(Exception e)
		{
			Debug.Log(e.Message);

		}
		return null;
	}
	public static void Save(SaveData inSaveData)
	{
		try
		{
			using(var sw = new StreamWriter(mSavePath))
			{
				var json = JsonUtility.ToJson(inSaveData);
				sw.Write(json);
			}
		}
		catch(Exception e)
		{
			Debug.Log(e.Message);
		}
	}
}
