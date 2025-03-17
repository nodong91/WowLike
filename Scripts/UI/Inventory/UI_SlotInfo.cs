using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SlotInfo : MonoBehaviour, IPointerClickHandler
{
    public RectTransform rect;
    public Vector2 point;
    public TMPro.TMP_Text typeText, nameText;
    public Image iconImage;
    public CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void OnInfomation(UI_InvenSlot _slot)
    {
        if (showInfo != null)
            StopCoroutine(showInfo);

        if (_slot == null || _slot.itemType == UI_InvenSlot.ItemType.Empty)
        {
            showInfo = StartCoroutine(OffInfomation());
            return;
        }
        showInfo = StartCoroutine(ShowInfomation(_slot));
    }
    Coroutine showInfo;
    IEnumerator ShowInfomation(UI_InvenSlot _slot)
    {
        yield return new WaitForSeconds(0.3f);

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

        rect.position = Input.mousePosition;
        point = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        rect.pivot = point;

        canvasGroup.alpha = 1f;
    }

    IEnumerator OffInfomation()
    {
        float normalize = 0f;
        while (normalize < 1f)
        {
            normalize += Time.deltaTime * 3f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, normalize);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }
}
