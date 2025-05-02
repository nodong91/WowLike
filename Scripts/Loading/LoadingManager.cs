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
                asyncOperation.allowSceneActivation = true; //�� Ȱ��ȭ
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
        asyncOperation.allowSceneActivation = false; //�ε��� �Ϸ�Ǵ´�� ���� Ȱ��ȭ�Ұ�����

        while (asyncOperation.isDone == false)
        {
            //isDone�� �ε��� �Ϸ�Ǿ����� Ȯ���ϴ� ����
            print(asyncOperation.progress); //�ε��� �󸶳� �Ϸ�Ǿ����� 0~1�� ������ ������
            float progress = asyncOperation.progress / 0.9f;
            sliderImage.fillAmount = progress;
            if (progress >= 1f) //�ε� ���
            {
                isDone = true;
                asyncOperation.allowSceneActivation = true; //�� Ȱ��ȭ
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
