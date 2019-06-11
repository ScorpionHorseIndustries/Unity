using UnityEngine;
using System.Collections;

public class UConsoleExamplePlayerScript : MonoBehaviour
{
	// Prefab References
	public GameObject bulletPrefab;

	// Components
	CharacterController cc;
		
	// Statistical variables
	Vector3 vel;
	static public float moveSpeed = 5;
	float jumpStrength = 0.3f;

	void Start()
	{ 
		// Get my CharacterController component
		cc = GetComponent<CharacterController>();

		// Making sure the public bulletPrefab var has been set
		if (!bulletPrefab)
			Debug.Log("No bulletPrefab has been assigned to the Player object, please assign one in the inspector.", this);
	}

	void Update()
	{ 
		// Set X velocity to the horizontal input axis
		vel.x = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
			
		if (Input.GetButtonDown("Fire1"))
		{ // If Fire1 is pressed, create a bullet
			Instantiate(bulletPrefab, transform.position + Vector3.right, transform.rotation);
		}

		if (cc.isGrounded)
		{ // If we're grounded, remove gravity force, then add jump force if the Jump or Vertical buttons are pressed
			vel.y = 0;
			if (Input.GetButton("Vertical") || Input.GetButton("Jump"))
				vel.y = jumpStrength;
		}
		else // If we're not grounded, add gravity force
			vel += Physics.gravity * Time.deltaTime * 0.09f;

		// Apply our velocity as CharacterController movement
		cc.Move(vel);
	}
}