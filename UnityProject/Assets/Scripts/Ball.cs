using UnityEngine;
using System;
[Serializable]
public class BallParameter
{
	[SerializeField] Color mColor;
	[SerializeField] float mScale;
	[SerializeField] int mScore;
	public Color color => mColor;
	public float scale => mScale;
	public int score => mScore;
}
[Serializable]
public class BallParameters
{
	[SerializeField] BallParameter[] mParams;
	public int count => mParams.Length;
	public BallParameter Get(int inLevel)
	{
		if(inLevel < 0 || inLevel >= mParams.Length)
		{
			return null;
		}
		return mParams[inLevel];
	}
}
public class Ball : MonoBehaviour
{
	[SerializeField] SpriteRenderer mSprite;
	[SerializeField] Rigidbody2D mRigidbody;
	[SerializeField] Collider2D mCollider;
	int mLevel;
	Player mPlayer;
	bool mIsHited;
	public void SetDynamic(bool inIsDynamic)
	{
		mRigidbody.isKinematic = !inIsDynamic;
		mCollider.enabled = inIsDynamic;
	}
	public void Setup(Player inPlayer, int inLevel, BallParameter inParam)
	{
		mLevel = inLevel;
		mPlayer = inPlayer;
		mSprite.color = inParam.color;
		transform.localScale = new Vector3(inParam.scale, inParam.scale, 1.0f);
	}
	public void Update()
	{
		if(mCollider.enabled)
		{
			mPlayer.CheckGameOver(transform.position);
		}
	}
	void OnCollisionEnter2D(Collision2D inCollision)
	{
		if(mIsHited)
		{
			return;
		}
		if(inCollision.collider.TryGetComponent<Ball>(out var ball))
		{
			if(ball.mLevel == mLevel)
			{
				ball.mIsHited = true;
				Destroy(gameObject);
				Destroy(ball.gameObject);
				var newBall = mPlayer.ConbineBall(mLevel + 1);
				if(newBall != null)
				{
					newBall.transform.position = (transform.position + ball.transform.position) / 2.0f;
				}
			}
		}
	}
}
