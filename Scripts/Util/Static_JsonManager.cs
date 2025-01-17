using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public class Static_JsonManager
{
    //======================================================================================
    // ���� ã��
    //======================================================================================

    static void FindFolder(string folderName)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(folderName);
        if (dirInfo.Exists == false)
        {
            // ������ �����
            dirInfo.Create();
        }
    }
    //======================================================================================
    // ä�� ��ġ ĳ����
    //======================================================================================
    public static void SaveRecruitmentStatus(string fileName, List<Data_RecruitmentStatus> recruitmentStatus)
    {
        string filePath = Application.dataPath + "/Save/";
        // ���� ������ ����
        FindFolder(filePath);

        string toJson = JsonHelper.ToJson(recruitmentStatus, prettyPrint: true);
        File.WriteAllText(filePath + fileName + ".json", toJson);
    }
    // �ҷ�����
    public static bool TryLoadRecruitmentStatus(string fileName, out List<Data_RecruitmentStatus> recruitmentStatus)
    {
        string filePath = Application.dataPath + "/Save/";
        string path = filePath + fileName + ".json";
        FileInfo fileInfo = new FileInfo(path);

        if (fileInfo.Exists == true)
        {
            string fromJson = File.ReadAllText(path);
            recruitmentStatus = JsonHelper.FromJson<Data_RecruitmentStatus>(fromJson);
            return true;
        }
        recruitmentStatus = default;
        return false;
    }
    //======================================================================================
    // ĳ���� ���� ����
    //======================================================================================

    // ���� ����
    public static void SaveCustomizeData(string fileName, List<CharInfo> charInfos)
    {
        string filePath = Application.dataPath + "/Save/";
        // ���� ������ ����
        FindFolder(filePath);

        string toJson = JsonHelper.ToJson(charInfos, prettyPrint: true);
        //toJson = Static_AES.Program.Encrypt(toJson, "StatusData");          // ��ȣȭ ����
        File.WriteAllText(filePath + fileName + ".json", toJson);
    }

    // �ҷ�����
    public static bool TryLoadCustomizeData(string fileName, out List<CharInfo> charInfos)
    {
        string filePath = Application.dataPath + "/Save/";
        string path = filePath + fileName + ".json";
        FileInfo fileInfo = new FileInfo(path);

        if (fileInfo.Exists == true)
        {
            string fromJson = File.ReadAllText(path);
            //fromJson = Static_AES.Program.Decrypt(fromJson, "StatusData");      // ��ȭ
            charInfos = JsonHelper.FromJson<CharInfo>(fromJson);
            return true;
        }

        charInfos = new List<CharInfo>();
        return false;
    }

    public static void RemoveCustomizeFile(string fileName)
    {
        File.Delete(Application.dataPath + "/Save/" + fileName + ".json");
    }

    ////======================================================================================
    //// ī�� ���� ����
    ////======================================================================================
    //// ����
    //public static void SaveCardData(string fileName, List<int> jsonCards)
    //{
    //    string filePath = Application.dataPath + "/Save/Save_Card/";
    //    // ���� ������ ����
    //    FindFolder(filePath);

    //    string toJson = JsonHelper.ToJson(jsonCards, prettyPrint: true);
    //    File.WriteAllText(filePath + fileName + ".json", toJson);
    //}

    //// �ҷ�����
    //public static bool TryLoadCardData(string fileName, out List<int> jsonCards)
    //{
    //    string filePath = Application.dataPath + "/Save/Save_Card/";
    //    string path = filePath + fileName + ".json";
    //    FileInfo fileInfo = new FileInfo(path);

    //    if (fileInfo.Exists == true)
    //    {
    //        string fromJson = File.ReadAllText(path);
    //        jsonCards = JsonHelper.FromJson<int>(fromJson);
    //        return true;
    //    }
    //    jsonCards = default;
    //    return false;
    //}

    //======================================================================================
    // �ɼ� ������ ����
    //======================================================================================

    public static void SaveOptionData(string fileName, Data_Option option)
    {
        string filePath = Application.dataPath + "/Save/";
        // ���� ����
        FindFolder(filePath);

        string toJson = JsonUtility.ToJson(option, prettyPrint: true);
        //toJson = Static_AES.Program.Encrypt(toJson, "SaveOptionData");          // ��ȣȭ ����
        File.WriteAllText(filePath + fileName + ".json", toJson);
    }

    public static bool TryLoadOptionData(string fileName, out Data_Option option)
    {
        string filePath = Application.dataPath + "/Save/";
        string path = filePath + fileName + ".json";
        FileInfo fileInfo = new FileInfo(path);

        if (fileInfo.Exists == true)
        {
            string fromJson = File.ReadAllText(path);
            //fromJson = Static_AES.Program.Decrypt(fromJson, "StatusData");      // ��ȭ
            option = JsonUtility.FromJson<Data_Option>(fromJson);
            return true;
        }

        option = default;
        return false;
    }
    //======================================================================================
    // �߰� ���̺� ������ ����
    //======================================================================================

    public static void SaveCountinueData(string fileName, Data_Countinue countinue)
    {
        string filePath = Application.dataPath + "/Save/";
        // ���� ����
        FindFolder(filePath);

        string toJson = JsonUtility.ToJson(countinue, prettyPrint: true);
        //toJson = Static_AES.Program.Encrypt(toJson, "SaveOptionData");          // ��ȣȭ ����
        File.WriteAllText(filePath + fileName + ".json", toJson);
    }

    public static bool TryLoadCountinueData(string fileName, out Data_Countinue countinue)
    {
        string filePath = Application.dataPath + "/Save/";
        string path = filePath + fileName + ".json";
        FileInfo fileInfo = new FileInfo(path);

        if (fileInfo.Exists == true)
        {
            string fromJson = File.ReadAllText(path);
            //fromJson = Static_AES.Program.Decrypt(fromJson, "SaveOptionData");      // ��ȭ
            countinue = JsonUtility.FromJson<Data_Countinue>(fromJson);
            return true;
        }

        countinue = default;
        return false;
    }
    //======================================================================================
    // �κ��丮 ����
    //======================================================================================
    //public static void SaveInventoryData(string fileName, Singleton_SaveData.SaveData _data)
    //{
    //    string filePath = Application.dataPath + "/Save/";
    //    // ���� ����
    //    FindFolder(filePath);

    //    string toJson = JsonUtility.ToJson(_data, prettyPrint: true);
    //    //toJson = Static_AES.Program.Encrypt(toJson, "SaveInventoryData");          // ��ȣȭ ����
    //    File.WriteAllText(filePath + fileName + ".json", toJson);
    //}

    //public static bool TryLoadInventoryData(string fileName, out Singleton_SaveData.SaveData _data)
    //{
    //    string filePath = Application.dataPath + "/Save/";
    //    string path = filePath + fileName + ".json";
    //    FileInfo fileInfo = new FileInfo(path);

    //    if (fileInfo.Exists == true)
    //    {
    //        string fromJson = File.ReadAllText(path);
    //        //fromJson = Static_AES.Program.Decrypt(fromJson, "SaveOptionData");      // ��ȭ
    //        _data = JsonUtility.FromJson<Singleton_SaveData.SaveData>(fromJson);
    //        return true;
    //    }
    //    _data = default;
    //    return false;
    //}
    //public static void SaveInventoryData(string fileName, List<Data_Inventory> inventory)
    //{
    //    string filePath = Application.dataPath + "/Save/";
    //    // ���� ����
    //    FindFolder(filePath);

    //    string toJson = JsonHelper.ToJson(inventory, prettyPrint: true);
    //    //toJson = Static_AES.Program.Encrypt(toJson, "SaveOptionData");          // ��ȣȭ ����
    //    File.WriteAllText(filePath + fileName + ".json", toJson);
    //}

    //public static bool TryLoadInventoryData(string fileName, out List<Data_Inventory> inventory)
    //{
    //    string filePath = Application.dataPath + "/Save/";
    //    string path = filePath + fileName + ".json";
    //    FileInfo fileInfo = new FileInfo(path);

    //    if (fileInfo.Exists == true)
    //    {
    //        string fromJson = File.ReadAllText(path);
    //        //fromJson = Static_AES.Program.Decrypt(fromJson, "SaveOptionData");      // ��ȭ
    //        inventory = JsonHelper.FromJson<Data_Inventory>(fromJson);
    //        return true;
    //    }

    //    inventory = default;
    //    return false;
    //}
    //======================================================================================
    // Json ����Ʈ ����� ����
    //======================================================================================

    public static class JsonHelper
    {
        public static List<T> FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(List<T> array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }
    }

    public class Wrapper<T>
    {
        public List<T> Items;
    }
}

