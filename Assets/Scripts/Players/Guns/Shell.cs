using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public float forceMin;
    public float forceMax;

    Rigidbody body;
    float lifeTime = 4;
    float fadeTime = 2;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        float force = Random.Range(forceMin, forceMax);
        body.AddForce(transform.right * force);
        body.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifeTime);

        float percent = 0;
        float fadeSpeed = 1 / fadeTime;
        Material mat = GetComponent<Renderer>().material;
        Color initial = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;

            mat.color = Color.Lerp(initial, Color.clear, percent);
            yield return null;
        }
        Destroy(gameObject);
    }
}
