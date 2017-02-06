using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip theme;
    public AudioClip menu;

	void Start ()
    {
        AudioManager.instance.PlayMusic(menu, 2);
	}
	
	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.instance.PlayMusic(theme, 3);
        }
	}
}
