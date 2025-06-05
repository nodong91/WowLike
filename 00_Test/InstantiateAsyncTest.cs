using UnityEngine;
using UnityEngine.AddressableAssets;

public class InstantiateAsyncTest : MonoBehaviour
{
    public Light directionLight;

    void Start()
    {
        directionLight = FindDirectionalLight();
        // 캐릭터등 미리 생성해 놓을 수 있는 오브젝트 비동기화 인스턴싱 테스트
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
        /* GameObject는 Load 보다는 InstantiateAysnc로 직접 생성해야 한다. */
        //Addressables.LoadAssetAsync<GameObject>(addressableName);
        Addressables.InstantiateAsync(testPath + "/" + addressableName);
    }

    public AssetLabelReference assetLabel;

    public void LoadPrefab_Label()
    {
        // 1안, 직접입력
        Addressables.InstantiateAsync("SomeLabel");
        // 2안, AssetLabelReference 사용
        Addressables.InstantiateAsync(assetLabel.labelString);
        // 3안, 경로 얻어와서 로드하기
        Addressables.LoadResourceLocationsAsync("SomeLabel").Completed +=
            (handle) =>
            {
                var locations = handle.Result;
                Addressables.InstantiateAsync(locations[0]);
            };
    }
}
