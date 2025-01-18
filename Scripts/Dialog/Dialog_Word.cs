using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialog_Word : MonoBehaviour
{
    public bool typing;
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
            public float typingSpeed;
        }
    }
    public int defaultSize;
    public string defaultColor;
    const float defaultTypingSpeed = 0.1f;
    public float typingSpeed;
    Coroutine typingCoroutine, actionCoroutine;

    public int dialogIndex;
    public float interval;
    public List<DialogText> dialogInfoamtions = new List<DialogText>();

    public Color dialogID;
    public Button button;
    [SerializeField] private List<Vector3Int> dialogActions = new List<Vector3Int>();


    void Start()
    {
        Audio_Manager.current.BackGroundMusic(BGMSound);
        typingSpeed = defaultTypingSpeed;
        button.onClick.AddListener(SetTest);
    }

    void SetTest()
    {
        if (typing == true)
        {
            StartTyping(false);// ��ŵ
            typingCoroutine = StartCoroutine(Typing());
        }
        else
        {
            dialogIndex++;
            if (dialogIndex >= dialogInfoamtions.Count)
            {
                dialogIndex = 0;
            }
            StopAllCoroutines();
            StartCoroutine(SetText());
        }
    }

    IEnumerator SetText()
    {
        TMP_Text component = dialogText;
        component.text = GetText();
        yield return new WaitForEndOfFrame();

        PreSetHide();
        yield return new WaitForEndOfFrame();

        StartTyping(true);
    }
    //===========================================================================================================
    // ��ȭ ����
    //===========================================================================================================
    string GetText()
    {
        string textStr = dialogInfoamtions[dialogIndex].text;
        DialogText.SubDialog[] subDialogs = dialogInfoamtions[dialogIndex].subDialogs;
        for (int i = 0; i < subDialogs.Length; i++)
        {
            string temp = "{" + i + "}";
            string subText = subDialogs[i].text;
            string subColor = subDialogs[i].color;
            int subSize = subDialogs[i].size;
            subText = SetSizeColor(subText, subSize, subColor);
            textStr = textStr.Replace(temp, subText);
        }
        textStr = SetSizeColor(textStr, defaultSize, defaultColor);
        Debug.LogWarning(textStr);

        return textStr;
    }

    string SetSizeColor(string _text, int _size, string _color)
    {
        string temp = $"<color=#{_color}><size={_size}>{_text}</size></color>"; ;
        return temp;
    }

    // �̸� �����س��� �����
    void PreSetHide()
    {
        TMP_TextInfo textInfo = dialogText.textInfo;
        for (int c = 0; c < textInfo.characterCount; c++)
        {
            var charInfo = textInfo.characterInfo[c];
            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            for (int i = 0; i < 4; i++)
            {
                int index = vertexIndex + i;
                vertexColors[index].a = 0;// ����ȭ
            }
        }
        SetActionRange();
        dialogText.UpdateVertexData();
    }

    void SetActionRange()
    {
        dialogActions.Clear();
        string textStr = dialogInfoamtions[dialogIndex].text;
        DialogText.SubDialog[] subDialogs = dialogInfoamtions[dialogIndex].subDialogs;
        for (int i = 0; i < subDialogs.Length; i++)
        {
            string temp = "{" + i + "}";
            string subText = subDialogs[i].text;

            int start = textStr.IndexOf(temp, 0, textStr.Length);// ���� ��ġ
            int end = start + subText.Length;// �� ������
            int type = (int)subDialogs[i].style - 1;// �׼� ��Ÿ�� (None 0 ����)
            Vector3Int vector = new Vector3Int(start, end, type);
            dialogActions.Add(vector);
            textStr = textStr.Replace(temp, subText);// ����
        }
    }

    //===========================================================================================================
    // ��ȭ ����
    //===========================================================================================================
    void StartTyping(bool _typing)
    {
        typing = _typing;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(Typing());

        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        actionCoroutine = StartCoroutine(TextAction());
    }

    IEnumerator Typing()
    {
        int daIndex = 0;
        TMP_TextInfo textInfo = dialogText.textInfo;
        for (int c = 0; c < textInfo.characterCount; c++)
        {
            DialogText.SubDialog[] sub = dialogInfoamtions[dialogIndex].subDialogs;
            if (sub.Length > 0)
            {
                // Ÿ���� �ӵ� ����
                float speed = sub[daIndex].typingSpeed;
                if (c == dialogActions[daIndex].x)
                {
                    if (speed > 0)// Ÿ���� ���ǵ尡 0 �̻��̶��..
                        typingSpeed = speed;
                }

                if (c == dialogActions[daIndex].y)
                {
                    if (daIndex + 1 < sub.Length)
                    {
                        daIndex++;
                        speed = sub[daIndex].typingSpeed;
                    }
                    typingSpeed = defaultTypingSpeed;// �⺻ �ӵ�
                }
            }

            var charInfo = textInfo.characterInfo[c];
            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            for (int i = 0; i < 4; i++)
            {
                int index = vertexIndex + i;
                vertexColors[index].a = (byte)255;// Ȱ��ȭ
            }
            Audio_Manager.current.FXAudio(FXSound);
            if (typing == true)
                yield return new WaitForSeconds(typingSpeed);
            dialogText.UpdateVertexData();
        }
        typing = false;
    }
    public string BGMSound, FXSound;
    //===========================================================================================================
    // �ؽ�Ʈ �׼�
    //===========================================================================================================
    IEnumerator TextAction()
    {
        TMP_Text component = dialogText;
        TMP_MeshInfo[] cachedMeshInfo = component.textInfo.CopyMeshInfoVertexData();

        while (dialogActions.Count > 0)
        {
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < dialogActions.Count; i++)
            {
                // x - ���� ������
                // y - �� ������
                // z - �׼� Ÿ��
                if (dialogActions[i].z > -1)// �׼� Ÿ���� None(-1)�� �ƴϸ�
                {
                    for (int c = dialogActions[i].x; c < dialogActions[i].y; c++)
                    {
                        var charInfo = component.textInfo.characterInfo[c];
                        if (charInfo.isVisible == false)
                            continue;

                        int materialIndex = charInfo.materialReferenceIndex;
                        int vertexIndex = charInfo.vertexIndex;

                        // ���� ��������
                        Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                        // ���� ���� ������ ��� �����
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
}
