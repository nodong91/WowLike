using UnityEngine;
using System.Collections;

public class ShakeObject : MonoBehaviour
{
    public float shakeAmount;
    public float rotateSpeed;
    public float duration;
    public GameObject shakeTarget;

    private void Start()
    {
        StartCoroutine(ShakingObject());
    }

    IEnumerator ShakingObject()
    {
        float delay = 0f;
        float axisY = 0f;
        bool shake = true;
        Vector3 randomPos = Random.insideUnitSphere * shakeAmount;
        Vector3 currentPos = shakeTarget.transform.position;
        while (shake == true)
        {
            delay += Time.deltaTime / duration;
            shakeTarget.transform.position = Vector3.Lerp(currentPos, randomPos, delay);
            axisY += Time.deltaTime * rotateSpeed;
            shakeTarget.transform.rotation = Quaternion.Euler(0f, axisY, 0f);
            if (delay > 1f)
            {
                delay = 0f;
                randomPos = Random.insideUnitSphere * shakeAmount;
                currentPos = shakeTarget.transform.position;
            }
            yield return null;
        }
    }
}
