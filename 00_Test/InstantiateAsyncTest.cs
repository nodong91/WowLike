using UnityEngine;
using UnityEngine.AddressableAssets;

public class InstantiateAsyncTest : MonoBehaviour
{
    public Light directionLight;

    void Start()
    {
        directionLight = FindDirectionalLight();
        // ĳ���͵� �̸� ������ ���� �� �ִ� ������Ʈ �񵿱�ȭ �ν��Ͻ� �׽�Ʈ
        //GameObject.InstantiateAsync
        LoadPrefab();
    }

    Light FindDirectionalLight()
    {
        Light[] getLight = FindObjectsByType<Light>(FindObjectsSortMode.InstanceID);
        for (int i = 0; i < getLight.Length; i++)
        {
            if (getLight[i].type == LightType.Directional)
                return getLight[i];
        }
        return null;
    }

    public string addressableName = "SomePrefab";
    const string prefabPath = "Assets/01_Resources/Prefabs";
    const string testPath = "Assets/00_Test/Quirky Series Ultimate";
    //public 
    public void LoadPrefab()
    {
        /* GameObject�� Load ���ٴ� InstantiateAysnc�� ���� �����ؾ� �Ѵ�. */
        //Addressables.LoadAssetAsync<GameObject>(addressableName);
        Addressables.InstantiateAsync(testPath + "/" + addressableName);
    }

    public AssetLabelReference assetLabel;

    public void LoadPrefab_Label()
    {
        // 1��, �����Է�
        Addressables.InstantiateAsync("SomeLabel");
        // 2��, AssetLabelReference ���
        Addressables.InstantiateAsync(assetLabel.labelString);
        // 3��, ��� ���ͼ� �ε��ϱ�
        Addressables.LoadResourceLocationsAsync("SomeLabel").Completed +=
            (handle) =>
            {
                var locations = handle.Result;
                Addressables.InstantiateAsync(locations[0]);
            };
    }
}
