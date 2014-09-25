using Mp3Sharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class ExternalSong : IDisposable
{
    public Song song;
    public string filename;
    public string error;
    public bool loaded = false;
    public bool failedToLoad = false;
    public bool waitingToLoad = true;
    public string failedToLoadReason;
    public int progress = 0;
    public bool MP3_setData = false;
    public float[] MP3_data;
    public int MP3_freq;
    public string type;
    // notification
    public float alpha = 0f;
    public float animation_alpha = 0f;
    public float animation_Y = 0f;
    public float Y = 0f;

    public ExternalSong(string filename)
    {
        this.filename = filename;
        song = new Song();
        song.associated = this;
        song.name = System.IO.Path.GetFileNameWithoutExtension(filename);
    }

    public void LoadAudio()
    {
        waitingToLoad = false;
        try
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                song.clip = GetAudioClipFromStream(stream);
            }
        }
        catch(Exception e)
        {
            error = e.Message;
            failedToLoad = true;
            failedToLoadReason = e.Message;
            throw;
        }
    }

    public void Dispose()
    {
        UnityEngine.AudioClip.Destroy(song.clip);
    }

    private AudioClip GetAudioClipFromStream(Stream inData)
    {
        type = "MP3";
        using (BinaryReader reader = new BinaryReader(inData))
        {
            byte[] bytes = reader.ReadBytes(4);
            // ogg magic number
            if (bytes[0] == 0x4F && bytes[1] == 0x67 && bytes[2] == 0x67 && bytes[3] == 0x53)
            {
                type = "OGG";
            }
            // IS OGG
            if (type.Equals("OGG"))
            {
                return GetAudioClipFromStream_OGG(inData);
            }
        }
        // ACTUALLY ISN'T OGG
        // load on thread
        ThreadPool.QueueUserWorkItem(LoadMp3);
        return null;
    }

    private AudioClip GetAudioClipFromStream_OGG(Stream inData)
    {
        WWW www = new WWW("file://" + this.filename);
        while (!www.isDone)
        {

        }
        while(www.progress != 1.0f)
        {

        }
        loaded = true;
        return www.GetAudioClip(false, true, AudioType.OGGVORBIS);
    }

    private void LoadMp3(object state)
    {
        // create mp3
        using (FileStream f_stream = new FileStream(filename, FileMode.Open))
        {
            Mp3Stream stream = new Mp3Sharp.Mp3Stream(f_stream, 65536);
            MemoryStream convertedAudioStream = new MemoryStream();
            byte[] buffer = new byte[65536];
            int bytesReturned = -1;
            int totalBytesReturned = 0;

            while (bytesReturned != 0)
            {
                bytesReturned = stream.Read(buffer, 0, buffer.Length);
                convertedAudioStream.Write(buffer, 0, bytesReturned);
                totalBytesReturned += bytesReturned;
                this.progress = (int)(((float)stream.Position / (float)stream.Length) * 100);
                this.progress = Mathf.Clamp(this.progress, 0, 100);
            }

            Debug.Log("MP3 file has " + stream.ChannelCount + " channels with a frequency of " + stream.Frequency);
            byte[] convertedAudioData = convertedAudioStream.ToArray();
            Debug.Log("Converted Data has " + convertedAudioData.Length + " bytes of data");
            MP3_data = new float[convertedAudioData.Length / 2];
            for (int i = 0; i < MP3_data.Length; i++)
            {
                MP3_data[i] = (float)(BitConverter.ToInt16(convertedAudioData, i * 2) / (float)short.MaxValue);
            }
            MP3_freq = stream.Frequency;
            stream.Dispose();
            loaded = true;
        }
    }
}