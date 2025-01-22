using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Dialog_Test : MonoBehaviour
{
    const string inID = "{";
    const string outID = "}";

    const float defaultTypingSpeed = 0.1f;
    const int defaultSize = 15;
    const string defaultColor = "000000";

    public List<Vector3Int> actionList = new List<Vector3Int>();

    public TMP_Text dialogText;

    public DialogInfoamtion.TextType textStyle;
    public float speed;
    public float range;
    public Vector2 angle;

    [System.Serializable]
    public class DialogInfoamtion
    {
        public string ID;
        public string korean;
        public string japanese;
        public string color;
        public int size;
        public bool bold;

        public enum TextType
        {
            None,
            Move,
            MoveAll,
            Wave,
            Squash,
            Jitter,
        }
        //public Data_DialogType.TextStyle textStyle;
        //public float speed;
    }
    public List<DialogInfoamtion> dialog;
    public DialogInfoamtion mainText;

    private void Start()
    {
        TestTest();
        StartCoroutine(SetTextTest());
    }

    void TestTest()
    {
        DialogInfoamtion temp0 = new DialogInfoamtion
        {
            ID = "50010",
            korean = "{50011}이 {50012}",
            color = "000000"
        };
        mainText = temp0;
        dialog = new List<DialogInfoamtion>();
        DialogInfoamtion temp1 = new DialogInfoamtion
        {
            ID = "50011",
            korean = "김지영",
            color = "00FF20",
            size = 20
        }; 
        DialogInfoamtion temp2 = new DialogInfoamtion
        {
            ID = "50012",
            korean = "죽어버렸어!!!",
            color = "FF0000",
            size = 25
        };
        dialog.Add(temp1);
        dialog.Add(temp2);
    }

    IEnumerator SetTextTest()
    {
        dialogText.text = GetStringID();
        dialogText.ForceMeshUpdate(true);// 메쉬 재 생성 (리셋)
        yield return new WaitForEndOfFrame();

        //PreSetHide();// 글자 숨김
        yield return new WaitForSeconds(defaultTypingSpeed);

        StartCoroutine(TextAction(actionList));
    }

    string GetStringID()
    {
        //Data_Manager.DialogInfoamtion mainDialog = Singleton_Data.INSTANCE.Dict_Dialog[_string];
        DialogInfoamtion mainDialog = mainText;
        string mainString = mainDialog.korean;
        List<string> id = new List<string>();
        string[] start = mainString.Split(inID);// id추출
        for (int i = 0; i < start.Length; i++)
        {
            int endIndex = start[i].IndexOf(outID);
            if (endIndex > -1)
            {
                string result = start[i].Substring(0, endIndex);
                id.Add(result);
            }
        }

        actionList.Clear();
        string setIndex = mainString;
        string setText = mainString;
        for (int i = 0; i < id.Count; i++)
        {
            string setting = inID + id[i] + outID;
            //DialogInfoamtion temp = Singleton_Data.INSTANCE.Dict_Dialog[id[i]];
            DialogInfoamtion temp = dialog[i];
            int startPoint = setIndex.IndexOf(setting, 0, setIndex.Length);// 시작 위치
            int endPoint = startPoint + temp.korean.Length;
            Vector3Int actionVector = new Vector3Int(startPoint, endPoint, 0);
            actionList.Add(actionVector);

            setIndex = setIndex.Replace(setting, temp.korean);
            string richString = SetRichText(temp.korean, temp.size, temp.color, temp.bold);
            setText = setText.Replace(setting, richString);

            Debug.LogWarning($"{setting}, {setText}");
        }
        int mainSize = mainDialog.size > 0 ? mainDialog.size : defaultSize;
        string mainColor = mainDialog.color.Length > 0 ? mainDialog.color : defaultColor;
        bool mainBold = mainDialog.bold;
        setText = SetRichText(setText, mainSize, mainColor, mainBold);
        return setText;
    }

    IEnumerator TextAction(List<Vector3Int> _actionText)
    {
        TMP_Text component = dialogText;
        TMP_MeshInfo[] cachedMeshInfo = component.textInfo.CopyMeshInfoVertexData();

        while ( true)
        {
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
                        Color32[] vertexColors = component.textInfo.meshInfo[materialIndex].colors32;
                        SetActionType(vertexIndex, sourceVertices, destinationVertices, c);
                    }
                }
            }
            yield return null;
            component.UpdateVertexData();
            Debug.LogWarning("TextAction");
        }
    }

    string SetRichText(string _text, int _size, string _color, bool _bold)
    {
        string temp = _size > 0 ? $"<size={_size}>{_text}</size>" : _text;
        temp = _color.Length > 0 ? $"<color=#{_color}>{temp}</color>" : temp;
        temp = _bold == true ? $"<b>{temp}</b>" : temp;

        return temp;
    }

    void SetActionType(int vertexIndex, Vector3[] sourceVertices, Vector3[] destinationVertices, int _index)
    {
        switch (textStyle)
        {
            case DialogInfoamtion.TextType.None:

                break;

            case DialogInfoamtion.TextType.Move:
                Vector3 offset = Wobble(Time.time * speed + _index, angle, range);
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    destinationVertices[index] = sourceVertices[index] + offset;
                }
                break;

            case DialogInfoamtion.TextType.MoveAll:
                offset = Wobble(Time.time * speed, angle, range);
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    destinationVertices[index] = sourceVertices[index] + offset;
                }
                break;

            case DialogInfoamtion.TextType.Wave:
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    float actionRange = range * 0.01f;
                    float animTime = Time.time * speed;
                    float x = Mathf.Sin(animTime + sourceVertices[index].x * actionRange) * angle.x;
                    float y = Mathf.Cos(animTime + sourceVertices[index].y * actionRange) * angle.y;
                    destinationVertices[index] = sourceVertices[index] + new Vector3(y, x, 0f);
                }
                break;

            case DialogInfoamtion.TextType.Squash:
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    float actionRange = range * 0.01f;
                    float animTime = Time.time * speed + _index;
                    float x = Mathf.Sin(animTime + sourceVertices[index].x * actionRange) * angle.x;
                    float y = Mathf.Cos(animTime + sourceVertices[index].y * actionRange) * angle.y;
                    destinationVertices[index] = sourceVertices[index] + new Vector3(x, y, 0f);
                }
                break;

            case DialogInfoamtion.TextType.Jitter:
                for (int v = 0; v < 4; v++)
                {
                    int index = vertexIndex + v;
                    for (int j = 0; j < 2; j++)
                    {
                        float randomIndex = Random.Range(-range, range);
                        destinationVertices[index][j] = sourceVertices[index][j] + randomIndex;
                    }
                }
                break;
        }
    }
    Vector2 Wobble(float _time, Vector2 _angle, float _length)
    {
        return new Vector2(Mathf.Sin(_time * _angle.x) * _angle.x, Mathf.Cos(_time * _angle.y) * _angle.y) * _length;
    }
}
