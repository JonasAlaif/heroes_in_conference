using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicDetect : MonoBehaviour
{
    // Main challenge asset
    public GameObject obj;
    // Environmental game objects
    public GameObject[] environment;
    public GameObject hint;
    public GameObject hintsphere;

    // Microphone requirements
    private string device;

    // 'obj' renderer
    SkinnedMeshRenderer skin;

    // 'obj' animation setup
    Animator anim;
    int scare = Animator.StringToHash("dance");
    bool complete;
    Vector3 moveTo;

    float volume;
    AudioClip record; //clipRecord
    bool isOn;

	//mic initialization
	public void InitMic()
	{
		if (device == null)
		{
			device = Microphone.devices[0];
		}
		record = Microphone.Start(device, true, 999, 44100);
		isOn = true;
	}

	public void StopMicrophone()
	{
		Microphone.End(device);
		isOn = false;
	}

	int sampleSize = 128;

	//get data from microphone into audioclip
	float MicrophoneLevelMax()
	{
		float levelMax = 0;
		float[] waveData = new float[sampleSize];
		int micPosition = Microphone.GetPosition(null) - (sampleSize + 1); // null means the first microphone
		if (micPosition < 0) return 0;
		record.GetData(waveData, micPosition);
		// Getting a peak on the last 128 samples
		for (int i = 0; i < sampleSize; i++)
		{
			float wavePeak = waveData[i] * waveData[i];
			if (levelMax < wavePeak)
			{
				levelMax = wavePeak;
			}
		}
		return levelMax;
	}

	void Update()
	{
        // levelMax equals to the highest normalized value power 2, a small number because < 1
        // pass the value to a static var so we can access it from anywhere
        if (obj.GetComponentInChildren<SkinnedMeshRenderer>().isVisible)
        {

            volume = MicrophoneLevelMax();

            if (volume > 0.04 && !complete && skin.isVisible)
            {
                anim.SetTrigger(scare);
                obj.transform.Rotate(0, 180, 0);
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, obj.transform.position - moveTo, Time.deltaTime * 1200);
                complete = true;
                hint.SetActive(true);
                hintsphere.SetActive(true);
            }

            if (ARHandler.GetHitIfAny().Equals(hintsphere.name))
            {
                // Waiting for finalised achievement list
                Destroy(hint);
                ARHandler.GetAchievement("TNT I'm Zygomite!");
            }
        }    
	}

	// start mic when scene starts
	void Start()
	{
		InitMic();
		isOn = true;
        complete = false;
        anim = obj.GetComponent<Animator>();
        moveTo = new Vector3(0, 0, 0.2f);
        hint.SetActive(false);
        hintsphere.SetActive(false);
        obj.SetActive(!NetworkDatabase.NDB.GetAchievementWonByName("TNT I'm Zygomite!"));
        skin = obj.GetComponentInChildren<SkinnedMeshRenderer>();
    }

	//stop mic when loading a new level or quit application
	void OnDisable()
	{
		StopMicrophone();
	}

	void OnDestroy()
	{
		StopMicrophone();
	}


	// make sure the mic gets started & stopped when application gets focused
	void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			if (!isOn)
			{
				InitMic();
			}
		}
		if (!focus)
		{
			StopMicrophone();
		}
	}
}
