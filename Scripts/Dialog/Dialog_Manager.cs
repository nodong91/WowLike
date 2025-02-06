using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialog_Manager : MonoBehaviour
{
    public string FXSound;
    const float defaultTypingSpeed = 0.1f;
    const int defaultSize = 15;
    const string defaultColor = "000000";
    const string lineEnd = "/n";

    public bool typing;
    public TMP_Text dialogText;
    public Data_DialogType dialogType;

    public float typingSpeed;
    Coroutine typingCoroutine, actionCoroutine;

    public int dialogIndex;
    public float interval;
    public Transform target;
    public Vector3 offset;

    public Image nextMark;
    public Button button;

    void Start()
    {
        typingSpeed = defaultTypingSpeed;
        button.onClick.AddListener(SetTest);
    }

    void SetTest()
    {
        AddFollow();

        if (typing == true)
        {
            StartTyping(false);
        }
        else
        {
            StopAllCoroutines();
            typingCoroutine = StartCoroutine(StartDialog());
        }
    }

    void AddFollow()
    {
        target = Game_Manager.instance.target;// 임시 타겟
        UI_Manager.FollowStruct followStruct = new UI_Manager.FollowStruct
        {
            followTarget = target,
            followOoffset = offset,
        };
        UI_Manager.instance.AddFollowUI(transform, followStruct);
    }

    void RemoveFollow()
    {
        UI_Manager.instance.RemoveFollowUI(transform);
    }

















    bool actionBool = false;
    const string inID = "{";
    const string outID = "}";
    public string setID;
    public List<Vector3Int> actionList = new List<Vector3Int>();
    public List<float> speedList = new List<float>();

    IEnumerator StartDialog()
    {
        bool id = Singleton_Data.INSTANCE.Dict_Dialog.ContainsKey(setID);
        dialogText.text = id ? TryDialog(setID) : $"<size=25>{setID}</size> : 아이디가 없습니다!!";
        dialogText.ForceMeshUpdate(true);// 메쉬 재 생성 (리셋)
        yield return null;

        PreSetHide();// 글자 숨김
        yield return new WaitForSeconds(defaultTypingSpeed);

        StartTyping(true);
        StartActing();
    }

    string TryDialog(string _string)
    {
        Data_Manager.DialogInfoamtion mainDialog = Singleton_Data.INSTANCE.Dict_Dialog[_string];
        string mainString = Singleton_Data.INSTANCE.TryDialogTranslation(_string);
        actionBool = false;
        List<string> ids = new List<string>();
        string[] start = mainString.Split(inID);// id추출
        for (int i = 0; i < start.Length; i++)
        {
            int endIndex = start[i].IndexOf(outID);
            if (endIndex > -1)
            {
                string result = start[i].Substring(0, endIndex);
                ids.Add(result);
            }
        }
        actionList.Clear();
        speedList.Clear();
        string setIndex = mainString;// 글자 개수 뽑을 때 사용
        setIndex = setIndex.Replace(lineEnd, " ");// 줄넘김 제거(\n 일경우 제거 안됨)
        string setText = mainString;// 실제 출력 시 사용
        setText = setText.Replace(lineEnd, "\n");// 줄넘김 변경

        for (int i = 0; i < ids.Count; i++)
        {
            string setting = inID + ids[i] + outID;
            Data_Manager.DialogInfoamtion temp = Singleton_Data.INSTANCE.Dict_Dialog[ids[i]];
            string tempText = Singleton_Data.INSTANCE.TryDialogTranslation(ids[i]);
            if (temp.textStyle != Data_DialogType.TextStyle.None)
                actionBool = true;
            int startPoint = setIndex.IndexOf(setting);// 시작 위치
            int endPoint = startPoint + tempText.Length;
            Vector3Int actionVector = new Vector3Int(startPoint, endPoint, (int)(temp.textStyle - 1));
            actionList.Add(actionVector);

            float speed = temp.speed;
            speedList.Add(speed);

            setIndex = setIndex.Replace(setting, tempText);
            string richString = SetRichText(tempText, temp.size, temp.color, temp.bold);
            setText = setText.Replace(setting, richString);

        }
        Debug.LogWarning($"{setText}");
        int mainSize = mainDialog.size > 0 ? mainDialog.size : defaultSize;
        string mainColor = mainDialog.color.Length > 0 ? mainDialog.color : defaultColor;
        bool mainBold = mainDialog.bold;
        setText = SetRichText(setText, mainSize, mainColor, mainBold);
        return setText;
    }

    string SetRichText(string _text, int _size, string _color, bool _bold)
    {
        string temp = _size > 0 ? $"<size={_size}>{_text}</size>" : _text;
        temp = _color.Length > 0 ? $"<color=#{_color}>{temp}</color>" : temp;
        temp = _bold == true ? $"<b>{temp}</b>" : temp;

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

    void StartActing()
    {
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        actionCoroutine = StartCoroutine(TextAction(actionList));
    }

    IEnumerator TextAction(List<Vector3Int> _actionText)
    {
        TMP_Text component = dialogText;
        TMP_MeshInfo[] cachedMeshInfo = component.textInfo.CopyMeshInfoVertexData();
        while (actionBool == true)
        {
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < _actionText.Count; i++)
            {
                // x - 시작 포지션
                // y - 끝 포지션
                // z - 액션 타입
                if (_actionText[i].z > -1)// 액션 타입이 None(-1)이 아니면
                {
                    for (int c = _actionText[i].x; c < _actionText[i].y; c++)
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
                        //Color32[] vertexColors = component.textInfo.meshInfo[materialIndex].colors32;
                        Data_DialogType.ActionType type = dialogType.actionType[_actionText[i].z];
                        //SetActionType(type, vertexIndex, sourceVertices, destinationVertices, c);
                        TryAimationWave(type, vertexIndex, sourceVertices, destinationVertices, c);
                    }
                }
            }
            component.UpdateVertexData();
            Debug.LogWarning("TextAction");
        }
    }

    void TryAnimationCurve(Data_DialogType.ActionType type, int vertexIndex, Vector3[] sourceVertices, Vector3[] destinationVertices, int _index)
    {
        AnimationCurve curve = type.curve;
        float curveTime = (Time.time * type.speed) + (type.interval * _index);
        float curveValue = curve.Evaluate(curveTime);
        for (int v = 0; v < 4; v++)
        {
            int index = vertexIndex + v;
            float x = curveValue * type.angle.x;
            float y = curveValue * type.angle.y;
            destinationVertices[index] = sourceVertices[index] + new Vector3(x, y, 0f);
        }
    }

    void TryAimationWave(Data_DialogType.ActionType type, int vertexIndex, Vector3[] sourceVertices, Vector3[] destinationVertices, int _index)
    {
        AnimationCurve curve = type.curve;
        for (int v = 0; v < 4; v++)
        {
            int index = vertexIndex + v;
            float curveTime = (Time.time * type.speed) + (type.interval * _index);
            float curveValue = curve.Evaluate(curveTime);

            float x = curveValue * type.angle.x;
            float y = curveValue * type.angle.y;
            //float animTime = Time.time * type.speed;
            //float actionRange = 5f * 0.01f;
            //float x = Mathf.Sin(curveTime + sourceVertices[index].x * actionRange) * type.angle.x;
            //float y = Mathf.Cos(curveTime + sourceVertices[index].y * actionRange) * type.angle.y;
            destinationVertices[index] = sourceVertices[index] + new Vector3(y, x, 0f);
        }
    }

    //void SetActionType(Data_DialogType.ActionType type, int vertexIndex, Vector3[] sourceVertices, Vector3[] destinationVertices, int _index)
    //{
    //    switch (type.type)
    //    {
    //        case Data_Manager.DialogInfoamtion.TextType.None:

    //            break;

    //        case Data_Manager.DialogInfoamtion.TextType.Move:
    //            Vector3 offset = Wobble(Time.time * type.speed + _index, type.angle, type.range);
    //            for (int v = 0; v < 4; v++)
    //            {
    //                int index = vertexIndex + v;
    //                destinationVertices[index] = sourceVertices[index] + offset;
    //            }
    //            break;

    //        case Data_Manager.DialogInfoamtion.TextType.MoveAll:
    //            offset = Wobble(Time.time * type.speed, type.angle, type.range);
    //            for (int v = 0; v < 4; v++)
    //            {
    //                int index = vertexIndex + v;
    //                destinationVertices[index] = sourceVertices[index] + offset;
    //            }
    //            break;

    //        case Data_Manager.DialogInfoamtion.TextType.Wave:
    //            for (int v = 0; v < 4; v++)
    //            {
    //                int index = vertexIndex + v;
    //                float actionRange = type.range * 0.01f;
    //                float animTime = Time.time * type.speed;
    //                float x = Mathf.Sin(animTime + sourceVertices[index].x * actionRange) * type.angle.x;
    //                float y = Mathf.Cos(animTime + sourceVertices[index].y * actionRange) * type.angle.y;
    //                destinationVertices[index] = sourceVertices[index] + new Vector3(y, x, 0f);
    //            }
    //            break;

    //        case Data_Manager.DialogInfoamtion.TextType.Squash:
    //            for (int v = 0; v < 4; v++)
    //            {
    //                int index = vertexIndex + v;
    //                float actionRange = type.range * 0.01f;
    //                float animTime = Time.time * type.speed + _index;
    //                float x = Mathf.Sin(animTime + sourceVertices[index].x * actionRange) * type.angle.x;
    //                float y = Mathf.Cos(animTime + sourceVertices[index].y * actionRange) * type.angle.y;
    //                destinationVertices[index] = sourceVertices[index] + new Vector3(x, y, 0f);
    //            }
    //            break;

    //        case Data_Manager.DialogInfoamtion.TextType.Jitter:
    //            for (int v = 0; v < 4; v++)
    //            {
    //                int index = vertexIndex + v;
    //                for (int j = 0; j < 2; j++)
    //                {
    //                    float randomIndex = Random.Range(-type.range, type.range);
    //                    destinationVertices[index][j] = sourceVertices[index][j] + randomIndex;
    //                }
    //            }
    //            break;

    //        case Data_Manager.DialogInfoamtion.TextType.Test:

    //            break;
    //    }
    //}

    //Vector2 Wobble(float _time, Vector2 _angle, float _length)
    //{
    //    return new Vector2(Mathf.Sin(_time * _angle.x) * _angle.x, Mathf.Cos(_time * _angle.y) * _angle.y) * _length;
    //}

    void StartTyping(bool _typing)
    {
        typing = _typing;
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(Typing(actionList));
    }

    IEnumerator Typing(List<Vector3Int> _actionText)
    {
        int subIndex = 0;
        TMP_TextInfo textInfo = dialogText.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (_actionText.Count > 0)
            {
                float speed = speedList[subIndex];
                if (i == _actionText[subIndex].x)
                {
                    if (speed > 0)// 타이핑 스피드가 0 이상이라면..
                        typingSpeed = speed;
                }
                else if (i == _actionText[subIndex].y)
                {
                    if (subIndex + 1 < _actionText.Count)
                        subIndex++;
                    typingSpeed = defaultTypingSpeed;// 기본 속도
                }
            }

            var charInfo = textInfo.characterInfo[i];
            if (charInfo.isVisible == false)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            for (int j = 0; j < 4; j++)
            {
                int index = vertexIndex + j;
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
        RemoveFollow();
    }
}
