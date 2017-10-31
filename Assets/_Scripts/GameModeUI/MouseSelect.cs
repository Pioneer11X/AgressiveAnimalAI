using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enum = System.Enum;

public class MouseSelect : MonoBehaviour {


    public static MouseSelect instance = null;

    [Header("Mouse select helpers")]
    [SerializeField]
    private GameObject selectedGameObject;

    public LayerMask animalLayer;

    public string animalTag;

    [Space(20)]
    [Header("UI Variables")]
    public Text nameText;
    public Text healthText;
    public Text StateText;
    public Dropdown AISelect;

    [Space(20)]
    [Header("AI State")]
    public AILevel currentAI;

    private void Awake()
    {
        if ( null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // TODO: Add dont destroy if we have multiple levels.

    }

    // Use this for initialization
    void Start () {

        // TODO: Clean up this mess.

        //AISelect.ClearOptions();

        //List<AILevel> list = new List<AILevel>();

        //foreach ( AILevel lev in Enum.GetValues(typeof(AILevel)))
        //{

        //    list.Add(lev);

        //}

        //AISelect.AddOptions(list);

        OnChange();

	}

    // Option change from Dropdown.
    public void OnChange()
    {

        switch (AISelect.value)
        {
            case 1:
                currentAI = AILevel.SMART;
                break;
            case 0:
                currentAI = AILevel.DUMB;
                break;
            default:
                currentAI = AILevel.DUMB;
                break;
        }

    }
	
	// Update is called once per frame
	void Update () {
		
        if ( Input.GetMouseButtonDown(0))
        {

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float y = Camera.main.transform.position.y + 20.0f;

            if ( Physics.Raycast(ray,  out hit, y, animalLayer))
            {

                if (hit.collider.CompareTag(animalTag))
                {
                    selectedGameObject = hit.transform.gameObject;
                }

            }

        }

        if ( null != selectedGameObject && null != selectedGameObject.transform)
        {
            nameText.text = "Name: " + selectedGameObject.name;
            healthText.text = "Health: " + Mathf.RoundToInt(selectedGameObject.GetComponent<Brain>().currentHealth);
            StateText.text = "State: " + selectedGameObject.GetComponent<Brain>().currentState.ToString();
        }

	}
}
