using System;
using System.Collections;
using UnityEngine;

public class ParabolaTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DrawParabolicTrajectory();
        }
    }
    public float initialSpeed = 10f;     // �ʱ� �ӵ�
    public float angle = 45f;            // �߻� ����
    void DrawParabolicTrajectory()
    {
        // �ʱ� �ӵ��� �߻� ������ �������� ��ȯ�մϴ�.
        float radians = angle * Mathf.Deg2Rad;

        // �ʱ� �ӵ��� x, y �������� �и��մϴ�.
        float initialVelocityX = initialSpeed * Mathf.Cos(radians);
        float initialVelocityY = initialSpeed * Mathf.Sin(radians);

        // �߷� ���ӵ��� �����ɴϴ�.
        float gravity = Mathf.Abs(Physics.gravity.y);

        // �ð� ����
        float timeStep = 0.02f;

        // �ʱ� ��ġ ����
        Vector3 currentPosition = transform.position;

        // ������ ���� �׸���
        for (float t = 0; t < 10f; t += timeStep)
        {
            float x = initialVelocityX * t;
            float y = (initialVelocityY * t) - (0.5f * gravity * t * t);

            // ���� �ð��� ���� ��ġ ���
            Vector3 newPosition = new Vector3(x, y, 0f);

            // ���� �� �׸���
            Debug.DrawLine(currentPosition, currentPosition + newPosition, Color.red, 1f);

            // ���� ��ġ ����
            currentPosition = currentPosition + newPosition;
        }
    }
}
