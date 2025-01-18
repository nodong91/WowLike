using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public class Dialog_Test : MonoBehaviour
{
    public Button button, button1;

    bool skip;
    public float interval;
    Coroutine typing, animating;

    public TMP_Text dialogText;
    public string[] dialogID;
    public List<Vector2Int> animatingTextCount = new List<Vector2Int>();
    public List<Data_Manager.DialogInfoamtion> dialogInfoamtions = new List<Data_Manager.DialogInfoamtion>();

    public delegate void TypingDelegate(Dialog_Test _test);
    public TypingDelegate typingDelegate;

    private void Start()
    {
        button.onClick.AddListener(StartDialog);
        button1.onClick.AddListener(ResetPoint);
    }

    void ResetPoint()
    {
        TMP_Text component = dialogText;
        //component.text = GetDialog().Replace("/n", "\n");
        //StartCoroutine(Test(component));
        if (typing != null)
            StopCoroutine(typing);

        if (animating != null)
            StopCoroutine(animating);
        ResetTextAnimation(component.textInfo);
        component.UpdateVertexData();
    }
    public float ddffff;
    IEnumerator Test(TMP_Text component)
    {
        //    for (int c = 0; c < component.textInfo.characterCount; c++)
        //    {
        //        var charInfo = component.textInfo.characterInfo[c];
        //        if (!charInfo.isVisible)
        //            continue;

        //        int materialIndex = charInfo.materialReferenceIndex;
        //        int vertexIndex = charInfo.vertexIndex;
        //        Color32[] vertexColors = component.textInfo.meshInfo[materialIndex].colors32;

        //        for (int i = 0; i < 4; i++)
        //        {
        //            int index = vertexIndex + i;
        //            vertexColors[index].a = (byte)(255f / ddffff);// 투명화
        //        }
        //    }

        if (typing != null)
            StopCoroutine(typing);

        if (animating != null)
            StopCoroutine(animating);

        //TMP_TextInfo textInfo = component.textInfo;

        //TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        //List<Vector2Int> aniTextCount = animatingTextCount;
        yield return null;
        //TextAnimation(textInfo, cachedMeshInfo);
        ResetTextAnimation(component.textInfo);
        component.UpdateVertexData();

    }

    void ResetTextAnimation(TMP_TextInfo _textInfo)
    {
        TMP_MeshInfo[] cachedMeshInfo = _textInfo.CopyMeshInfoVertexData();
        for (int characterIndex = 0; characterIndex < animatingTextCount.Count; characterIndex++)
        {
            int queueIndex = animatingTextCount[characterIndex].x;
            var charInfo = _textInfo.characterInfo[queueIndex];
            if (charInfo.isVisible == false)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            if (cachedMeshInfo.Length > materialIndex)
            {
                // 원래 정점정보
                Vector3[] sourceVertices = originVertices[materialIndex].vertices;

                // 현재 정점 정보를 얻고 덮어쓰기
                var destinationVertices = _textInfo.meshInfo[materialIndex].vertices;

                Color32[] vertexColors = _textInfo.meshInfo[materialIndex].colors32;
                for (int i = 0; i < 4; i++)
                {
                    int index = vertexIndex + i;
                    for (int j = 0; j < 2; j++)
                    {
                        destinationVertices[index][j] = sourceVertices[index][j];
                    }
                    //destinationVertices[index] = sourceVertices[index];
                    vertexColors[index].a = (byte)(255f / ddffff);// 투명화
                }
            }
        }
    }




    [System.Serializable]
    public class djdjdj
    {
        public Vector3[] vertices;
    }
    public List<djdjdj> originVertices = new List<djdjdj>();
    void SetAniFontOrigin()
    {
        originVertices.Clear();
        TMP_MeshInfo[] cachedMeshInfo = dialogText.textInfo.CopyMeshInfoVertexData();

        for (int i = 0; i < animatingTextCount.Count; i++)
        {
            int queueIndex = animatingTextCount[i].x;
            var charInfo = dialogText.textInfo.characterInfo[queueIndex];
            if (charInfo.isVisible == false)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            if (cachedMeshInfo.Length > materialIndex)
            {
                // 원래 정점정보
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                djdjdj jjjj = new djdjdj { vertices = sourceVertices };
                originVertices.Add(jjjj);
            }
        }
    }
    void StartDialog()
    {
        if (typing != null)
            StopCoroutine(typing);
        typing = StartCoroutine(StartSetting());
    }

    IEnumerator StartSetting()
    {
        TMP_Text component = dialogText;
        component.text = GetDialog().Replace("/n", "\n");
        //component.enableWordWrapping = true;
        //component.textWrappingMode = TextWrappingModes.PreserveWhitespace;

        yield return null;
        SetAniFontOrigin();
        OnPreHideText(component.textInfo);
        component.UpdateVertexData();
        //component.ForceMeshUpdate();

        yield return null;

        // 텍스트 세팅 및 애니메이션 플레이
        if (typing != null)
            StopCoroutine(typing);
        typing = StartCoroutine(Typing());

        if (animating != null)
            StopCoroutine(animating);
        animating = StartCoroutine(TextAnimation());
    }
    public int ininininn;
    string GetDialog()
    {
        string setSpeech = "";
        int animIndex = 0;
        animatingTextCount.Clear();
        dialogInfoamtions.Clear();
        //for (int i = 0; i < dialogID.Length; i++)
        //{
            Data_Manager.DialogInfoamtion dialogInfoamtion = Singleton_Data.INSTANCE.Dict_Dialog[dialogID[ininininn]];
            dialogInfoamtions.Add(dialogInfoamtion);
            if (dialogInfoamtion.animType != Data_Manager.DialogInfoamtion.AnimType.None)// 애니메이션 문자열
            {
                int start = animIndex;
                int end = animIndex + dialogInfoamtion.text.Length;
                for (int c = start; c < end; c++)
                {
                    Vector2Int textAnimation = new Vector2Int(c, ininininn);
                    animatingTextCount.Add(textAnimation);
                }
            }

            string textStr = dialogInfoamtion.text;
            if (dialogInfoamtion.lineEnd == true)// 다음 문자열에서 빼기
            {
                textStr += "/n";
                animIndex++;
            }
            animIndex += dialogInfoamtion.text.Length;
            string colorHex = ColorUtility.ToHtmlStringRGB(dialogInfoamtion.color);
            setSpeech += $"<color=#{colorHex}><size={dialogInfoamtion.size}>{textStr}</size></color>";
        //}
        return setSpeech;
    }

    // 미리 세팅해놓고 숨기기
    void OnPreHideText(TMP_TextInfo textInfo)
    {
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
                vertexColors[index].a = 0;// 투명화
            }
        }
    }

    IEnumerator Typing()
    {
        skip = false;

        TMP_Text component = dialogText;
        TMP_TextInfo textInfo = component.textInfo;
        for (int characterIndex = 0; characterIndex < textInfo.characterCount; characterIndex++)
        {
            var charInfo = textInfo.characterInfo[characterIndex];
            if (!charInfo.isVisible)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            for (int i = 0; i < 4; i++)
            {
                int index = vertexIndex + i;
                vertexColors[index].a = (byte)255;
            }

            if (skip == false)
            {
                yield return new WaitForSeconds(0.05f);
                //AudioManager.INSTANCE.StartSoundEffect(dialogVoice, dialogPitch * pitchValue);
            }
            component.UpdateVertexData();
        }
        yield return new WaitForSeconds(interval);

        typingDelegate?.Invoke(this);// 타이핑 끝
    }

    IEnumerator TextAnimation()
    {
        TMP_Text component = dialogText;
        TMP_TextInfo textInfo = component.textInfo;

        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {
            if (animatingTextCount.Count > 0)
            {
                TextAnimation(textInfo, cachedMeshInfo);
                component.UpdateVertexData();
            }
            yield return new WaitForSeconds(interval);
        }
    }

    void TextAnimation(TMP_TextInfo _textInfo, TMP_MeshInfo[] _cachedMeshInfo)
    {
        for (int characterIndex = 0; characterIndex < animatingTextCount.Count; characterIndex++)
        {
            int queueIndex = animatingTextCount[characterIndex].x;
            var charInfo = _textInfo.characterInfo[queueIndex];
            if (charInfo.isVisible == false)
                continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            if (_cachedMeshInfo.Length > materialIndex)
            {
                // 원래 정점정보
                Vector3[] sourceVertices = _cachedMeshInfo[materialIndex].vertices;
                // 현재 정점 정보를 얻고 덮어쓰기
                Vector3[] destinationVertices = _textInfo.meshInfo[materialIndex].vertices;
                int textType = animatingTextCount[characterIndex].y;
                Debug.LogWarning($"{dialogInfoamtions.Count}  {textType}" );
                Data_Manager.DialogInfoamtion textStruct = dialogInfoamtions[materialIndex];
                float animSpeed = textStruct.speed;

                switch (textStruct.animType)
                {
                    case Data_Manager.DialogInfoamtion.AnimType.None:

                        break;

                    case Data_Manager.DialogInfoamtion.AnimType.Moving:
                        Vector3 offset = Wobble(Time.time * animSpeed + queueIndex, textStruct.angle, textStruct.length);
                        for (int i = 0; i < 4; i++)
                        {
                            int index = vertexIndex + i;
                            destinationVertices[index] = sourceVertices[index] + offset;
                        }
                        break;

                    case Data_Manager.DialogInfoamtion.AnimType.Wave:
                        for (int i = 0; i < 4; i++)
                        {
                            float animTime = Time.time * animSpeed;
                            int index = vertexIndex + i;
                            float x = Mathf.Sin(animTime + sourceVertices[index].x * textStruct.length * 0.01f) * textStruct.angle.y;
                            float y = Mathf.Cos(animTime + sourceVertices[index].y * textStruct.length * 0.01f) * textStruct.angle.x;
                            destinationVertices[index] = sourceVertices[index] + new Vector3(y, x, 0f);
                        }
                        break;

                    case Data_Manager.DialogInfoamtion.AnimType.Jitter:
                        for (int i = 0; i < 4; i++)
                        {
                            int index = vertexIndex + i;
                            for (int j = 0; j < 2; j++)
                            {
                                destinationVertices[index][j] = sourceVertices[index][j] + Random.Range(-animSpeed, animSpeed);
                            }
                        }
                        break;
                }
            }
        }
    }

    Vector2 Wobble(float _time, Vector2 _angle, float _length)
    {
        return new Vector2(Mathf.Sin(_time * _angle.x) * _angle.x, Mathf.Cos(_time * _angle.y) * _angle.y) * _length;
    }
}
