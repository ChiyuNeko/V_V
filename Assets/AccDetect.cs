using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccDetect : MonoBehaviour
{
    public Vector3 dir;
    public AudioSource clapAudio;
    public Slider slider;
    public float threashold;
    public TextMeshProUGUI txt;
    bool canPlay = true;
    float maxThreashold = 0;
    // Start is called before the first frame update
    
    void Update()
    {
        dir = Input.acceleration;
        if (dir.magnitude > maxThreashold)
        {
            txt.text = dir.magnitude.ToString();
            maxThreashold = dir.magnitude;
        }
        threashold = 5 / (slider.value + 1);

        if(dir.magnitude >= threashold && canPlay)
        {
            clapAudio.Play();
            canPlay = false;
        }
        if (dir.magnitude < threashold)
        {
            canPlay = true;
        }
    }
}
