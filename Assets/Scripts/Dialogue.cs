using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public string[] lines;
    public float speedText;
    public Text dialogueText;

    public int index;

    // Start is called before the first frame update
    void Start()
    {
        dialogueText.text = string.Empty;
        StartDialogue();


    }
    void StartDialogue()
    { 
        index = 0;
        StartCoroutine(TypeLine());
    }
    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speedText);


        }
    }
    // Update is called once per frame
    public void scipText()
    {
        if(dialogueText.text == lines[index])
        {
            NextLines();
        }
        else
        {
            StopAllCoroutines();
            dialogueText.text = lines[index];
        }
    }
    private void NextLines()
    {
        if (index < lines.Length - 1)
        {
            index++;
            dialogueText.text = string.Empty;
            StartCoroutine (TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    
    }  
    

}
