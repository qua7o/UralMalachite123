using DialogueEditor;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class DDDialogue : MonoBehaviour
{

    // Start is called before the first frame update
    [SerializeField] NPCConversation DedConversation;

    private GameManager counter;
    // Start is called before the first frame update

    private void Start()
    {
        counter = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<XROrigin>() != null && counter.sparks == 1)
        {
            StartDialogue();
        }

    }
    private void StartDialogue()
    {
        ConversationManager.Instance.StartConversation(DedConversation);
    }

}
