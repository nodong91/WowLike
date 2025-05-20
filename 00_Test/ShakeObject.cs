using UnityEngine;

public class ShakeObject : MonoBehaviour
{
    public float shakeAmount;
    public float rotateAmount;
    public float duration;
    public GameObject shakeTarget;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 randomPos = Random.insideUnitSphere * shakeAmount;
        shakeTarget.transform.position = randomPos;

        shakeTarget.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
