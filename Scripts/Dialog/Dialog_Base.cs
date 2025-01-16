using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialog_Base : MonoBehaviour
{
    public SetDialogInfo.DialogType dialogType;
    private const float holdDelay = 0.1f;
    private const float interval = 0.05f;

    public float typingSpeed = 0.05f;
    public string dialogVoice = "Beep Short";
    Vector2 dialogPitch = new Vector2(0.7f, 1.3f);
    [Range(0.1f, 2f)]
    public float pitchValue = 1f;

    public bool skip, typing;
    Coroutine onTyping, animating;

    SetDialogInfo currentInfo;

    [SerializeField]
    private AnimationCurve markAnimCurve;// 다음 대화 마크 애니메이션

    //public delegate void Dele_CheckTyping(Dialog_Base dialog);
    //public Dele_CheckTyping dele_CheckTyping;// 타이핑 끝났는지 체크


    [System.Serializable]
    public struct DialogCanvas
    {
        public SetDialogInfo.DialogType dialogType;
        public CanvasGroup dialogCanvas;
        public TMP_Text dialogText;
        public GameObject nextMark;

        //public void ActiveCanvas(bool _on)
        //{
        //    dialogCanvas.alpha = _on ? 1f : 0f;
        //    dialogText.text = "";
        //}

        //public void SetText(string _text)
        //{
        //    dialogText.text = _text;
        //}
    }
    DialogCanvas currentDialog;
    public DialogCanvas GetCurrentDialog { get { return currentDialog; } }
    public DialogCanvas[] dialogCanvasList;

    public void SetDialog(SetDialogInfo _currentInfo)
    {
        currentInfo = _currentInfo;
        for (int i = 0; i < dialogCanvasList.Length; i++)
        {
            bool onCanvas = _currentInfo.dialogType == dialogCanvasList[i].dialogType;
            dialogCanvasList[i].dialogCanvas.alpha = onCanvas == true ? 1f : 0f;
            if (onCanvas == true)
                currentDialog = dialogCanvasList[i];
        }

        if (onTyping != null)
            StopCoroutine(onTyping);
        onTyping = StartCoroutine(Typing());
    }

    IEnumerator Typing()
    {
        currentDialog.nextMark.SetActive(false);

        TMP_Text component = currentDialog.dialogText;
        TMP_TextInfo textInfo = component.textInfo;

        component.text = currentInfo.GetSpeech().Replace("/n", "\n");
        //component.enableWordWrapping = true;
        component.textWrappingMode = TextWrappingModes.PreserveWhitespace;

        yield return null;
        // 텍스트 세팅 및 애니메이션 플레이
        OnPreHideText(component);
        component.UpdateVertexData();
        //component.ForceMeshUpdate();
        //yield return null;

        if (animating != null)
            StopCoroutine(animating);
        animating = StartCoroutine(TextAnimation(component));

        int cc = textInfo.characterCount;
        typing = true;
        while (typing)
        {
            for (int characterIndex = 0; characterIndex < cc; characterIndex++)
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
            typing = false;
            skip = false;
        }

        //dele_CheckTyping(this);// 타이핑 끝
        yield return new WaitForSeconds(holdDelay);

        currentDialog.nextMark.SetActive(true);
    }

    public void SkipDialog()
    {
        if (typing == true)
            skip = true;
    }

    // 미리 세팅해놓고 숨기기
    private void OnPreHideText(TMP_Text _component)
    {
        TMP_TextInfo textInfo = _component.textInfo;
        int cc = textInfo.characterCount;
        for (int characterIndex = 0; characterIndex < cc; characterIndex++)
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
                vertexColors[index].a = (byte)0;
            }
        }
    }
    IEnumerator TextAnimation(TMP_Text _component)
    {
        TMP_TextInfo textInfo = _component.textInfo;
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        float normalize = 0f;
        while (true)
        {
            List<SetDialogInfo.TextAnimation> aniTextCount = currentInfo.animatingTextCount;

            normalize += Time.deltaTime * 20f;
            float pingpong = Mathf.PingPong(normalize, 1f);
            float movingY = markAnimCurve.Evaluate(pingpong) * 10f;
            currentDialog.nextMark.transform.localPosition = Vector3.up * movingY;

            if (aniTextCount.Count > 0)
            {
                for (int characterIndex = 0; characterIndex < aniTextCount.Count; characterIndex++)
                {
                    int QueueIndex = aniTextCount[characterIndex].textNum;
                    var charInfo = textInfo.characterInfo[QueueIndex];
                    if (!charInfo.isVisible)
                        continue;

                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;

                    if (cachedMeshInfo.Length > materialIndex)
                    {
                        // 원래 정점정보
                        Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                        // 현재 정점 정보를 얻고 덮어쓰기
                        var destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                        int textType = aniTextCount[characterIndex].textType;
                        //Debug.LogWarning(" : " + currentInfo.textSet.Length + " : " + textType);// 왜 전에 찍었던 textType 인수를 찍을까?
                        TextStruct textStruct = currentInfo.textStruct[textType];
                        float animSpeed = textStruct.animSpeed;

                        if (sourceVertices.Length > vertexIndex + 3)
                        {
                            switch (textStruct.textType)
                            {
                                case TextStruct.TextType.None:

                                    break;

                                case TextStruct.TextType.Moving:
                                    Vector3 offset = Wobble(Time.time * animSpeed + QueueIndex, textStruct.actionAngle, textStruct.length);
                                    for (int i = 0; i < 4; i++)
                                    {
                                        destinationVertices[vertexIndex + i] = sourceVertices[vertexIndex + i] + offset;
                                    }
                                    break;

                                case TextStruct.TextType.Wave:
                                    for (int i = 0; i < 4; i++)
                                    {
                                        float animTime = Time.time * animSpeed;
                                        float x = Mathf.Sin(animTime + sourceVertices[vertexIndex + i].x * textStruct.length * 0.01f) * textStruct.actionAngle.y;
                                        float y = Mathf.Cos(animTime + sourceVertices[vertexIndex + i].y * textStruct.length * 0.01f) * textStruct.actionAngle.x;
                                        destinationVertices[vertexIndex + i] = sourceVertices[vertexIndex + i] + new Vector3(y, x, 0f);
                                    }
                                    break;

                                case TextStruct.TextType.Jitter:
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
                _component.UpdateVertexData();
            }
            yield return new WaitForSeconds(interval);
        }
    }

    Vector2 Wobble(float _time, Vector2 _angle, float _length)
    {
        return new Vector2(Mathf.Sin(_time * _angle.x) * _angle.x, Mathf.Cos(_time * _angle.y) * _angle.y) * _length;
    }
}
