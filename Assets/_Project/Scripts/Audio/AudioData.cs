using UnityEngine;

public abstract class AudioData : ScriptableObject, IIdentifiable
{
    public string Id;
    public AudioClip AudioClip;

    string IIdentifiable.Id => Id;
}