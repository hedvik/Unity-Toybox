using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BongoCatManager : MonoBehaviour {
    public Sprite defaultImage;
    public List<Sprite> leftImages = new List<Sprite>();
    public List<Sprite> rightImages = new List<Sprite>();
    public List<Sprite> bothImages = new List<Sprite>();
    public List<Sprite> upImages = new List<Sprite>();
    public List<Sprite> clapImages = new List<Sprite>();

    public AudioClip playbackSong;
    public AudioClip meowClip;
    public AudioClip clapClip;
    public Image imageContainer;

    private List<List<Sprite>> actionImages = new List<List<Sprite>>();
    private List<KeyCode> allowedKeys = new List<KeyCode>();
    private AudioSource sfxPlayer;

    private void Start()
    {
        GetComponent<AudioSource>().PlayOneShot(playbackSong);
        actionImages.Add(leftImages);
        actionImages.Add(rightImages);
        actionImages.Add(bothImages);
        actionImages.Add(upImages);
        actionImages.Add(clapImages);

        allowedKeys.Add(KeyCode.LeftArrow);
        allowedKeys.Add(KeyCode.RightArrow);
        allowedKeys.Add(KeyCode.DownArrow);
        allowedKeys.Add(KeyCode.UpArrow);
        allowedKeys.Add(KeyCode.Space);

        sfxPlayer = GameObject.Find("SFXPlayer").GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckKeyDown();
    }

    void CheckKeyDown()
    {
        int numKeysUp = 0;
        for(int i = 0; i < allowedKeys.Count; i++)
        {
            if(Input.GetKeyDown(allowedKeys[i]))
            {
                imageContainer.sprite = actionImages[i][Random.Range(0, actionImages[i].Count)];

                // TODO: this should just check if the action has a corresponding sound and then play it
                if(allowedKeys[i] == KeyCode.UpArrow)
                {
                    sfxPlayer.PlayOneShot(meowClip);
                } 
                else if (allowedKeys[i] == KeyCode.Space)
                {
                    sfxPlayer.PlayOneShot(clapClip);
                }
            }

            if (!Input.GetKey(allowedKeys[i]))
            {
                numKeysUp++;
            }
        }

        if(numKeysUp == allowedKeys.Count)
        {
            imageContainer.sprite = defaultImage;
        }
    }
}
