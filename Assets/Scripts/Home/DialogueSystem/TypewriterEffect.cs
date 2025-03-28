using TMPro;
using UnityEngine;
using System.Collections;
using System.Text;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float delay = 0.03f;
    [SerializeField] private TextMeshProUGUI text;
    private Coroutine typingCoroutine;

    public void SetText(string dialogue)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        text.text = "";
        typingCoroutine = StartCoroutine(TypeText(dialogue));
    }

    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            text.maxVisibleCharacters = text.text.Length;
        }
    }

    private IEnumerator TypeText(string dialogue)
    {
        text.text = dialogue;
        text.maxVisibleCharacters = 0;

        StringBuilder sb = new StringBuilder();
        int visibleCharacterCount = 0;
        bool insideTag = false;

        for (int i = 0; i < dialogue.Length; i++)
        {
            char c = dialogue[i];

            if (c == '<')
            {
                insideTag = true;
            }

            sb.Append(c);

            if (c == '>')
            {
                insideTag = false;
                continue;
            }

            if (!insideTag)
            {
                visibleCharacterCount++;
                text.text = sb.ToString();
                text.maxVisibleCharacters = visibleCharacterCount;
                yield return new WaitForSeconds(delay);
            }
        }

        text.maxVisibleCharacters = dialogue.Length;
    }

    public void ResetText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        text.text = "";
    }
}