using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishingGame : MonoBehaviour
{
    public Image test;
    public Transform fish;
    [Range(0f, 1f)]
    public float fillAmount;
    public float rotateAngle;
    public float target;

    void Start()
    {
        StartCoroutine(FishPosition());
    }
    public float result, result1;
    public bool asdfadf;
    void Update()
    {
        test.material.SetFloat("_FillAmount", fillAmount);
        rotateAngle = 180f * fillAmount;
        test.material.SetFloat("_RotateAngle", rotateAngle + 180f);
        //fillAmount -= Time.deltaTime * 0.1f;

        //fish.rotation = Quaternion.Slerp(fish.rotation, Quaternion.Euler(0, 0, target), 0.1f);

        result = Mathf.Abs((test.transform.rotation.eulerAngles.z - fish.rotation.eulerAngles.z) % 360f);
        result1 = Mathf.Abs((result + rotateAngle*2f) % 360f);
        asdfadf = (result > rotateAngle || result1 < rotateAngle);
        if (result > fillAmount)
        {

        }
    }

    IEnumerator FishPosition()
    {
        while (true)
        {
            float aaa = Random.Range(-100, 100f);
            target += aaa;
            float random = Random.Range(1f, 3f);
            yield return new WaitForSeconds(random);
        }
    }
}