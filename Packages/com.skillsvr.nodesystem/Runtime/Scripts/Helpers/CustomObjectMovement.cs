using SkillsVRNodes;
using UnityEngine;

[ExecuteInEditMode]
public class CustomObjectMovement : MonoBehaviour
{
	public float distance = 5f;

	private PlayerSpawnPosition playerSpawnPosition;
	private Transform targetTransform;
#if UNITY_EDITOR
	private void OnEnable()
	{
		if (playerSpawnPosition == null)
		{
			playerSpawnPosition = FindObjectOfType<PlayerSpawnPosition>();
			if (playerSpawnPosition != null)
				targetTransform = playerSpawnPosition.transform;
		}
	}

	private void LateUpdate()
	{
		if (playerSpawnPosition == null)
		{
			playerSpawnPosition = FindObjectOfType<PlayerSpawnPosition>();
			if (playerSpawnPosition != null)
				targetTransform = playerSpawnPosition.transform;

			return;
		}
		else if(targetTransform == null)
			targetTransform = playerSpawnPosition.transform;

		// Calculate the target position based on the player's position and the distance
		Vector3 targetPosition = targetTransform.position + targetTransform.forward;

#if UNITY_EDITOR
		// Move the object only during edit mode in the Scene view
		if (!Application.isPlaying)
		{
			// Update the target position with the Scene view position
			targetPosition = transform.position;
		}
#endif

		// Reverse the rotation to face the player's camera
		transform.rotation = Quaternion.LookRotation(transform.position - targetTransform.position, Vector3.up);

		var norm = (transform.position - targetTransform.position).normalized;

		transform.position = targetTransform.position + (norm * distance);
	}
#endif
}