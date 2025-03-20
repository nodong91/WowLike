using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class DialogTest : MonoBehaviour
{
    public void Startasdfadsf()
    {

        StartCoroutine(StartDialog());
    }

    IEnumerator StartDialog()
    {
        dialogText.text = TryDialog($"<size=25>{setID}</size> : 아이디가 없습니다!!");
        dialogText.ForceMeshUpdate(true);// 메쉬 재 생성 (리셋)
        yield return null;

        PreSetHide();// 글자 숨김
        yield return new WaitForSeconds(0.1f);

        StartTyping(true);
        //StartActing();
    }

    string TryDialog(string _string)
    {
        //Data_Manager.DialogStruct mainDialog = Singleton_Data.INSTANCE.Dict_Dialog[_string];
        //string mainString = Singleton_Data.INSTANCE.TryTranslation(0, _string);
        string mainString = _string;
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
            Data_Manager.DialogStruct temp = Singleton_Data.INSTANCE.Dict_Dialog[ids[i]];
            string tempText = Singleton_Data.INSTANCE.TryTranslation(0, ids[i]);
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
        //int mainSize = mainDialog.size > 0 ? mainDialog.size : defaultSize;
        //string mainColor = mainDialog.color.Length > 0 ? mainDialog.color : defaultColor;
        //bool mainBold = mainDialog.bold;
        //setText = SetRichText(setText, mainSize, mainColor, mainBold);
        return setText;
    }

    string SetRichText(string _text, int _size, string _color, bool _bold)
    {
        string temp = _size > 0 ? $"<size={_size}>{_text}</size>" : _text;
        temp = _color.Length > 0 ? $"<color=#{_color}>{temp}</color>" : temp;
        temp = _bold == true ? $"<b>{temp}</b>" : temp;

        return temp;
    }


    bool actionBool = false;
    const string inID = "{";
    const string outID = "}";
    public string setID;
    public List<Vector3Int> actionList = new List<Vector3Int>();
    public List<float> speedList = new List<float>();

    public TMP_Text dialogText;
    public Data_DialogType dialogType;
    Coroutine typingCoroutine;
    bool typing;
    // 기본 세팅
    const float defaultTypingSpeed = 0.1f;
    const int defaultSize = 15;
    const string defaultColor = "000000";
    const string lineEnd = "/n";

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
        float typingSpeed = 0f;
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
                //Singleton_Audio.INSTANCE.Audio_SetFX(FXSound);
                dialogText.UpdateVertexData();
                yield return new WaitForSeconds(0.1f);
            }
        }
        dialogText.UpdateVertexData();
        typing = false;
        typingSpeed = defaultTypingSpeed;// 기본 속도

        //StartCoroutine(WaitingNext());
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
}
