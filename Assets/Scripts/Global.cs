using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class Global : Singleton<MonoBehaviour>
{
    // hold some info global so that it can be accessed at any time
    public static Character[] characters;
    public static Stage[] stages;
    public static Song[] playlist;

    // to be serialized
    public Character[] serializeCharacters;
    public Stage[] serializeStages;
    public Song[] serializePlaylist;

    public GUISkin skin;

    void Awake()
    {
        UnityEngine.Object.DontDestroyOnLoad(this);
        if (characters == null || stages == null || playlist == null)
        {
            characters = serializeCharacters;
            stages = serializeStages;
            playlist = serializePlaylist;
        }
    }

    void Update()
    {
        foreach (ExternalSong s in GameSettings.userPlaylist)
        {
            if (!s.loaded && !s.waitingToLoad && !s.failedToLoad)
            {
                // lets wait for this one first.
                break;
            }
            if (!s.loaded && s.waitingToLoad && !s.failedToLoad)
            {
                s.LoadAudio();
                break;
            }
        }
        int padding = 2;
        int offset = 4;
        foreach(ExternalSong song in GameSettings.userPlaylist)
        {
            if(song.loaded)
            {
                song.Y = -24;
            }
            else
            {
                song.alpha = 1.0f;
                song.Y = offset;
                offset += 24 + padding;
            }
            if (song.waitingToLoad)
            {
                song.alpha = 0.0f;
            }
            song.animation_alpha = Mathf.Lerp(song.animation_alpha, song.alpha, Time.deltaTime * 3.0f);
            song.animation_Y = Mathf.Lerp(song.animation_Y, song.Y, Time.deltaTime * 3.0f);
            if (song.type == "MP3")
            {
                if (song.loaded && !song.MP3_setData)
                {
                    AudioClip audioClip = AudioClip.Create(song.song.name, song.MP3_data.Length / 2, 2, song.MP3_freq, false, false);
                    audioClip.SetData(song.MP3_data, 0);
                    song.song.clip = audioClip;
                    song.MP3_setData = true;
                }
            }
        }
    }

    void OnGUI()
    {
        GUI.depth = -100;
        GUI.skin = skin;
        foreach (ExternalSong song in GameSettings.userPlaylist)
        {
            if (song.song != null)
            {
                if (!(song.loaded && song.animation_Y < 0))
                {
                    GUIStyle style = new GUIStyle(GUI.skin.GetStyle("LabelOdd")) { alignment = TextAnchor.MiddleLeft, fontSize = 15 };
                    Vector2 textSize = style.CalcSize(new GUIContent("100%"));
                    Vector2 textSize2 = style.CalcSize(new GUIContent(song.song.name));
                    GUI.color = Color.white * song.animation_alpha;
                    GUI.Label(new Rect(4, song.animation_Y, textSize.x, 24), song.progress + "%", style);
                    GUI.Label(new Rect(4 + textSize.x, song.animation_Y, textSize2.x, 24), song.song.name, new GUIStyle(style) { alignment = TextAnchor.MiddleRight });
                    GUI.color = Color.white;
                    if (song.animation_Y > 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}