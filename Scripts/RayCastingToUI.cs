using UnityEngine;
using UnityEngine.EventSystems;

public class RayCastingToUI : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler
{
    public Vector3 ScreenToWorldPoint;
    public GameObject test;

    public void OnPointerMove(PointerEventData eventData)
    {
        Vector3 targetPoint = RayCasting();
        if (targetPoint != Vector3.zero)
        {
            Node getNode = Game_Manager.current.GetMapGenerator.GetNodeFromPosition(targetPoint);
            test.transform.position = getNode.worldPosition;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    Vector3 RayCasting()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << 0;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}
