using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UI;

public class UI_SlotInfo : MonoBehaviour, IPointerClickHandler
{
    public RectTransform rect;
    public Camera mainCamera;
    public Vector2 point;
    public TMPro.TMP_Text typeText, nameText;
    public Image iconImage;

    void Start()
    {
        mainCamera = Camera.main;
        gameObject.SetActive(false);
    }

    void Update()
    {
        //OnInfomation();
    }

    public void OnInfomation(UI_InvenSlot _slot)
    {
        if (_slot.itemType == UI_InvenSlot.ItemType.Empty)
            return;

        string nameString = "";
        Sprite sprite = null;
        switch (_slot.itemType)
        {
            case UI_InvenSlot.ItemType.Item:
                nameString = Singleton_Data.INSTANCE.TryTranslation(1, _slot.itemStruct.itemName);
                sprite = _slot.itemStruct.itemIcon;
                break;

            case UI_InvenSlot.ItemType.Skill:
                nameString = Singleton_Data.INSTANCE.TryTranslation(1, _slot.skillStruct.skillName);
                sprite = _slot.skillStruct.icon;
                break;

            case UI_InvenSlot.ItemType.Unit:
                nameString = Singleton_Data.INSTANCE.TryTranslation(1, _slot.unitStruct.unitName);
                sprite = _slot.unitStruct.unitIcon;
                break;
        }
        typeText.text = _slot.itemType.ToString();
        nameText.text = nameString;
        iconImage.sprite = sprite;
        gameObject.SetActive(true);
        rect.position = Input.mousePosition;
        point = mainCamera.ScreenToViewportPoint(Input.mousePosition);
        rect.pivot = point;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
    }
}
