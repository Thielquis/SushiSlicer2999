﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ComboList : MonoBehaviour
{
    public static ComboList Instance;
    public ParticleSystem finishedComboEffect;
    public ScoreCanvas scoreCanvas;
   
    public void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private float CurrentPoints = 0;
    private float CurrentComboSum = 0;
    private float CurrentComboMultiplier = 1;
    public float ComboTime;
    public List<EnemyData> myComboList = new List<EnemyData>();
    public List<AudioClip> ComboSounds = new List<AudioClip>();

    public List<Recipe> recipes = new List<Recipe>();
    [System.Serializable]
    public class Recipe
    {
        public EnemyData[] Ingredients;
        public GameObject ResultPrefab;
        public float PointScore
        {
            get
            {
                float sum = 0;
                foreach (EnemyData e in Ingredients)
                {
                    sum += e.score;
                }

                return sum;
            }
        }

        private bool IsContainedIn(List<EnemyData> list)
        {
            if (list == null || Ingredients == null || Ingredients.Length > list.Count)
            {
                return false;
            }

            foreach (EnemyData e in Ingredients)
            {
                if (list.Contains(e))
                {
                    list.Remove(e);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        //checks if recipe is in list fully and removes it, order is not important
        public bool RemoveFrom(List<EnemyData> list)
        {
            List<EnemyData> copy = new List<EnemyData>(list);
            if (IsContainedIn(copy))
            {
                foreach (EnemyData e in Ingredients)
                {
                    list.Remove(e);
                }
                return true;
            }
            else
            {
                return false;
            }


        }
    }

    //sort recipes descending
    private class RecipeSorter : IComparer<Recipe>
    {
        public int Compare(Recipe x, Recipe y)
        {
            float f = -(x.PointScore - y.PointScore);
            if (f < 0)
            {
                return -1;
            }
            else if (f == 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }

    private float timeOfLastIngredient;

    public void Start()
    {
        recipes.Sort(new RecipeSorter());
    }

    public void Update()
    {
        //we have ingredients and time has run out
        if (CurrentComboSum > 0 && timeOfLastIngredient + ComboTime < Time.time)
        {
            //clear current Combo and add points
            ResolveCombo();
        }
    }

    public void AddIngredient(EnemyData enemyData)
    {
        myComboList.Add(enemyData);
        
        timeOfLastIngredient = Time.time;

        //add points of this object
        CurrentComboSum += enemyData.score;

        //check for a combo and remove those ingredients
        CheckCombo();

        if (scoreCanvas)
        {
            scoreCanvas.IngredientsChanged(true);
        }
    }

    private void CheckCombo()
    {
        foreach(Recipe r in recipes)
        {
            bool removed = r.RemoveFrom(myComboList);
            
            //Combo Found
            if(removed)
            {
                CurrentComboMultiplier++;
                
                int random = UnityEngine.Random.Range(0, ComboSounds.Count);
                SoundEffectManager.Instance.CreateSoundEffect(ComboSounds[random]);

                StartCoroutine(spawnSushi(0.5f, r));
            }
        }
    }

    public float GetMultiplier()
    {
        return CurrentComboMultiplier;
    }

    public float GetSum()
    {
        return CurrentComboSum;
    }

    public float GetPoints()
    {
        return CurrentPoints;
    }

    public void ResolveCombo()
    {
        if (!scoreCanvas)
        {
            return;
        }
        //petal effect
        if(CurrentComboMultiplier > 1)
            finishedComboEffect.Play();

        //add combo points to currentPoints
        CurrentPoints += CurrentComboSum * CurrentComboMultiplier;

        CurrentComboSum = 0;
        CurrentComboMultiplier = 1;

        //update list
        myComboList.Clear();

        //update score canvas
        scoreCanvas.IngredientsChanged(true);
    }

    public void EndLevel()
    {
        CurrentPoints += CurrentComboSum * CurrentComboMultiplier;

        CurrentComboSum = 0;
        CurrentComboMultiplier = 1;
    }

    public void OnGUI()
    {
        /*
        float timeToCombo = timeOfLastIngredient + ComboTime - Time.time;
        GUI.Label(new Rect(Screen.width - 100, 50, 100, 100), CurrentComboSum + " x " + CurrentComboMultiplier);
        GUI.Label(new Rect(Screen.width - 100, 70, 100, 100), "Time to combo: " + timeToCombo);*/
    }

    private IEnumerator spawnSushi(float delay, Recipe r)
    {
        yield return new WaitForSeconds(delay);

        //spawn a jumping sushi
        float horizontalPos = UnityEngine.Random.Range(-0.9f, 0.9f) * 800.0f;
        GameObject sushi = Instantiate(r.ResultPrefab);
        sushi.transform.SetParent(scoreCanvas.transform);
        RectTransform rt = sushi.GetComponent<RectTransform>();
        rt.localPosition = new Vector3(horizontalPos, 0, 0);
        rt.localScale = new Vector3(1, 1, 1);

        yield return null;
    }
}
