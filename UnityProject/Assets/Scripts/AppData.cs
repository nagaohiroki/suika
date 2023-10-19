using UnityEngine;
public class AppData : MonoBehaviour
{
	[SerializeField] int mFrameRate = 60;
	[SerializeField] bool mIsVsync = false;
	void Awake()
	{
		Application.targetFrameRate = mFrameRate;
		QualitySettings.vSyncCount = mIsVsync ? 1 : 0;
	}
}
