using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseAnimSync : MonoBehaviour
{
    public GameObject musicPlayer;
    public float minScale = 0.4f;
    public float maxScale = 3.0f;

    public AK.Wwise.RTPC MusicVolume;
    public AK.Wwise.RTPC arpOn;
    public ParticleSystem ps;

    public float volumeThreshold = -6f;

    private void Start()
    {
        
    }

    private void Update()
    {
        VolumeToScale();
        TriggerParticles();

        if (Input.GetKey(KeyCode.Space))
        {
            arpOn.SetValue(musicPlayer, 100f);
        }
        else
        {
            arpOn.SetValue(musicPlayer, 0f);
        }
    }

    public void VolumeToScale()
    {
        float musicVolume = MusicVolume.GetValue(musicPlayer);
        float remappedVolume = Util.remap(musicVolume, -48f, 0f, minScale, maxScale);

        Vector3 newScale = new Vector3(remappedVolume, remappedVolume, remappedVolume);
        gameObject.transform.localScale = newScale;
    }

    public void TriggerParticles()
    {
        float musicVolume = MusicVolume.GetValue(musicPlayer);
        if(musicVolume >= volumeThreshold)
        {
            ps.Play();
        }
    }

}
