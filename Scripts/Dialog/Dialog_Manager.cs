using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialog_Manager : MonoBehaviour
{
    public bool typing, actionText;
    public TMP_Text dialogText;
    public Data_DialogType dialogType;
    [System.Serializable]
    public class DialogText
    {
        [TextArea]
        public string text;
        public SubDialog[] subDialogs;
        [System.Serializable]
        public struct SubDialog
        {
            [TextArea]
            public string text;
            public Data_DialogType.TextStyle style;
            public int size;
            public string color;
            public bool bold;
            public float typingSpeed;
        }
    }
    const float defaultTypingSpeed = 0.1f;
    const string lineEnd = "`";

    public int defaultSize;
    public string defaultColor;
    public float typingSpeed;
    Coroutine typingCoroutine, actionCoroutine;

    public int dialogIndex;
    public float interval;
    public List<DialogText> dialogInfoamtions = new List<DialogText>();

    public Image nextMark;
    public Button button;
    [SerializeField] private List<Vector3Int> dialogActions = new List<Vector3Int>();

    void Start()
    {
        //Singleton_Audio.INSTANCE.Audio_SetBGM(BGMSound);
        typingSpeed = defaultTypingSpeed;
        button.onClick.AddListener(SetTest);
    }

    void SetTest()
    {
        if (typing == true)
        {
            StartTyping(false);// 스킵
        }
        else
        {
            nextMark.gameObject.SetActive(false);
            if (dialogIndex < dialogInfoamtions.Count)
            {
                StopAllCoroutines();
                StartCoroutine(SetText());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    IEnumerator SetText()
    {
        TMP_Text component = dialogText;
        component.text = GetText();
        dialogText.ForceMeshUpdate(true);// 메쉬 재 생성 (리셋)
        yield return new WaitForEndOfFrame();

        PreSetHide();// 글자 숨김
        SetActionRange();// 움직여야 할 글자 체크
        yield return new WaitForSeconds(defaultTypingSpeed);

        StartTyping(true);// 타이핑 시작
        StartAction();// 움직임 시작
    }
    //===========================================================================================================
    // 대화 세팅
    //===========================================================================================================
    string GetText()
    {
        string textStr = dialogInfoamtions[dialogIndex].text;
        DialogText.SubDialog[] subDialogs = dialogInfoamtions[dialogIndex].subDialogs;
        for (int i = 0; i < subDialogs.Length; i++)
        {
            string subText = subDialogs[i].text;
            string subColor = subDialogs[i].color;
            int subSize = subDialogs[i].size;
            bool subBold = subDialogs[i].bold;
            subText = SetSizeColor(subText, subSize, subColor, subBold);

            string temp = "{" + i + "}";
            textStr = textStr.Replace(temp, subText);
        }
        textStr = textStr.Replace(lineEnd, "\n");
        textStr = SetSizeColor(textStr, defaultSize, defaultColor, false);
        Debug.LogWarning(textStr);

        return textStr;
    }

    string SetSizeColor(string _text, int _size, string _color, bool _bold)
    {
        string temp = $"<color=#{_color}><size={_size}>{_text}</size></color>";
        if (_bold == true)
            temp = $"<b>{temp}</b>";
        return temp;
    }

    // 미리 세팅해놓고 숨기기
    void PreSetHide()
    {
        TMP_TextInfo textInfo = dialogText.textInfo;
        for (int c = 0; c < textInfo.characterCount; c++)
        {
            var charInfo = textInfo.characterInfo[c];
            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            int vertexIndex = charInfo.vertexIndex;
            for (int i = 0; i < 4; i++)
            {
                int index = vertexIndex + i;
                vertexColors[index].a = 0;// 투명화
            }
        }

        // 메쉬 업데이트
        for (int i = 0; i < textInfo.materialCount; i++)
        {
            if (textInfo.meshInfo[i].mesh == null) { continue; }
            textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;   // 변경
            dialogText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    void SetActionRange()
    {
        actionText = false;
        dialogActions.Clear();
        string textStr = dialogInfoamtions[dialogIndex].text;
        DialogText.SubDialog[] subDialogs = dialogInfoamtions[dialogIndex].subDialogs;
        for (int i = 0; i < subDialogs.Length; i++)
        {
            string temp = "{" + i + "}";
            string subText = subDialogs[i].text;

            int start = textStr.IndexOf(temp, 0, textStr.Length);// 시작 위치
            int end = start + subText.Length;// 끝 포지션
            int type = (int)subDialogs[i].style - 1;// 액션 스타일 (None 0 제거)
            if (type > 0)
                actionText = true;
            Vector3Int vector = new Vector3Int(start, end, type);
            dialogActions.Add(vector);

            textStr = textStr.Replace(temp, subText);// 변경
        }
    }

    //===========================================================================================================
    // 대화 시작
    //===========================================================================================================
    void StartTyping(bool _typing)
    {
        typing = _typing;
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(Typing());
    }

    void StartAction()
    {
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        actionCoroutine = StartCoroutine(TextAction());
    }

    IEnumerator Typing()
    {
        int subIndex = 0;
        TMP_TextInfo textInfo = dialogText.textInfo;
        for (int c = 0; c < textInfo.characterCount; c++)
        {
            DialogText.SubDialog[] sub = dialogInfoamtions[dialogIndex].subDialogs;
            if (sub.Length > 0)
            {
                // 타이핑 속도 조절
                float speed = sub[subIndex].typingSpeed;
                if (c == dialogActions[subIndex].x)
                {
                    if (speed > 0)// 타이핑 스피드가 0 이상이라면..
                        typingSpeed = speed;
                }

                if (c == dialogActions[subIndex].y)
                {
                    if (subIndex + 1 < sub.Length)
                    {
                        subIndex++;
                        speed = sub[subIndex].typingSpeed;
                    }
                    typingSpeed = defaultTypingSpeed;// 기본 속도
                }
            }

            var charInfo = textInfo.characterInfo[c];
            if (charInfo.isVisible == false)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            for (int i = 0; i < 4; i++)
            {
                int index = vertexIndex + i;
                vertexColors[index].a = (byte)255;// 활성화
            }
            if (typing == true)
            {
                Singleton_Audio.INSTANCE.Audio_SetFX(FXSound);
                dialogText.UpdateVertexData();
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        dialogText.UpdateVertexData();
        typing = false;
        typingSpeed = defaultTypingSpeed;// 기본 속도
        nextMark.gameObject.SetActive(true);

        StartCoroutine(WaitingNext());
    }


    IEnumerator WaitingNext()
    {
        dialogIndex++;

        float normalize = 0f;
        while (normalize < 1f && typing == false)
        {
            normalize += Time.deltaTime / 3f;
            nextMark.fillAmount = normalize;
            yield return null;
        }
    }
    public string BGMSound, FXSound;
    //===========================================================================================================
    // 텍스트 액션
    //===========================================================================================================
    IEnumerator TextAction()
    {
        TMP_Text component = dialogText;
        TMP_MeshInfo[] cachedMeshInfo = component.textInfo.CopyMeshInfoVertexData();

        while (actionText == true)
        {
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < dialogActions.Count; i++)
            {
                // x - 시작 포지션
                // y - 끝 포지션
                // z - 액션 타입
                if (dialogActions[i].z > -1)// 액션 타입이 None(-1)이 아니면
                {
                    for (int c = dialogActions[i].x; c < dialogActions[i].y; c++)
                    {
                        var charInfo = component.textInfo.characterInfo[c];
                        if (charInfo.isVisible == false)
                            continue;

                        int materialIndex = charInfo.materialReferenceIndex;
                        int vertexIndex = charInfo.vertexIndex;

                        // 원래 정점정보
                        Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                        // 현재 정점 정보를 얻고 덮어쓰기
                        Vector3[] destinationVertices = component.textInfo.meshInfo[materialIndex].vertices;
                        Color32[] vertexColors = component.textInfo.meshInfo[materialIndex].colors32;
                        Data_DialogType.ActionType type = dialogType.actionType[dialogActions[i].z];
                        SetActionType(type, vertexIndex, sourceVertices, destinationVertices, c);
                    }
                }
            }
            component.UpdateVertexData();
            Debug.LogWarning("TextAction");
        }
    }

    Vector2 Wobble(float _time, Vector2 _angle, float _length)
    {
        return new Vector2(Mathf.Sin(_time * _angle.x) * _angle.x, Mathf.Cos(_time * _angle.y) * _angle.y) * _length;
    }

    void SetActionType(Data_DialogType.ActionType type, int vertexIndex, Vector3[] sourceVertices, Vector3[] destinationVertices, int _index)
    {
        switch (type.type)
        {
            case Data_DialogType.ActionType.TextType.None:

                break;

            case Data_DialogType.ActionType.TextType.Move:
                Vector3 offset = Wobble(Time.time * type.speed + _index, type.angle, type.range);
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    destinationVertices[index] = sourceVertices[index] + offset;
                }
                break;

            case Data_DialogType.ActionType.TextType.MoveAll:
                offset = Wobble(Time.time * type.speed, type.angle, type.range);
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    destinationVertices[index] = sourceVertices[index] + offset;
                }
                break;

            case Data_DialogType.ActionType.TextType.Wave:
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    float actionRange = type.range * 0.01f;
                    float animTime = Time.time * type.speed;
                    float x = Mathf.Sin(animTime + sourceVertices[index].x * actionRange) * type.angle.x;
                    float y = Mathf.Cos(animTime + sourceVertices[index].y * actionRange) * type.angle.y;
                    destinationVertices[index] = sourceVertices[index] + new Vector3(y, x, 0f);
                }
                break;

            case Data_DialogType.ActionType.TextType.Squash:
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    float actionRange = type.range * 0.01f;
                    float animTime = Time.time * type.speed;
                    float x = Mathf.Sin(animTime + sourceVertices[index].x * actionRange) * type.angle.x;
                    float y = Mathf.Cos(animTime + sourceVertices[index].y * actionRange) * type.angle.y;
                    destinationVertices[index] = sourceVertices[index] + new Vector3(x, y, 0f);
                }
                break;

            case Data_DialogType.ActionType.TextType.Jitter:
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    for (int j = 0; j < 2; j++)
                    {
                        float randomIndex = Random.Range(-type.range, type.range);
                        destinationVertices[index][j] = sourceVertices[index][j] + randomIndex;
                    }
                }
                break;
        }
    }

    private void Update()
    {
        FollowUI();
    }
    public Transform target;
    public Vector3 offset;
    void FollowUI()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + offset);
        transform.position = screenPosition;
    }
}
