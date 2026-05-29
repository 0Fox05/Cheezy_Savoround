using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SortingSystem : MonoBehaviour
{
    public static SortingSystem instance;
    private int activeMoves = 0; // lock flag for animations

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        Plate[] allPlates = FindObjectsOfType<Plate>();

        foreach (Plate plate in allPlates)
        {
            if (plate == null || !plate.isPlaced)
                continue;

            TryMoveFromPlate(plate);
        }

        ClearEmptyPlates(allPlates);
    }

    void TryMoveFromPlate(Plate source)
    {
        if (source.transform.childCount == 0)
            return;

        // 🚨 If plate has exactly one pizza → locked unless merging
        if (source.transform.childCount == 1)
        {
            string tag = source.transform.GetChild(0).tag;

            Plate target = GetNearbyValidTarget(source, tag);
            if (target != null && !target.IsFull())
            {
                MovePizza(source.transform.GetChild(0), source, target);
            }

            return; // otherwise locked
        }

        // Normal multi‑pizza behavior
        List<Transform> pizzas = new List<Transform>();
        foreach (Transform t in source.transform)
            pizzas.Add(t);

        foreach (Transform pizza in pizzas)
        {
            string tag = pizza.tag;
            Plate target = GetNearbyValidTarget(source, tag);

            if (target != null && target != source && !target.IsFull())
            {
                MovePizza(pizza, source, target);
            }
        }
    }

    Plate GetNearbyValidTarget(Plate source, string tag)
    {
        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        foreach (Vector3 dir in dirs)
        {
            Collider[] hits = Physics.OverlapSphere(source.transform.position + dir, 0.2f);

            foreach (Collider hit in hits)
            {
                Plate other = hit.GetComponent<Plate>();
                if (other != null && other.isPlaced && !other.IsFull())
                {
                    // ✅ Empty plate only valid for mixed source
                    if (other.transform.childCount == 0 && source.IsMixed())
                        return other;

                    // ✅ If plate has only one pizza, lock to that type
                    if (other.transform.childCount == 1)
                    {
                        string lockedTag = other.transform.GetChild(0).tag;
                        if (tag != lockedTag) continue;

                        // ✅ Single + Single → newer single wins
                        if (source.transform.childCount == 1 && other.transform.childCount == 1)
                        {
                            if (other.placedTimer < source.placedTimer)
                                return other; // newer wins
                            else
                                continue;
                        }

                        return other; // same type → accept
                    }

                    // ✅ If plate is pure (all same type), also lock to that type
                    if (other.IsPure())
                    {
                        string lockedTag = other.transform.GetChild(0).tag;
                        if (tag != lockedTag) continue;

                        // ✅ Pure + Pure → newer pure wins
                        if (source.IsPure())
                        {
                            if (other.placedTimer < source.placedTimer)
                                return other; // newer wins
                            else
                                continue;
                        }

                        return other;
                    }

                    // ✅ Mixed → Mixed allowed
                    if (source.IsMixed() && other.IsMixed() && other.HasPizza(tag))
                        return other;

                    // ✅ Normal same-type rule
                    if (other.HasPizza(tag))
                        return other;
                }
            }
        }

        return null;
    }

    void MovePizza(Transform pizza, Plate source, Plate target)
    {
        activeMoves++; // lock destroy system

        PizzaMover.instance.MovePizza(
            pizza,
            source.transform.position,
            target.transform.position,
            source,
            target,
            () => { activeMoves--; } // unlock destroy system after animation
        );
    }

    void ClearEmptyPlates(Plate[] plates)
    {
        if (activeMoves > 0) return; // skip while pizzas are moving

        foreach (Plate p in plates)
        {
            if (p == null || !p.isPlaced || p.transform.childCount > 0)
                continue;

            bool keep = false;
            Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            foreach (Vector3 dir in dirs)
            {
                Collider[] hits = Physics.OverlapSphere(p.transform.position + dir, 0.2f);
                foreach (Collider hit in hits)
                {
                    Plate neighbor = hit.GetComponent<Plate>();
                    if (neighbor != null && neighbor.isPlaced && neighbor.IsMixed())
                    {
                        keep = true;
                        break;
                    }
                }
                if (keep) break;
            }

            if (!keep)
            {
                Debug.Log($"Removed empty plate: {p.name}");
                Destroy(p.gameObject);
            }
        }
    }
}
