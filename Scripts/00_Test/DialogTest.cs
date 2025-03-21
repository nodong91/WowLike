using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DialogTest;

public class DialogTest : MonoBehaviour
{
    bool actionBool = false;
    const string inID = "{";
    const string outID = "}";
    string setID;

    public TMP_Text dialogText;
    public Data_DialogType dialogType;
    Coroutine typingCoroutine;
    Coroutine actionCoroutine;
    bool typing;
    //public List<Vector3Int> actionList = new List<Vector3Int>();
    //public List<float> speedList = new List<float>();
    [System.Serializable]
    public class ActionText
    {
        public Vector3Int offset;
        public float speed;
        public ActionText(Vector3Int _offset, float _speed)
        {
            offset = _offset;
            speed = _speed;
        }
    }
    public List<ActionText> actionText = new List<ActionText>();

    public delegate void DeleHandler();
    public DeleHandler deleTyping;

    // 기본 세팅
    const float defaultTypingSpeed = 0.1f;
    const int defaultSize = 40;
    const string defaultColor = "FFFFFF";
    const string lineEnd = "/n";

    public void SetDialog(string _id)
    {
        setID = _id;
        StartCoroutine(StartDialog());
    }

    IEnumerator StartDialog()
    {
        dialogText.text = TryDialog();
        dialogText.ForceMeshUpdate(true);// 메쉬 재 생성 (리셋)
        PreSetting();

        yield return new WaitForSeconds(defaultTypingSpeed);

        StartTyping(true);
        StartActing();
    }

    string TryDialog()
    {
        if (Singleton_Data.INSTANCE.Dict_Dialog.ContainsKey(setID) == false)
            return $"<color=#FF0000>{setID}</color> : 아이디가 없습니다!!";

        Data_Manager.DialogStruct mainDialog = Singleton_Data.INSTANCE.Dict_Dialog[setID];
        string mainString = Singleton_Data.INSTANCE.TryTranslation(0, setID);
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
        actionText.Clear();

        string setIndex = mainString;// 글자 개수 뽑을 때 사용
        setIndex = setIndex.Replace(lineEnd, " ");// 줄넘김 제거(\n 일경우 제거 안됨)
        string setText = mainString;// 실제 출력 시 사용
        setText = setText.Replace(lineEnd, "\n");// 줄넘김 변경

        for (int i = 0; i < ids.Count; i++)
        {
            string setting = $"{inID}{ids[i]}{outID}"; // {}안 텍스트 추출
            Data_Manager.DialogStruct temp = Singleton_Data.INSTANCE.Dict_Dialog[ids[i]];
            string tempText = Singleton_Data.INSTANCE.TryTranslation(0, ids[i]);
            if (temp.textStyle != Data_DialogType.TextStyle.None)
                actionBool = true;

            int startPoint = setIndex.IndexOf(setting);// 시작 위치
            int endPoint = startPoint + tempText.Length;

            Vector3Int offset = new Vector3Int(startPoint, endPoint, (int)(temp.textStyle - 1));
            float speed = temp.speed;

            ActionText tempAction = new ActionText(offset, speed);
            actionText.Add(tempAction);

            setIndex = setIndex.Replace(setting, tempText);
            string richString = SetRichText(tempText, temp.size, temp.color, temp.bold);
            setText = setText.Replace(setting, richString);

        }
        int mainSize = mainDialog.size > 0 ? mainDialog.size : defaultSize;
        string mainColor = mainDialog.color.Length > 0 ? mainDialog.color : defaultColor;
        bool mainBold = mainDialog.bold;
        setText = SetRichText(setText, mainSize, mainColor, mainBold);
        Debug.LogWarning($"{setText}");
        return setText;
    }

    string SetRichText(string _text, int _size, string _color, bool _bold)
    {
        string temp = _size > 0 ? $"<size={_size}>{_text}</size>" : _text;
        temp = _color.Length > 0 ? $"<color=#{_color}>{temp}</color>" : temp;
        temp = _bold == true ? $"<b>{temp}</b>" : temp;
        return temp;
    }

    void StartTyping(bool _typing)
    {
        typing = _typing;
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(Typing(actionText));
    }

    IEnumerator Typing(List<ActionText> _actionText)
    {
        int subIndex = 0;
        float typingSpeed = defaultTypingSpeed;// 기본 속도
        TMP_TextInfo textInfo = dialogText.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (_actionText.Count > 0)
            {
                float speed = _actionText[subIndex].speed;
                if (i == _actionText[subIndex].offset.x)
                {
                    if (speed > 0)// 타이핑 스피드가 0 이상이라면..
                        typingSpeed = speed;
                }
                else if (i == _actionText[subIndex].offset.y)
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
                deleTyping?.Invoke();// 타이핑 소리??
                //Singleton_Audio.INSTANCE.Audio_SetFX(FXSound);
                dialogText.UpdateVertexData();
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        dialogText.UpdateVertexData();
        typing = false;
    }

    // 미리 세팅해놓고 숨기기
    void PreSetting()
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
        actionCoroutine = StartCoroutine(TextAction(actionText));
    }

    IEnumerator TextAction(List<ActionText> _actionText)
    {
        TMP_Text component = dialogText;
        TMP_MeshInfo[] cachedMeshInfo = component.textInfo.CopyMeshInfoVertexData();
        while (actionBool == true)
        {
            yield return null;
            //yield return new WaitForSeconds(interval);
            for (int i = 0; i < _actionText.Count; i++)
            {
                // x - 시작 포지션
                // y - 끝 포지션
                // z - 액션 타입
                if (_actionText[i].offset.z > -1)// 액션 타입이 None(-1)이 아니면
                {
                    for (int c = _actionText[i].offset.x; c < _actionText[i].offset.y; c++)
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
                        Data_DialogType.ActionType type = dialogType.actionType[_actionText[i].offset.z];
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
}
