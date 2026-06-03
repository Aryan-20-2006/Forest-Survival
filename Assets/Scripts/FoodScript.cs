using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FoodScript : MonoBehaviour, IInteractable, IKeyOnlyInteractable
{
	[Header("Food Settings")]
	[SerializeField, Min(1)] private int foodAmount = 1;
	[SerializeField] private bool destroyOnPickup = true;
	[SerializeField] private FoodCounter foodCounter;

	[Header("Prompt UI")]
	[SerializeField] private TMP_Text promptLabel;

	private Collider foodCollider;
	private ReticleInteractor reticleInteractor;

	private void Awake()
	{
		foodCollider = GetComponent<Collider>();

		if (foodCounter == null)
			foodCounter = FindFirstObjectByType<FoodCounter>();
	}

	private void Start()
	{
		if (promptLabel != null)
		{
			promptLabel.gameObject.SetActive(false);

			reticleInteractor = FindFirstObjectByType<ReticleInteractor>();
			if (reticleInteractor != null)
			{
				reticleInteractor.OnHoverEnter += OnHoverEnter;
				reticleInteractor.OnHoverExit += OnHoverExit;
			}
		}
	}

	private void OnDestroy()
	{
		if (reticleInteractor != null)
		{
			reticleInteractor.OnHoverEnter -= OnHoverEnter;
			reticleInteractor.OnHoverExit -= OnHoverExit;
		}
	}

	private void OnHoverEnter(string text)
	{
		if (reticleInteractor.CurrentInteractable == (IInteractable)this)
			promptLabel.gameObject.SetActive(true);
	}

	private void OnHoverExit()
	{
		promptLabel.gameObject.SetActive(false);
	}

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

