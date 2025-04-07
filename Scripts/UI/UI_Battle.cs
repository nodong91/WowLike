using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UI_Battle : MonoBehaviour
{
    public Camera UICamera;
    public UI_OpenCanvas menuCanvas;
    public Button[] timeScaleButtons;
    public Button battleStart, invenButton, quickButton, allButton, lootingButton;
    public delegate void DeleTimeScale(float timeScale);
    public DeleTimeScale deleTimeScale;
    public Transform overlayCanvas, cameraCanvas;
    public UI_Inventory inventory;
    UI_Inventory instInventory;
    public UI_Inventory GetInventory { get { return instInventory; } }

    public Follow_Target followTarget;
    public Follow_HP followHP;
    public RectTransform followParent;
    public Follow_Manager followManager;
    private Queue<Follow_HP> followHPQueue = new Queue<Follow_HP>();
    private Queue<Follow_Target> followQueue = new Queue<Follow_Target>();

    public delegate void DeleBattleStart();
    public DeleBattleStart deleBattleStart;

    void Start()
    {
        SetUICamera();

        timeScaleButtons[0].onClick.AddListener(delegate { SetTimeScale(0f); });
        timeScaleButtons[1].onClick.AddListener(delegate { SetTimeScale(1f); });
        timeScaleButtons[2].onClick.AddListener(delegate { SetTimeScale(3f); });
        timeScaleButtons[3].onClick.AddListener(delegate { SetTimeScale(5f); });

        battleStart.onClick.AddListener(BattleStart);
        invenButton.onClick.AddListener(OpenInventory);
        quickButton.onClick.AddListener(OpenQuick);
        allButton.onClick.AddListener(CloseAll);
        lootingButton.onClick.AddListener(OpenLooting);

        instInventory = Instantiate(inventory, overlayCanvas);
        instInventory.SetInventory();
    }

    void SetUICamera()
    {
        Camera mainCamera = Camera.main;
        var cameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (cameraData.cameraStack.Contains(UICamera) == false)
        {
            UICamera.fieldOfView = mainCamera.fieldOfView;
            cameraData.cameraStack.Add(UICamera);
        }
    }

    void SetTimeScale(float _timeScale)
    {
        deleTimeScale(_timeScale);
    }

    void BattleStart()
    {
        deleBattleStart();
        CloseAll();
    }

    void OpenInventory()
    {
        instInventory.OpenInventory();
    }

    void OpenQuick()
    {
        instInventory.OpenQuick();
    }

    void OpenLooting()
    {
        instInventory.OpenLooting();
    }

    void OpenAll()
    {
        instInventory.OpenAllCanvas();
    }

    void CloseAll()
    {
        instInventory.CloseAllCanvas();
    }

    public Follow_HP AddFollow_Unit(Unit_AI _unit)
    {
        Follow_HP instHP = TryFollowHPTarget();
        instHP.SetFollowUnit(_unit);

        followManager.AddFollowUI(_unit.gameObject, instHP);

        return instHP;
    }

    Follow_HP TryFollowHPTarget()
    {
        if (followHPQueue.Count > 0)
        {
            Follow_HP follow = followHPQueue.Dequeue();
            follow.gameObject.SetActive(true);
            return follow;
        }
        Follow_HP instTarget = Instantiate(followHP, followParent);
        return instTarget;
    }

    public void RemoveFollowHP(GameObject _target)
    {
        Follow_HP instTarget = followManager.RemoveFollowHP(_target);
        instTarget.gameObject.SetActive(false);
        followHPQueue.Enqueue(instTarget);
    }

    public void AddFollow(GameObject _target)
    {
        Follow_Target instTarget = TryFollowTarget();
        followManager = GetComponent<Follow_Manager>();
        followManager.AddFollowUI(_target.gameObject, instTarget);
    }

    Follow_Target TryFollowTarget()
    {
        if (followQueue.Count > 0)
        {
            Follow_Target follow = followQueue.Dequeue();
            follow.gameObject.SetActive(true);
            return follow;
        }
        Follow_Target instTarget = Instantiate(followTarget, followParent);
        instTarget.SetFollow();

        return instTarget;
    }

    public void RemoveFollow(GameObject _target)
    {
        Follow_Target instTarget = followManager.RemoveFollowUI(_target);
        instTarget.gameObject.SetActive(false);
        followQueue.Enqueue(instTarget);
    }

    public void ShakingUI(GameObject _target)
    {
        followManager.ShakingUI(_target);
    }

    public TMPro.TMP_Text damageText;
    Queue<TMPro.TMP_Text> instDamage = new Queue<TMPro.TMP_Text>();
    public AnimationCurve textAction;

    public void DamageText(Vector3 _point, string _damage)
    {
        TMPro.TMP_Text instText = TryDamageText();
        instText.text = _damage;

        StartCoroutine(DamageTextAction(instText, _point));
    }

    IEnumerator DamageTextAction(TMPro.TMP_Text _text, Vector3 _point)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_point);
        Vector3 targetPosition = UICamera.ScreenToWorldPoint(screenPosition);
        float unitSize = 0.5f;
        Vector3 randomPosition = (Random.insideUnitSphere * unitSize) + targetPosition;
        //_text.color = Color.red;
        float normailze = 0f;
        while (normailze < 1f)
        {
            normailze += Time.deltaTime;
            float curve = textAction.Evaluate(normailze);
            _text.transform.position = Vector3.Lerp(randomPosition + Vector3.up, randomPosition, curve);
            //_text.transform.localScale = Vector3.one + Vector3.one * scale;
            _text.alpha = curve;
            yield return null;
        }
        instDamage.Enqueue(_text);
    }

    TMPro.TMP_Text TryDamageText()
    {
        if (instDamage.Count > 0)
        {
            return instDamage.Dequeue();
        }
        TMPro.TMP_Text inst = Instantiate(damageText, cameraCanvas);
        return inst;
    }
}
