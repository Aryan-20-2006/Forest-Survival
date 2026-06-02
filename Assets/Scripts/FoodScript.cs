using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FoodScript : MonoBehaviour, IInteractable, IInteractionInfo
{
	[Header("Food Settings")]
	[SerializeField, Min(1)] private int foodAmount = 1;
	[SerializeField] private string interactionPrompt = "Press E to pick up";
	[SerializeField] private bool destroyOnPickup = true;
	[SerializeField] private FoodCounter foodCounter;

	private Collider foodCollider;

	private void Awake()
	{
		foodCollider = GetComponent<Collider>();

		if (foodCounter == null)
		{
			foodCounter = FindFirstObjectByType<FoodCounter>();
		}
	}

	public string GetInteractionText() => interactionPrompt;

	public void Interact()
	{
		if (foodCounter == null)
		{
			foodCounter = FindFirstObjectByType<FoodCounter>();
			if (foodCounter == null)
			{
				Debug.LogWarning("FoodScript could not find FoodCounter in the scene.", this);
				return;
			}
		}

		foodCounter.ModifyFood(foodAmount);

		if (destroyOnPickup)
		{
			Destroy(gameObject);
		}
		else
		{
			if (foodCollider != null)
			{
				foodCollider.enabled = false;
			}

			gameObject.SetActive(false);
		}
	}
}

