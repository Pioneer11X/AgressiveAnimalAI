using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeingEaten : MonoBehaviour {

    [Header("Variables")]
    public float maxHealth;
    public float health;
    public string animalTag = "Animal";

    [SerializeField]
    private Vector3 initialScale;
    

	// Use this for initialization
	void Start () {

        initialScale = transform.localScale;

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag(animalTag))
        {

            Brain b = other.gameObject.GetComponent<Brain>();

            if (null == b)
                return;

            if ( b.GetCurrentState() == AnimalState.EATING)
            {

                if ( health <= 0)
                {
                    b.currentState = AnimalState.WANDERING;
                    Destroy(gameObject);
                }

                health -= b.eatRate * Time.deltaTime;
                b.currentHealth = Mathf.Clamp(b.currentHealth + b.eatRate * Time.deltaTime, 0, b.maxHealth);

                // transform.localScale = new Vector3(initialScale.x * (health / maxHealth), initialScale.y, initialScale.z * (health / maxHealth));

            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(animalTag))
        {

            Brain b = other.gameObject.GetComponent<Brain>();

            if (null == b)
                return;

            if ( b.GetCurrentState() == AnimalState.EATING)
            {
                b.currentState = AnimalState.WANDERING;
            }
        }
    }
}
