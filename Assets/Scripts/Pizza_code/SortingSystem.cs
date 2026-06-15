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
        // Start sorting when entering Sorting state
        if (GameManager.Instance.CurrentState == GameState.Sorting && !isSorting && activeMoves == 0)
        {
            isSorting = true;
            StartCoroutine(SortLoop());
        }

        // Reset flag when returning to Playing
        if (GameManager.Instance.CurrentState == GameState.Playing && isSorting)
        {
            isSorting = false;
        }

        int placedCount = FindObjectsOfType<Plate>().Count(p => p.isPlaced);
        if (placedCount >= 24)
        {
            bool anyClearing = FindObjectsOfType<Plate>().Any(p => p.isPlaced && p.isClearing);
            if (!anyClearing && GameManager.Instance.CurrentState == GameState.Playing)
            {
                StartCoroutine(CheckGameOverAfterDelay());
            }
        }
    }

    IEnumerator SortLoop()
    {
        ComboStreakSystem.instance.StartCycle();

        var placedPlates = FindObjectsOfType<Plate>()
            .Where(p => p != null && p.isPlaced)
            .OrderBy(p => p.placedTimer)
            .ToList();

        if (placedPlates.Count == 0)
        {
            isSorting = false;
            GameManager.Instance.ChangeState(GameState.Playing);
            ComboStreakSystem.instance.EndCycle();
            yield break;
        }

        Plate earliestPlate = placedPlates.First();
        var neighbors = ScanNeighbors(earliestPlate).Where(n => n.isPlaced).ToList();

        if (neighbors.Count == 0)
        {
            isSorting = false;
            GameManager.Instance.ChangeState(GameState.Playing);
            ComboStreakSystem.instance.EndCycle();
            Debug.Log("Earliest plate has no neighbors, returning to Playing");
            yield break;
        }

        while (true)
        {
            placedPlates = placedPlates
                .Where(p => p != null)
                .OrderBy(p => p.IsPure() ? 0 : 1)
                .ThenBy(p => p.placedTimer)
                .ToList();

            var plannedMoves = new List<(Transform pizza, Plate source, Plate target)>();

            foreach (var plate in placedPlates)
                if (plate != null)
                    PlanMoves(plate, plannedMoves);

            if (plannedMoves.Count == 0)
            {
                ClearEmptyPlates(placedPlates);
                break;
            }

            foreach (var move in plannedMoves)
                if (move.target != null && !move.target.IsFull())
                    MovePizza(move.pizza, move.source, move.target);

            yield return new WaitUntil(() => activeMoves == 0);

            ClearEmptyPlates(placedPlates);

            yield return null;
            placedPlates = FindObjectsOfType<Plate>()
                .Where(p => p != null && p.isPlaced)
                .OrderBy(p => p.placedTimer)
                .ToList();

            yield return new WaitForSeconds(0.2f);
        }

        isSorting = false;
        GameManager.Instance.ChangeState(GameState.Playing);
        ComboStreakSystem.instance.EndCycle();
        Debug.Log("Sorting finished, returning to Playing");
    }

    void PlanMoves(Plate source, List<(Transform, Plate, Plate)> plannedMoves)
    {
        if (source == null || !source.isPlaced || source.isClearing) return;
        if (!source.GetPizzas().Any()) return;

        var neighbors = ScanNeighbors(source).Where(n => n != null && n.isPlaced).ToList();

        // Mixed plate with 2+ pure neighbors
        if (source.IsMixed())
        {
            var pureNeighbors = neighbors.Where(n => n.IsPure()).ToList();
            if (pureNeighbors.Count >= 2)
            {
                var mixTags = source.GetPizzas().Select(p => p.tag).Distinct().ToList();

                foreach (var tag in mixTags)
                {
                    var matchingPure = pureNeighbors.Where(p => p.HasPizza(tag)).ToList();
                    if (matchingPure.Count >= 2)
                    {
                        Plate lowerPure = matchingPure.OrderBy(p => p.placedTimer).First();
                        Plate higherPure = matchingPure.OrderBy(p => p.placedTimer).Last();

                        foreach (Transform pizza in source.GetPizzas())
                            if (pizza.CompareTag(tag) && !lowerPure.IsFull())
                                plannedMoves.Add((pizza, source, lowerPure));

                        foreach (Transform pizza in higherPure.GetPizzas())
                            if (pizza.CompareTag(tag) && !source.IsFull())
                                plannedMoves.Add((pizza, higherPure, source));
                    }
                }
            }
        }

        // Pure rules
        if (source.IsPure())
        {
            string pureTag = source.GetPizzas().First().tag;

            foreach (var neighbor in neighbors.Where(n => n.IsMixed()))
            {
                foreach (Transform pizza in neighbor.GetPizzas())
                    if (pizza.CompareTag(pureTag) && !source.IsFull())
                        plannedMoves.Add((pizza, neighbor, source));
            }

            foreach (var neighbor in neighbors.Where(n => n.IsPure() && n.HasPizza(pureTag)))
            {
                Plate target = (source.placedTimer < neighbor.placedTimer) ? source : neighbor;
                Plate donor = (target == source) ? neighbor : source;

                foreach (Transform pizza in donor.GetPizzas())
                    if (pizza.CompareTag(pureTag) && !target.IsFull())
                        plannedMoves.Add((pizza, donor, target));
            }
            return;
        }

        // Mix vs mix
        if (source.IsMixed())
        {
            foreach (var neighbor in neighbors.Where(n => n.IsMixed()))
            {
                var sourceTags = source.GetPizzas().Select(p => p.tag).Distinct();
                var neighborTags = neighbor.GetPizzas().Select(p => p.tag).Distinct();
                var commonTags = sourceTags.Intersect(neighborTags).ToList();

                Plate target = (source.placedTimer < neighbor.placedTimer) ? source : neighbor;
                Plate donor = (target == source) ? neighbor : source;

                foreach (var tag in commonTags)
                {
                    foreach (Transform pizza in donor.GetPizzas())
                        if (pizza.CompareTag(tag) && !target.IsFull())
                            plannedMoves.Add((pizza, donor, target));
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
        if (source == target) return;
        if (target == null || target.IsFull()) return;

        activeMoves++;

        PizzaMover.instance.MovePizza(
            pizza,
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
                p.hasCleared = true;

                GameManager.Instance.AddScore(10);
                SpawnExplosion(p.transform.position);

                var textObj = PoolManager.Instance.scoreTextPool.Get();
                textObj.transform.SetParent(PoolManager.Instance.worldCanvas.transform, false);
                textObj.transform.position = p.transform.position + Vector3.up * 0.5f;
                textObj.GetComponent<FloatingText>().Show("+10");

                var anim = p.GetComponent<PlateClearAnimation>();
                if (anim != null) anim.PlayClearAnimation();
                else Destroy(p.gameObject);

                // ✅ Register the clear here
                ComboStreakSystem.instance.RegisterPlateClear(p);
            }
            else if (!p.GetPizzas().Any())
            {
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
