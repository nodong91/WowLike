using UnityEngine;
using System.Collections;

public class Unit_Manager : Unit_Animation
{
    public Unit_Status status;
    Coroutine inputDirection;
    public float moveSpeed, maxSpeed = 1f;
    public float rotateSpeed = 10f;
    Vector3 direction;

    //Game_Manager.RotateType rotateType { get { return Game_Manager.instance.GetRotateType; } }
    public delegate Game_Manager.RotateType RotateType();
    public RotateType rotateType;

    void Start()
    {
        SetAnimator();
        rotateType = Game_Manager.instance.TryRotateType;
    }

    public void OutputDirection(Vector2 _direction)
    {
        if (inputDirection != null)
            StopCoroutine(inputDirection);
        inputDirection = StartCoroutine(Co_OutputDirection(_direction));
    }

    IEnumerator Co_OutputDirection(Vector2 _direction)
    {
        float targetSpeed = _direction == Vector2.zero ? 0f : 1f;
        float normalize = 0f;

        while (moveSpeed > 0 || _direction != Vector2.zero)
        {
            if (_direction != Vector2.zero)
            {
                Vector3 dir = new Vector3(_direction.x, 0, _direction.y);
                Vector3 temp = Camera.main.transform.TransformDirection(dir);
                direction = transform.position + new Vector3(temp.x, 0f, temp.z).normalized;
                Moving();
                Rotate();
            }
            yield return null;

            normalize += Time.deltaTime;
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, normalize);
            float x = rotateType() == Game_Manager.RotateType.Normal ? 1f : _direction.x;
            float y = rotateType() == Game_Manager.RotateType.Normal ? 0f : _direction.y;
            MoveAnimation(moveSpeed, x, y);
        }
    }

    void Moving()
    {
        float moveAmount = (rotateType() == Game_Manager.RotateType.Normal) ? maxSpeed : maxSpeed * 0.3f;
        float tempSpeed = moveSpeed * moveAmount;
        Vector3 movePoint = Vector3.Lerp(transform.position, direction, Time.deltaTime * tempSpeed);
        transform.position = movePoint;
        CheckDistance();
    }

    public void Rotate()
    {
        switch (rotateType())
        {
            case Game_Manager.RotateType.Normal:
                if (direction == Vector3.zero)
                    return;

                Vector3 offset = (direction - transform.position).normalized;
                Quaternion rotatePoint = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(offset), Time.deltaTime * rotateSpeed);
                transform.rotation = rotatePoint;
                break;

            case Game_Manager.RotateType.Focus:
                Vector3 temp = Camera.main.transform.TransformDirection(Vector3.forward);
                Vector3 front = transform.position + new Vector3(temp.x, 0f, temp.z).normalized;

                offset = (front - transform.position).normalized;
                rotatePoint = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(offset), Time.deltaTime * rotateSpeed);
                transform.rotation = rotatePoint;

                //if (Game_Manager.instance.inputMouseRight == false && Game_Manager.instance.inputDir != 0)
                //{
                //    rotateType = Game_Manager.RotateType.Normal;
                //}
                break;
        }
    }

    public float CheckDistance()
    {
        Transform target = Game_Manager.instance.GetTarget;
        if (target != null)
        {
            float targetDistance = Vector3.Distance(target.position, transform.position);
            UI_Manager.instance.CheckDistance(targetDistance);
            //UI_InvenSlot[] quickSlots = UI_Manager.instance.GetInventory.GetQuickSlot;
            //for (int i = 0; i < quickSlots.Length; i++)
            //{
            //    //Skill_Slot[] slotArray = UI_Manager.instance.slotArray;
            //    quickSlots[i].InDistance(quickSlots[i].skillStruct.distance > targetDistance);
            //}
            return targetDistance;
        }
        return 0f;
    }

}
