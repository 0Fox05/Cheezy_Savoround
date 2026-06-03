using UnityEngine;
using System.Collections;

public class AutoReturnParticle : MonoBehaviour
{
    ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        StartCoroutine(ReturnRoutine());
    }

    IEnumerator ReturnRoutine()
    {
        // ⏱ Fixed wait time (e.g. 2 seconds)
        yield return new WaitForSeconds(2f);

        PoolManager.Instance.explosionPool.Return(gameObject);
    }
}
