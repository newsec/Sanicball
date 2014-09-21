using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// taken from Improved Sanicball (used for loading and in the leaderboard for avatars)
// an object is used for animating
public class Spinner : MonoBehaviour
{
    public Texture2D spinner;
    public static Spinner instance;
    private float frame;
    private const double frameTime = 31;
    private static List<Texture2D> frames = new List<Texture2D>();

    public void Start()
    {
        instance = this;
        if(frames.Count == 0)
        {
            //split the texture into frames
            for (int i = 0; i < spinner.width / 32; i++)
            {
                Texture2D currentFrame = new Texture2D(32, 32);
                currentFrame.SetPixels(spinner.GetPixels(32 * i, 0, 32, 32));
                currentFrame.Apply();
                frames.Add(currentFrame);
            }
        }
    }

    public void Update()
    {
        frame += (float) (frameTime * Time.deltaTime);
        if((int)frame > frames.Count-1)
        {
            frame = 0;
        }
    }

    public static void Draw(string text = "", Texture2D customPicture = null, int fontSize = 0, GUIStyle customStyle = null)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        if (customStyle != null)
        {
            style = new GUIStyle(customStyle);
        }
        style.fontSize += fontSize;
        style.wordWrap = true;
        Rect rect = GUILayoutUtility.GetRect(32, 34);
        // 2px offset
        style.fontSize += 2;
        // draw a background behind it
        GUI.Label(rect, "    ", style);
        style.fontSize -= 2;
        if (customPicture == null)
        {
            GUI.DrawTexture(new Rect(rect.x + 8, rect.y, 32, 32), frames[(int)instance.frame]);
        }
        else
        {
            GUI.DrawTexture(new Rect(rect.x + 8, rect.y, 32, 32), customPicture);
        }
        GUI.Label(new Rect(rect.x + 8 + 32, rect.y + (int)((-fontSize) / 2F), Screen.width - rect.x + 8 + 32, 34), text, style);
    }

    public static void DrawAt(Vector2 pos)
    {
        GUI.DrawTexture(new Rect(pos.x, pos.y, 32, 32), frames[(int)instance.frame]);
    }
}