using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    // The name of the scene to load when the player confirms
    public string levelToLoad;

    // Array of star images for showing level completion status
    public Image[] stars;

    private int starsActive;

    // The level number associated with this confirmation panel
    public int level;

    private GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        gameData = FindObjectOfType<GameData>();
        LoadData();
        // Call the method to initially hide all the stars in the panel
        ActivateStars();
    }

    void LoadData()
    {
        if (gameData != null)
        {
            starsActive = gameData.saveData.stars[level - 1];
        }
    }

    // This method deactivates all the star images (making them invisible)
    void ActivateStars()
    {
        // Loop through all the stars and disable them (set them to not visible)
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
        }
    }

    // Method to handle the cancel action - hides the confirmation panel
    public void Cancel()
    {
        // Disable the game object (confirmation panel), effectively hiding it
        this.gameObject.SetActive(false);
    }

    // Method to handle the play action - sets the current level and loads the corresponding scene
    public void Play()
    {
        // Set the current level in PlayerPrefs to the previous level (level - 1), for tracking purposes
        PlayerPrefs.SetInt("Current Level", level - 1);

        // Load the scene associated with the level to load
        SceneManager.LoadScene(levelToLoad);
    }
}