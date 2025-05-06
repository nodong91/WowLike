using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StartMenu : MonoBehaviour
{
    public CanvasGroup unitCanvas;
    public Button startButton;
    public Button[] unitButton;
    public Data_Manager.UnitStruct[] unitStructs;
    public TMPro.TMP_Text unitName;
    public UI_StartSet[] menuToggle;

    void Start()
    {
        RandomUnit();
        TestTest();
        for (int i = 0; i < unitButton.Length; i++)
        {
            int index = i;
            unitButton[i].onClick.AddListener(delegate { SelectUnit(index); });
        }

        for (int i = 0; i < menuToggle.Length; i++)
        {
            int index = i;
            menuToggle[i].toggle.onValueChanged.AddListener(delegate
            {
                SelectToggle(index);
            });
        }

        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(Exit);
    }

    void RandomUnit()
    {
        List<string> strings = new List<string>();
        foreach (var child in Singleton_Data.INSTANCE.Dict_Unit)
        {
            strings.Add(child.Key);
        }
        strings = ShuffleList(strings, 0);
        unitStructs = new Data_Manager.UnitStruct[unitButton.Length];
        for (int i = 0; i < unitStructs.Length; i++)
        {
            string randomID = strings[i];
            Data_Manager.UnitStruct temp = Singleton_Data.INSTANCE.Dict_Unit[randomID];
            unitStructs[i] = temp;
        }
    }

    // ¸®½ºÆ® ¼¯±â
    public static List<T> ShuffleList<T>(List<T> _list, int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < _list.Count - 1; i++)
        {
            int randomIndex = prng.Next(i, _list.Count);
            T tempItem = _list[randomIndex];
            _list[randomIndex] = _list[i];
            _list[i] = tempItem;
        }
        return _list;
    }

    void SelectUnit(int _index)
    {
        unitName.text = unitStructs[_index].unitName;
    }

    void SelectToggle(int index)
    {
        menuToggle[index].OpenCanvas();
        //Toggle toggle = toggleGroup.GetFirstActiveToggle();
        //Debug.LogWarning(toggle.name);
    }

    public CanvasGroup skillCanvas;
    public Button exitButton;
    private void Exit()
    {
        skillCanvas.alpha = 0;
        skillCanvas.interactable = false;
        skillCanvas.blocksRaycasts = false;
    }

    void StartGame()
    {
        unitCanvas.alpha = 0;
        unitCanvas.interactable = false;
        unitCanvas.blocksRaycasts = false;
    }












    public List<Data_Manager.SkillStruct> skillStruct = new List<Data_Manager.SkillStruct>();
    public List<Data_Manager.ItemStruct> itemStruct = new List<Data_Manager.ItemStruct>();
    public List<Data_Manager.UnitStruct> unitStruct = new List<Data_Manager.UnitStruct>();


    void TestTest()
    {
        skillStruct = new List<Data_Manager.SkillStruct>();
        itemStruct = new List<Data_Manager.ItemStruct>();
        unitStruct = new List<Data_Manager.UnitStruct>();
        foreach (var child in Singleton_Data.INSTANCE.Dict_Unit)
        {
            SetSlotTest(child.Key);
        }
    }

    public void SetSlotTest(string _id)
    {
        if (_id[0] == 'S')
        {

        }
        else if (_id[0] == 'T')
        {

        }
        else if (_id[0] == 'U')
        {
            Data_Manager.UnitStruct unit = Singleton_Data.INSTANCE.Dict_Unit[_id];
            unitStruct.Add(unit);
        }
    }
}
