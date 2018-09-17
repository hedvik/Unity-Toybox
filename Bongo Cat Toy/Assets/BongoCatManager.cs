using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BongoCatManager : MonoBehaviour {
    public Sprite defaultImage;
    public List<Sprite> leftImages = new List<Sprite>();
    public List<Sprite> rightImages = new List<Sprite>();
    public List<Sprite> bothImages = new List<Sprite>();

    public AudioClip playbackSong;
    public Image imageContainer;

    private List<List<Sprite>> actionImages = new List<List<Sprite>>();
    private List<KeyCode> allowedKeys = new List<KeyCode>();

    private void Start()
    {
        GetComponent<AudioSource>().PlayOneShot(playbackSong);
        actionImages.Add(leftImages);
        actionImages.Add(rightImages);
        actionImages.Add(bothImages);

        allowedKeys.Add(KeyCode.LeftArrow);
        allowedKeys.Add(KeyCode.RightArrow);
        allowedKeys.Add(KeyCode.DownArrow);
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
