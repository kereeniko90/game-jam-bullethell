using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
	public static BulletPool Instance;

	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private int poolSize = 50;

	private Queue<GameObject> bulletPool = new Queue<GameObject>();

	void Awake()
	{
		Instance = this;

		for (int i = 0; i < poolSize; i++)
		{
			GameObject bullet = Instantiate(bulletPrefab);
			bullet.SetActive(false);
			bulletPool.Enqueue(bullet);
		}
	}

	public GameObject GetBullet(Vector2 position, Quaternion rotation)
	{
		if (bulletPool.Count > 0)
		{
			GameObject bullet = bulletPool.Dequeue();
			bullet.transform.position = position;
			bullet.transform.rotation = rotation;

			// ✨ Important: Prevent inherited transform changes
			bullet.transform.SetParent(null);

			// Reset Rigidbody if present
			Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
			if (rb != null)
			{
				rb.linearVelocity = Vector2.zero;
				rb.angularVelocity = 0f;
			}

			bullet.SetActive(true);
			return bullet;
		}
		else
		{
			Debug.LogWarning("Pool exhausted — consider increasing pool size.");
			return null;
		}
	}




	public void ReturnBullet(GameObject bullet)
	{
		bullet.SetActive(false);
		bulletPool.Enqueue(bullet);
	}
}
