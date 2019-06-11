using UnityEngine;
using System.Collections;

public class UConsoleExampleBulletScript : MonoBehaviour
{
	Rigidbody rb;

	void Start()
	{ // Get my rigidbody component and add a force to the right
		rb = GetComponent<Rigidbody>();
		rb.AddForce(Vector3.right * 1000);
	}

	void OnCollisionEnter (Collision other)
	{ // If this bullet collides with another object that is not the player, destroy it
		if (other.gameObject.tag != "Player")
			Destroy(gameObject);
	}
}