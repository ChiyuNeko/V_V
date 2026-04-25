using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AccDetect : MonoBehaviour
{
    public Vector3 dir;
    public AudioSource clapAudio;
    public float threashold;
    public TextMeshProUGUI txt;
    // Start is called before the first frame update
    
    void Update()
    {
        dir = Input.acceleration;
        txt.text = dir.ToString();
        if(dir.x >= threashold)
        {
            clapAudio.Play();
        }
    }
}
