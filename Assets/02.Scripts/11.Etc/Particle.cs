using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem system;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyParticle());
    }

    private IEnumerator DestroyParticle()
    {
        yield return new WaitForSeconds(system.main.duration);
        Destroy(gameObject);
    }
}
