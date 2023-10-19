using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityUtility;
using TMPro;
public class Player : MonoBehaviour
{
	[SerializeField] PlayerInput mInput;
	[SerializeField] Ball mBall;
	[SerializeField] GameObject mGameOver;
	[SerializeField] Transform mNextPosition;
	[SerializeField] Transform mDown;
	[SerializeField] Transform mGameOverBar;
	[SerializeField] TMP_Text mScoreText;
	[SerializeField] TMP_Text mScoreRankingText;
	[SerializeField] int mLotteryMax = 5;
	[SerializeField] int mScoreRankingCount = 3;
	[SerializeField] int mNext = 1;
	[SerializeField] float mSpeed = 3.0f;
	[SerializeField] float mCooldownMax = 1.0f;
	[SerializeField] BallParameters mParameters;
	float mCooldownCurrent;
	int mScore;
	SaveData mSaveData;
	Ball[] mHands;
	bool isGameOver => mGameOver != null ? mGameOver.activeSelf : false;
	public void CheckGameOver(Vector3 inPosition)
	{
		if(mGameOverBar.position.y < inPosition.y)
		{
			GameOver();
		}
	}
	public Ball ConbineBall(int inLevel)
	{
		mScore += mParameters.Get(inLevel).score;
		UpdateScore();
		return CreateBall(inLevel);
	}
	public void OnPoint(InputAction.CallbackContext inContext)
	{
		if(Camera.main != null)
		{
			Move(Camera.main.ScreenToWorldPoint(inContext.ReadValue<Vector2>()).x);
		}
	}
	public void OnFire(InputAction.CallbackContext inContext)
	{
		if(!inContext.started)
		{
			return;
		}
		if(mHands[0] == null)
		{
			return;
		}
		mCooldownCurrent = mCooldownMax;
		mHands[0].SetDynamic(true);
		mHands[0] = null;
	}
	public void OnRetry(InputAction.CallbackContext inContext)
	{
		if(!inContext.started)
		{
			return;
		}
		if(!isGameOver)
		{
			GameOver();
			return;
		}
		Time.timeScale = 1.0f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	Ball CreateBall(int inLevel)
	{
		var param = mParameters.Get(inLevel);
		if (param == null)
		{
		    return null;
		}
		var newBall = Instantiate(mBall);
		newBall.Setup(this, inLevel, param);
		return newBall;
	}
	Ball CreateRandomBall()
	{
		var level = RandomObject.GetGlobal.Range(0, mLotteryMax);
		var newBall = CreateBall(level);
		newBall.SetDynamic(false);
		return newBall;
	}
	void Move(float inX)
	{
		if (mDown == null)
		{
		    return;
		}
		var ball = mHands[0];
		float range = mDown.localScale.x * 0.5f;
		if(ball != null)
		{
			range -= ball.transform.localScale.x * 0.5f;
		}
		var pos = transform.position;
		pos.x = inX;
		pos.x = Mathf.Clamp(pos.x, -range, range);
		transform.position = pos;
		if(ball != null)
		{
			ball.transform.position = pos;
		}
	}
	void GameOver()
	{
		if(isGameOver)
		{
			return;
		}
		Time.timeScale = 0.0f;
		mGameOver.SetActive(true);
		mSaveData.AddScore(mScore);
		SaveUtility.Save(mSaveData);
		UpdateScoreRanking();
	}
	void UpdateScore()
	{
		mScoreText.text = $"Score\n{mScore}";
	}
	void UpdateScoreRanking()
	{
		mScoreRankingText.text = $"Score Ranking\n";
		foreach(var score in mSaveData.scoreRanking)
		{
			mScoreRankingText.text += $"{score}\n";
		}
	}
	void InitPos()
	{
		mHands[0].transform.position = transform.position;
		mHands[1].transform.position = mNextPosition.position;
	}
	void Add()
	{
		int max = mHands.Length;
		for(int i = 0; i < max; ++i)
		{
			int next = i + 1;
			mHands[i] = next >= max ? CreateRandomBall() : mHands[next];
		}
		InitPos();
	}
	void InitSaveData()
	{
		mSaveData = SaveUtility.Load();
		if(mSaveData == null)
		{
			mSaveData = new SaveData();
			mSaveData.Init(mScoreRankingCount);
			SaveUtility.Save(mSaveData);
		}
	}
	void Start()
	{
		InitSaveData();
		mHands = new Ball[mNext + 1];
		for(int i = 0; i < mHands.Length; ++i)
		{
			mHands[i] = CreateRandomBall();
		}
		InitPos();
		UpdateScoreRanking();
		UpdateScore();
	}
	void Update()
	{
		if(isGameOver)
		{
			return;
		}
		var moveAction = mInput.actions["Move"];
		var move = moveAction.ReadValue<Vector2>();
		Move(transform.position.x + move.x * mSpeed * Time.deltaTime);
		if(mCooldownCurrent > 0.0f)
		{
			mCooldownCurrent -= Time.deltaTime;
			if(mCooldownCurrent <= 0.0f)
			{
				mCooldownCurrent = 0.0f;
				Add();
			}
		}
	}
}
