using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    LoadingManager Instance;
    float time = 0;
    bool isDone = false;
    AsyncOperation asyncOperation;
    public Image sliderImage;

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (isDone == false)
                LoadingScene("BattleTest");
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (isDone == true)
                asyncOperation.allowSceneActivation = true; //씬 활성화
        }
    }

    public void LoadingScene(string _scene)
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        //SceneManager.LoadSceneAsync(_scene);
        StartCoroutine(LoadingAsync(_scene, LoadSceneMode.Single));
    }

    IEnumerator LoadingAsync(string _scene, LoadSceneMode _loadMode)
    {
        asyncOperation = SceneManager.LoadSceneAsync(_scene, _loadMode);
        asyncOperation.allowSceneActivation = false; //로딩이 완료되는대로 씬을 활성화할것인지

        while (asyncOperation.isDone == false)
        {
            //isDone는 로딩이 완료되었는지 확인하는 변수
            print(asyncOperation.progress); //로딩이 얼마나 완료되었는지 0~1의 값으로 보여줌
            float progress = asyncOperation.progress / 0.9f;
            sliderImage.fillAmount = progress;
            if (progress >= 1f) //로딩 대기
            {
                isDone = true;
                asyncOperation.allowSceneActivation = true; //씬 활성화
            }
            //SceneManager.SetActiveScene(SceneManager.GetSceneByName(_scene));
            yield return null;
        }
    }

    //IEnumerator LoadSceneAsync(string[] _scene)
    //{
    //    loadingComplate = false;
    //    dele_OpenScreen?.Invoke(false);
    //    yield return null;

    //    while (loadingManager?.complate == false)
    //        yield return null;

    //    loadAsync = 0f;
    //    compAsync = (0.9f * _scene.Length) + 1f;
    //    string mainString = _scene[0];
    //    for (int i = 0; i < _scene.Length; i++)
    //    {
    //        LoadSceneMode sceneMode = (i == 0) ? LoadSceneMode.Single : LoadSceneMode.Additive;
    //        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_scene[i], sceneMode);
    //        asyncOperation.allowSceneActivation = false;
    //        yield return null;

    //        float asyncValue = 0f;
    //        while (asyncOperation.isDone == false)
    //        {
    //            asyncValue = asyncOperation.progress;
    //            progress = (loadAsync + asyncValue) / compAsync * 100f;
    //            Debug.Log(_scene[i] + " : " + progress);

    //            if (asyncOperation.progress >= 0.9f)
    //                asyncOperation.allowSceneActivation = true;
    //            yield return null;
    //        }
    //        loadAsync += asyncValue;
    //        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainString));
    //    }
    //    yield return new WaitForSeconds(1f);
    //    loadingComplate = true;
    //    dele_OpenScreen?.Invoke(true);
    //}
}
