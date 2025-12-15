using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public List<Slider> sliders;
    public List<AudioSource> audioSources;

    void Start()
    {
        int pairCount = Mathf.Min(sliders.Count, audioSources.Count);
        for (int i = 0; i < pairCount; i++)
        {
            int index = i;
            sliders[i].value = audioSources[i].volume;
            sliders[i].onValueChanged.AddListener(value => SetVolume(index, value));
        }
    }

    void SetVolume(int index, float value)
    {
        if (audioSources == null || audioSources.Count == 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, audioSources.Count - 1);
        audioSources[index].volume = value;
    }
}
