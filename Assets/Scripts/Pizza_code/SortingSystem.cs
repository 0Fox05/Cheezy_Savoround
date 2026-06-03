using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SortingSystem : MonoBehaviour
{
    public static SortingSystem instance;
    private int activeMoves = 0;
    private bool isSorting = false;

    private void Awake() => instance = this;

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Sorting && !isSorting && activeMoves == 0)
        {
            isSorting = true;
            StartCoroutine(SortLoop());
        }

        // ✅ Check if 24 plates are placed
        int placedCount = FindObjectsOfType<Plate>().Count(p => p.isPlaced);
        if (placedCount >= 24)
        {
            // 👇 new guard: skip if any plate is still clearing
            bool anyClearing = FindObjectsOfType<Plate>().Any(p => p.isPlaced && p.isClearing);

            if (!anyClearing && GameManager.Instance.CurrentState == GameState.Playing)
            {
                StartCoroutine(CheckGameOverAfterDelay());
            }
        }
    }

    IEnumerator SortLoop()
    {
        var placedPlates = FindObjectsOfType<Plate>()
            .Where(p => p != null && p.isPlaced)
            .OrderBy(p => p.placedTimer)
            .ToList();

        if (placedPlates.Count == 0)
        {
            isSorting = false;
            GameManager.Instance.ChangeState(GameState.Playing);
            yield break;
        }

        Plate earliestPlate = placedPlates.First();
        var neighbors = ScanNeighbors(earliestPlate).Where(n => n.isPlaced).ToList();

        if (neighbors.Count == 0)
        {
            isSorting = false;
            GameManager.Instance.ChangeState(GameState.Playing);
            Debug.Log("Earliest plate has no neighbors, returning to Playing");
            yield break;
        }

        while (true)
        {
            placedPlates = placedPlates
                .Where(p => p != null) // ✅ filter destroyed plates
                .OrderBy(p => p.IsPure() ? 0 : 1)
                .ThenBy(p => p.placedTimer)
                .ToList();

            var plannedMoves = new List<(Transform pizza, Plate source, Plate target)>();

            foreach (var plate in placedPlates)
                if (plate != null)
                    PlanMoves(plate, plannedMoves);

            if (plannedMoves.Count == 0) break;

            foreach (var move in plannedMoves)
                if (move.target != null && !move.target.IsFull())
                    MovePizza(move.pizza, move.source, move.target);

            yield return new WaitUntil(() => activeMoves == 0);

            // ✅ Safely clear and refresh plates after destruction
            ClearEmptyPlates(placedPlates);
            yield return null; // wait one frame so Destroy() completes
            placedPlates = FindObjectsOfType<Plate>()
                .Where(p => p != null && p.isPlaced)
                .OrderBy(p => p.placedTimer)
                .ToList();

            yield return new WaitForSeconds(0.2f);
        }

        isSorting = false;
        ClearEmptyPlates(FindObjectsOfType<Plate>());
        GameManager.Instance.ChangeState(GameState.Playing);
        Debug.Log("Sorting finished, cleared plates, returning to Playing");
    }

    void PlanMoves(Plate source, List<(Transform, Plate, Plate)> plannedMoves)
    {
        if (source == null || !source.isPlaced || source.isClearing) return;
        if (source.transform.childCount == 0) return;

        var neighbors = ScanNeighbors(source).Where(n => n != null && n.isPlaced).ToList();

        // ✅ NEW RULE: Mixed plate with 2+ pure neighbors
        if (source.IsMixed())
        {
            var pureNeighbors = neighbors.Where(n => n.IsPure()).ToList();
            if (pureNeighbors.Count >= 2)
            {
                // Find common tags between mix and pure neighbors
                var mixTags = source.transform.Cast<Transform>().Select(p => p.tag).Distinct().ToList();

                foreach (var tag in mixTags)
                {
                    var matchingPure = pureNeighbors.Where(p => p.HasPizza(tag)).ToList();
                    if (matchingPure.Count >= 2)
                    {
                        // Pick lower placedTimer pure plate
                        Plate lowerPure = matchingPure.OrderBy(p => p.placedTimer).First();
                        Plate higherPure = matchingPure.OrderBy(p => p.placedTimer).Last();

                        // Mixed gives pizzas of tag to lower pure
                        foreach (Transform pizza in source.transform)
                        {
                            if (pizza.CompareTag(tag) && !lowerPure.IsFull())
                                plannedMoves.Add((pizza, source, lowerPure));
                        }

                        // Higher pure sends pizzas of tag to mix
                        foreach (Transform pizza in higherPure.transform)
                        {
                            if (pizza.CompareTag(tag) && !source.IsFull())
                                plannedMoves.Add((pizza, higherPure, source));
                        }

                        // Then mix forwards them to lower pure (respect capacity)
                        foreach (Transform pizza in source.transform)
                        {
                            if (pizza.CompareTag(tag) && !lowerPure.IsFull())
                                plannedMoves.Add((pizza, source, lowerPure));
                        }
                    }
                }
            }
        }

        // Existing PURE RULES
        if (source.IsPure())
        {
            string pureTag = source.transform.GetChild(0).tag;

            foreach (var neighbor in neighbors.Where(n => n.IsMixed()))
            {
                foreach (Transform pizza in neighbor.transform)
                {
                    if (pizza.CompareTag(pureTag) && !source.IsFull() && neighbor != source)
                    {
                        plannedMoves.Add((pizza, neighbor, source));
                    }
                }
            }

            foreach (var neighbor in neighbors.Where(n => n.IsPure() && n.HasPizza(pureTag)))
            {
                Plate target = (source.placedTimer < neighbor.placedTimer) ? source : neighbor;
                Plate donor = (target == source) ? neighbor : source;

                if (donor == target) continue;

                foreach (Transform pizza in donor.transform)
                {
                    if (pizza.CompareTag(pureTag) && !target.IsFull())
                    {
                        plannedMoves.Add((pizza, donor, target));
                    }
                }
            }
            return;
        }

        // Existing MIX RULES (mix vs mix)
        if (source.IsMixed())
        {
            foreach (var neighbor in neighbors.Where(n => n.IsMixed()))
            {
                var sourceTags = source.transform.Cast<Transform>().Select(p => p.tag).Distinct();
                var neighborTags = neighbor.transform.Cast<Transform>().Select(p => p.tag).Distinct();
                var commonTags = sourceTags.Intersect(neighborTags).ToList();

                if (commonTags.Count == 0) continue;

                Plate target = (source.placedTimer < neighbor.placedTimer) ? source : neighbor;
                Plate donor = (target == source) ? neighbor : source;

                if (donor == target) continue;

                foreach (var tag in commonTags)
                {
                    foreach (Transform pizza in donor.transform)
                    {
                        if (pizza.CompareTag(tag) && !target.IsFull())
                        {
                            plannedMoves.Add((pizza, donor, target));
                        }
                    }
                }
            }
        }
    }
    List<Plate> ScanNeighbors(Plate source)
    {
        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        var neighbors = new List<Plate>();
        foreach (var dir in dirs)
        {
            foreach (var hit in Physics.OverlapSphere(source.transform.position + dir, 0.19f))
            {
                var other = hit.GetComponent<Plate>();
                if (other != null && other.isPlaced) neighbors.Add(other);
            }
        }
        return neighbors;
    }

    void MovePizza(Transform pizza, Plate source, Plate target)
    {
        // Don’t animate if source and target are the same
        if (source == target) return;

        activeMoves++;
        PizzaMover.instance.MovePizza(
            pizza,
            source.transform.position,
            target.transform.position,
            source,
            target,
            () => { activeMoves--; }
        );
    }

    void ClearEmptyPlates(IEnumerable<Plate> plates)
    {
        if (activeMoves > 0) return;

        foreach (var p in plates)
        {
            if (p == null || !p.isPlaced) continue;

            if (p.IsFull() && p.IsPure() && !p.hasCleared)
            {
                p.hasCleared = true; // ✅ prevents double scoring

                GameManager.Instance.AddScore(10);
                Debug.Log("add 10 score");

                SpawnExplosion(p.transform.position);

                var textObj = PoolManager.Instance.scoreTextPool.Get();
                textObj.transform.SetParent(PoolManager.Instance.worldCanvas.transform, false);
                textObj.transform.position = p.transform.position + Vector3.up * 0.5f;
                textObj.GetComponent<FloatingText>().Show("+10");

                var anim = p.GetComponent<PlateClearAnimation>();
                if (anim != null) anim.PlayClearAnimation();
                else Destroy(p.gameObject);

                continue;
            }

            if (p.transform.childCount == 0)
            {
                Debug.Log("Removed empty plate");

                // ❌ No text here, just animation
                var anim = p.GetComponent<PlateClearAnimation>();
                if (anim != null) anim.PlayClearAnimation();
                else Destroy(p.gameObject);
            }
        }
    }

    void SpawnExplosion(Vector3 pos)
    {
        GameObject explosion = PoolManager.Instance.explosionPool.Get();
        explosion.transform.position = pos;
        // rotation optional
        explosion.transform.rotation = Quaternion.identity;
    }


    IEnumerator CheckGameOverAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.ChangeState(GameState.GameOver);
            Debug.Log("Game Over: 24 plates placed and no moves left.");
        }
    }
}