//======================================================================================
// ����ȭ Ŭ����
//======================================================================================

[System.Serializable]
public struct CharInfo
{
    public int ID;
    public bool Sex;
    public string FullName;
    public int Rank;// ������ ���� ����

    public string HairColor;
    public string SkinColor;
    public List<int> Armors;
    public int Weapon;

    public List<int> Cards;
    public int inventoryAmount;
    public InventoryItem[] inventoryItems;
}

[System.Serializable]
public class Data_Countinue
{
    public List<CharInfo> TeamInfo;
    // �� ����
    public string MapDataName;
    public int MapSeed;
    public int FloorLevel;
    public List<int> ClearedBlocks;
    // ĳ���� ��ġ ����
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;
    // ĳ���� ����
    public int CharNumber;
    public int Hungry;
    //public CharacterStatus Status;
    // �κ��丮 ����
    //public List<ItemSlot> Items;
}

[System.Serializable]
public struct Data_Option
{
    // ���� ����
    public bool BGMMute;
    public float BGMVolume;
    public bool EffectMute;
    public float EffectVolume;
}

[System.Serializable]
public class Data_RecruitmentStatus
{
    public int gridX;
    public int gridY;
    public int charID;
}

[System.Serializable]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
    public InventoryItem(int id, int amount)
    {
        itemID = id;
        itemAmount = amount;
    }
}