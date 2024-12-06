using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterCreation : MonoBehaviour
{
    private List<GameObject> models;
    //
    private int selectionIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        models = new List<GameObject>();
        foreach (Transform t in transform)
        {
            models.Add(t.gameObject);
            t.gameObject.SetActive(false);

        }

        models[selectionIndex].SetActive(true);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            Select(2);
    }

    // Update is called once per frame
    public void Select(int index)
    {
        if (index == selectionIndex)
            return;

        if (index < 0 || index >= models.Count)
            return;

        models[selectionIndex].SetActive(false);
        selectionIndex = index;
        models[selectionIndex].SetActive(true);


    }
}