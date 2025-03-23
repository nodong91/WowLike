using System;
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

    public Follow_Target followTarget;
    public Follow_HP followHP;
    public RectTransform followParent;
    Follow_Manager followManager;

    public Follow_HP AddFollow_Unit(Unit_AI _unit)
    {
        Follow_HP instHP = Instantiate(followHP, followParent);
        instHP.SetUnit = _unit;
        instHP.followType = Follow_Target.FollowType.Camera;
        instHP.followOffset = new Vector3(0f, 1f, 0f);
        followManager = GetComponent<Follow_Manager>();
        followManager.AddFollowUI(_unit.gameObject, instHP);

        return instHP;
    }

    public void AddFollow(GameObject _target)
    {
        Follow_Target instTarget = Instantiate(followTarget, followParent);
        instTarget.followType = Follow_Target.FollowType.Camera;
        instTarget.followOffset = new Vector3(0f, 1f, 0f);
        followManager = GetComponent<Follow_Manager>();
        followManager.AddFollowUI(_target.gameObject, instTarget);
    }

    public void ShakingUI(GameObject _target)
    {
        followManager.ShakingUI(_target);
    }
}
