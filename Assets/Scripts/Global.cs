using UnityEngine;
using System.Collections;
using System;

public class Global : MonoBehaviour
{
    // hold some info global so that it can be accessed at any time
    public static Character[] characters;
    public static Stage[] stages;
    public static Song[] playlist;

    // to be serialized
    public Character[] serializeCharacters;
    public Stage[] serializeStages;
    public Song[] serializePlaylist;

    void Start()
    {
        characters = serializeCharacters;
        stages = serializeStages;
        playlist = serializePlaylist;
    }
}