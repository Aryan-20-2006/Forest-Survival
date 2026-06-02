using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AxeTreeHitbox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AxeSwingTrigger swingTrigger;

    [Header("Hit Control")]
    [SerializeField] private bool oneHitPerTreePerSwing = true;

    private readonly HashSet<TreeChopShrink> hitTreesThisSwing = new HashSet<TreeChopShrink>();
    private Collider hitCollider;
    private bool wasSwinging;

    private void Awake()
    {
        hitCollider = GetComponent<Collider>();

        if (swingTrigger == null)
        {
            swingTrigger = GetComponentInParent<AxeSwingTrigger>(true);
        }

        if (hitCollider != null)
        {
            hitCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        bool isSwinging = swingTrigger != null && swingTrigger.IsSwinging;

        if (!isSwinging && wasSwinging)
        {
            hitTreesThisSwing.Clear();
        }

        wasSwinging = isSwinging;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryProcessTreeHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryProcessTreeHit(other);
    }

    private void TryProcessTreeHit(Collider other)
    {
        if (swingTrigger == null || !swingTrigger.IsSwinging)
        {
            return;
        }

        TreeChopShrink tree = other.GetComponentInParent<TreeChopShrink>();
        if (tree == null)
        {
            return;
        }

        if (oneHitPerTreePerSwing)
        {
            if (!hitTreesThisSwing.Add(tree))
            {
                return;
            }
        }

        tree.RegisterAxeHit();
    }
}
