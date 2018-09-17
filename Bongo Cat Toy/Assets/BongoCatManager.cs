using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BongoCatManager : MonoBehaviour {
    [System.Serializable]
    public class ButtonAction
    {
        public KeyCode actionKey;
        public List<Sprite> actionFrames = new List<Sprite>();
        public AudioClip actionSFX;

        // Might eventually move to having these being runtime generated
        public AudioSource sfxPlayer;
    };

    public Sprite defaultImage;
    public AudioClip playbackSong;
    public Image imageContainer;

    public List<ButtonAction> actions = new List<ButtonAction>();

    private void Start()
    {
        GetComponent<AudioSource>().PlayOneShot(playbackSong);
    }

    private void Update()
    {
        CheckKeyDown();
    }

    void CheckKeyDown()
    {
        int numKeysUp = 0;
        for(int i = 0; i < actions.Count; i++)
        {
            if(Input.GetKeyDown(actions[i].actionKey))
            {
                imageContainer.sprite = actions[i].actionFrames[Random.Range(0, actions[i].actionFrames.Count)];
                actions[i]?.sfxPlayer.PlayOneShot(actions[i]?.actionSFX);
            }

            if (!Input.GetKey(actions[i].actionKey))
            {
                numKeysUp++;
            }
        }

        if(numKeysUp == actions.Count)
        {
            imageContainer.sprite = defaultImage;
        }
    }
}