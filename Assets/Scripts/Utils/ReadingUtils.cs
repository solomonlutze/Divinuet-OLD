using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ReadingUtils
{

    public static char readingBreakCharacter = '^';
    private static int alphaStep = 3;
    public static void HideAllCharacters(TextMeshProUGUI text)
    {

        //SET ALL TO TRANSPARENT
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);

        // Force and update of the mesh to get valid information.
        text.ForceMeshUpdate();
    }

    public static IEnumerator ReadText(TextMeshProUGUI text, float totalReadingTime)
    {
        int currentSegment = 0;
        string[] textSegments = text.text.Split(readingBreakCharacter);
        text.text = string.Join("", textSegments);
        int count = textSegments[currentSegment].Length;
        float fadeInStepTime = alphaStep / 255;
        float timeBetweenFadeIns = totalReadingTime / textSegments.Length - fadeInStepTime;
        int prevCount = 0;
        bool reading = true;

        Color32[] newVertexColors;
        byte currentAlpha = 0;
        while (reading)
        {
            // the below code has been more or less stolen from https://forum.unity.com/threads/have-words-fade-in-one-by-one.525175/
            // it's allowed tho
            while (currentAlpha < 255)
            {
                for (int i = prevCount; i < count; i++)
                {
                    int materialIndex = text.textInfo.characterInfo[i].materialReferenceIndex;
                    // Get the vertex colors of the mesh used by this text element (character or sprite).
                    newVertexColors = text.textInfo.meshInfo[materialIndex].colors32;
                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = text.textInfo.characterInfo[i].vertexIndex;
                    // Get the current character's alpha value.
                    // Set new alpha values.
                    newVertexColors[vertexIndex + 0].a = currentAlpha;
                    newVertexColors[vertexIndex + 1].a = currentAlpha;
                    newVertexColors[vertexIndex + 2].a = currentAlpha;
                    newVertexColors[vertexIndex + 3].a = currentAlpha;
                }
                text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                currentAlpha = (byte)Mathf.Clamp(currentAlpha + alphaStep, 0, 255);
                yield return new WaitForSeconds(fadeInStepTime);
            }
            currentAlpha = 0;
            prevCount = count;
            currentSegment += 1;
            if (currentSegment >= textSegments.Length)
            {
                reading = false;
                break;
            }
            count += textSegments[currentSegment].Length;
            yield return new WaitForSeconds(totalReadingTime / textSegments.Length);
        }
    }
}
