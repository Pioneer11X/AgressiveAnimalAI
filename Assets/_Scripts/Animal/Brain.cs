using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Exception = System.Exception;

public enum AnimalState { EATING, FLEEING, ATTACKING, SEEKING, WANDERING, SEEKING_ANIMAL };

public enum AILevel {  DUMB , SMART };

public class Brain : MonoBehaviour {


    [Header("Sensory Variables")]
    [Range(0.0f, 20.0f)]
    public float animalSight;

    public LayerMask animalLayer;
    public LayerMask obstacleLayer;
    public LayerMask foodLayer;
    public string foodTag = "Food";

    [Space(20)]
    [Header("AI State")]
    [SerializeField]
    private AILevel currentAI = AILevel.SMART;

    [Space(20)]
    [Header("State Variables")]
    public AnimalState currentState;
    [SerializeField]
    private GameObject animalToSeek;

    [Space(20)]
    [Header("Animal attributes")]
    public float speed = 5.0f;
    public float maxHealth = 100.0f;
    public float currentHealth;

    [Space(10)]
    [Header("Speed, Rate Variables")]
    public float eatRate = 10.0f;
    public float attackDamage = 5.0f;



    [Space(45)]
    // Local variables initialized once.
    [Header("Debug Data")]

    [SerializeField]
    private Collider[] hits;

    [SerializeField]
    private List<GameObject> nearbyAnimals = new List<GameObject>();

    [SerializeField]
    private List<GameObject> nearbyFood = new List<GameObject>();

    [SerializeField]
    private List<GameObject> nearbyObstacles = new List<GameObject>();

    [Space(10)]
    [Header("Debug Data for Obstacle Avoidance")]
    public Transform dummyTarget;
    public LayerMask obstacleAvoidanceLayerMask;

    public Vector3 targetPosition;

    [SerializeField]
    private Vector3 shootDirection;
    [SerializeField]
    private Vector3 swerveDirection;

    [SerializeField]
    private float distance;

    [SerializeField]
    private RaycastHit[] rayHits;

    Pathfinding pathFinder;

    [SerializeField]
    List<Node> pathToTraverse;

    [Header("Variables for making sure the animal isn't static for long time")]
    /* Variables for checking if the animal is static for the last few seconds */
    public float idleMaxTimer = 5.0f;
    public float minimumDistance = 1.0f;
    [SerializeField]
    private float idleTimer;
    [SerializeField]
    private Vector2 difference = Vector2.zero;
    [SerializeField]
    private Vector3 positionLastFrame;

    [Space(20)]
    [Header("Fleeing Logic Variables")]
    [SerializeField]
    private Vector3 fleeingFromAverage;
    [SerializeField]
    private Vector3 fleeDirection;
    [SerializeField]
    private Vector3 fleeTarget;
    [SerializeField]
    private Transform fleeVisualizer;

    [Space(20)]
    [Header("Variables to make sure the animal gives up its chase in favor of food")]
    [SerializeField]
    private GameObject attackTarget;
    [SerializeField]
    private GameObject foodGuarding;
    [SerializeField]
    private float distanceFromFoodGuarding;

    [Space(20)]
    [Header("Animation Variables")]
    public AnimationClip idleAnimation;
    public AnimationClip runAnimation;
    public AnimationClip attackAnimation;
    public AnimationClip dieAnimation;
    public float deathTimer = 2.0f;
    [SerializeField]
    private float deathTimeCounter = 0.0f;

    // Use this for initialization
    void Start() {

        currentState = AnimalState.WANDERING;

        pathFinder = GetComponent<Pathfinding>();

        if (0 == currentHealth)
            currentHealth = maxHealth;

        positionLastFrame = transform.position;

        idleTimer = 0;

    }

    // Update is called once per frame
    void Update() {

        currentAI = MouseSelect.instance.currentAI;

        // Interrupt if you are dead.
        if (0 >= currentHealth)
        {
            // Animal's dead.
            deathTimeCounter += Time.deltaTime;
            if (null != GetComponent<Animation>())
            {
                GetComponent<Animation>().Play("Die");
            }

            if (deathTimeCounter < deathTimer)
            {
                return;
            }
            else
            {
                Destroy(gameObject);
            }

        }

        // Try to move him there by avoiding obstacles.
        LookAround();

        // Soft reset.
        CheckIfTheAnimalIsNotMoving();

        switch (currentState)
        {

            case AnimalState.ATTACKING:

                if (attackTarget == null || attackTarget.transform == null)
                {
                    if (foodGuarding != null && foodGuarding.transform != null)
                    {
                        currentState = AnimalState.SEEKING;
                    }
                }

                if (0.2 * maxHealth < currentHealth)
                {
                    // Actual attack.
                    if (attackTarget != null && attackTarget.transform != null)
                    {
                        AttackAnimal(attackTarget);
                    }

                }
                else
                {
                    currentState = AnimalState.FLEEING;
                }

                break;


            case AnimalState.EATING:

                GuardAndEatFood();

                break;


            case AnimalState.FLEEING:


                if (nearbyAnimals.Count < 1)
                {
                    currentState = AnimalState.WANDERING;
                    GetRandomTarget();
                    break;
                }

                fleeingFromAverage = Vector3.zero;
                // Get the average transform positions of nearby Animals. Average them and run in the opposite direction.
                foreach (GameObject nearbyAnimal in nearbyAnimals)
                {

                    fleeingFromAverage += nearbyAnimal.transform.position;

                }

                fleeingFromAverage /= nearbyAnimals.Count;

                fleeDirection = (transform.position - fleeingFromAverage).normalized;

                Debug.DrawRay(transform.position, fleeDirection * 5, Color.red);


                fleeTarget = 2 * fleeDirection * Vector3.Distance(transform.position, fleeingFromAverage) + transform.position;
                fleeTarget.x = Mathf.Clamp(fleeTarget.x, -pathFinder.grid.gridWorldSize.x / 2, pathFinder.grid.gridWorldSize.x / 2);
                fleeTarget.z = Mathf.Clamp(fleeTarget.z, -pathFinder.grid.gridWorldSize.y / 2, pathFinder.grid.gridWorldSize.y / 2);
                fleeTarget.y = 0.5f;

                if (null != fleeVisualizer)
                {
                    fleeVisualizer.position = fleeTarget;
                        
                    if ( float.IsNaN(fleeTarget.x) )
                    {
                        Debug.Log(fleeDirection);
                        Debug.Log(Vector3.Distance(transform.position, fleeingFromAverage));
                        Debug.Log(fleeTarget);
                        Debug.Break();
                    }
                }
                
                // AvoidObstaclesAndSeek (-1 * fleeDirection );
                AvoidObstaclesAndSeek(fleeTarget);

                break;


            case AnimalState.SEEKING:

                if ( Vector3.Distance(transform.position, targetPosition) < 1.0f)
                {
                    currentState = AnimalState.EATING;
                }

                GuardAndEatFood();

                break;


            case AnimalState.SEEKING_ANIMAL:

                // Check if you are running too far from the food. You do not want to do that.
                if (null != foodGuarding && null != foodGuarding.transform)
                {

                    distanceFromFoodGuarding = Vector3.Distance(transform.position, foodGuarding.transform.position);
                    // Do not let the food out of your sight.
                    if (distanceFromFoodGuarding > animalSight)
                    {
                        currentState = AnimalState.SEEKING;
                        AvoidObstaclesAndSeek(foodGuarding.transform.position);
                    }

                }
                else
                {
                    // Why are we chasing this idiot?
                    currentState = AnimalState.WANDERING;
                }

                if (null == animalToSeek || null == animalToSeek.transform)
                {
                    if (targetPosition != null)
                    {
                        currentState = AnimalState.SEEKING;
                    }
                    else
                    {
                        currentState = AnimalState.WANDERING;
                    }
                    return;
                }

                AvoidObstaclesAndSeek(animalToSeek.transform.position);

                break;


            case AnimalState.WANDERING:

                Wander();

                if (nearbyFood.Count >= 1)
                {
                    currentState = AnimalState.SEEKING;
                }

                break;

        }
   

    }

    void GuardAndEatFood()
    {

        // We seek to the nearest food available.
        try
        {
            for (int i = 0; i < nearbyFood.Count; i++)
            {

                if (nearbyFood.Count > 0)
                {

                    if (null != nearbyFood[0].transform)
                    {
                        targetPosition = nearbyFood[i].transform.position;

                        // Check if there is any animal nearby the food.
                        Collider[] nearestAnimals = (Physics.OverlapSphere(targetPosition, animalSight, animalLayer).OrderBy(h => Vector3.Distance(targetPosition, h.transform.position)).ToArray());
                        GameObject g = null;

                        foreach (Collider c in nearestAnimals)
                        {
                            if (c.gameObject != transform.gameObject)
                            {
                                g = c.gameObject;
                                break;
                            }
                        }

                        if (g != null && g.transform != null)
                        {
                            if (0.7 * maxHealth < currentHealth)
                            {
                                animalToSeek = g;
                                foodGuarding = nearbyFood[0];
                                currentState = AnimalState.SEEKING_ANIMAL;
                                break;
                            }
                            else if (0.2 * maxHealth > currentHealth)
                            {
                                nearbyFood.RemoveAt(0);
                                // Add logic to flee from that animal and look food elsewhere.
                                currentState = AnimalState.FLEEING;
                            }
                        }

                    }
                    else if (nearbyFood.Count >= 1 && null != nearbyFood[0])
                    {
                        nearbyFood.RemoveAt(0);
                    }
                    else if (nearbyFood.Count == 0)
                    {
                        currentState = AnimalState.WANDERING;
                        return;
                    }

                }

            }

            if (AnimalState.SEEKING == currentState)
                AvoidObstaclesAndSeek(targetPosition);
        }
        catch (System.Exception e)
        {
            currentState = AnimalState.WANDERING;
            Debug.LogException(e);
            return;
        }


    }

    void CheckIfTheAnimalIsNotMoving(){

        if( AnimalState.WANDERING != currentState)
        {
            return;
        }

		if (idleTimer < idleMaxTimer) {

			difference.x += (transform.position.x - positionLastFrame.x);
			difference.y += (transform.position.z - positionLastFrame.z);

			positionLastFrame = transform.position;

			idleTimer += Time.deltaTime;

		} else {

			if (Vector2.SqrMagnitude (difference) < minimumDistance * minimumDistance) {
                currentState = AnimalState.WANDERING;
				GetRandomTarget ();
			}

			idleTimer = 0;

			difference = Vector2.zero;

		}


	}

    void Wander()
    {

		// Check if the current target is not good
		// Is it out of bounds?
		if (Mathf.Abs (targetPosition.x) > pathFinder.grid.gridWorldSize.x/2 || Mathf.Abs (targetPosition.z) > pathFinder.grid.gridWorldSize.y/2 ) {
			GetRandomTarget ();
		}

		// Is it set? and is it too close to the current position.
		if (Vector3.zero != targetPosition && ((Mathf.Abs (transform.position.x - targetPosition.x) + Mathf.Abs (transform.position.z - targetPosition.z)) > 1.5f)) {

			// Check if the current ttarget is not reachable / walkable.
			Node n = pathFinder.grid.GetNodeFromWorldPoint (targetPosition);

			if (n.walkable) {

				AvoidObstaclesAndSeek (n.worldPos);

			} else {
				GetRandomTarget ();
			}

		} else {
			GetRandomTarget ();
		}

    }

	void GetRandomTarget(){

		float currentPosX = transform.position.x;
		float currentPosZ = transform.position.z;

		float targetPosX = currentPosX + Random.Range (-1 * animalSight, animalSight);
		float targetPosZ = currentPosZ + Random.Range (-1 * animalSight, animalSight);

		targetPosition = new Vector3 (targetPosX, transform.position.y, targetPosZ);

	}


    // Obstacle Avoidance and Movement.
	void AvoidObstaclesAndSeek(Vector3 targetDestination)
    {

        switch (currentAI)
        {
            case AILevel.SMART:

                if (null != GetComponent<Animation>())
                {
                    GetComponent<Animation>().Play("RunCycle");
                }

                pathToTraverse = pathFinder.FindPath(transform.position, targetDestination);


                if (null != pathToTraverse && pathToTraverse.Count >= 1)
                {

                    Node n = pathToTraverse[0];

                    transform.LookAt(n.worldPos);
                    transform.position = Vector3.MoveTowards(transform.position, n.worldPos, speed * Time.deltaTime);

                    if (transform.position == n.worldPos)
                    {
                        pathToTraverse.Remove(n);
                    }

                }

                break;

            case AILevel.DUMB:

                // TODO: Emulate the steering behaviour from Craig Reynolds website.
                // You cannot use velocities and rigidbodies because that means you should be using Fixed Update and you need to refactor the entire Brain for that to work.

                // Ray cast right in front of you. If there is something right in front of you, turn right or left.
                shootDirection = -1 * (transform.position - targetDestination).normalized;
                distance = Vector3.Distance(targetDestination, transform.position);

                Debug.DrawRay(transform.position, shootDirection * distance, Color.cyan);

                List<RaycastHit> hits = Physics.RaycastAll(transform.position, shootDirection, distance, obstacleAvoidanceLayerMask).OrderBy(h=>h.distance).ToList();

                if ( hits.Count >=1 && hits[0].transform.gameObject == transform.gameObject)
                {
                    hits.RemoveAt(0);
                }

                if ( hits.Count >= 1)
                {

                    // You hit a roadbloack.
                    // Move right or left.
                    // Check that the block is not your target..


                    bool swerve = true; // Swerve/Manoveur if you are about to hit something that is not your target.

                    switch ( currentState)
                    {
                        case AnimalState.SEEKING_ANIMAL:
                            if ( hits[0].transform.gameObject == animalToSeek)
                            {
                                swerve = false;
                            }
                            break;

                        case AnimalState.ATTACKING:
                            // Why is the control even here?
                            swerve = false;
                            break;

                        case AnimalState.EATING:
                            // Again, it shouldn't be here.
                            swerve = false;
                            break;

                        case AnimalState.FLEEING:
                            // You swerve no matter what.
                            break;

                        case AnimalState.SEEKING:
                            // You shouldn't be here anyway.
                            // Seeking food.
                            if ( (hits[0].transform.gameObject == foodGuarding) || (hits[0].transform.gameObject == nearbyFood[0]) )
                            {
                                swerve = false; 
                            }
                            break;

                        case AnimalState.WANDERING:
                            // Swerve always..
                            break;
                    }

                    if ( swerve)
                    {
                        // Move right or left.
                        swerveDirection = Vector3.Cross(shootDirection, Vector3.up).normalized;
                        transform.LookAt(transform.position + swerveDirection * 2);
                        transform.position = Vector3.MoveTowards(transform.position, transform.position + swerveDirection * 2, speed * Time.deltaTime);
                        break;
                    }

                    // Debug.Break();

                }

                // Move forward if no changing of direction needed.

                if (null != GetComponent<Animation>())
                {
                    GetComponent<Animation>().Play("RunCycle");
                }
                transform.LookAt(targetDestination);
                try
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetDestination, speed * Time.deltaTime);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    Debug.Break();
                }

                break;

            default:

                break;

        }

        


    }


    // Sensory Input.
    void LookAround()
    {

        hits = Physics.OverlapSphere(transform.position, animalSight, obstacleLayer);
		nearbyObstacles.Clear ();
        foreach ( Collider hit in hits)
        {
            nearbyObstacles.Add(hit.gameObject);
        }


		hits = Physics.OverlapSphere(transform.position, animalSight, foodLayer).OrderBy(h=>Vector3.Distance(h.transform.position, transform.position)).ToArray();
		nearbyFood.Clear ();
        foreach (Collider hit in hits)
        {
			nearbyFood.Add(hit.gameObject);
        }


		hits = Physics.OverlapSphere (transform.position, animalSight, animalLayer).OrderBy(h=>Vector3.Distance(h.transform.position, transform.position)).ToArray();
		nearbyAnimals.Clear ();
        foreach (Collider hit in hits)
        {
			if ( hit.gameObject != transform.gameObject && !(nearbyAnimals.Contains(hit.gameObject)))
            	nearbyAnimals.Add(hit.gameObject);
        }

    }

    public AnimalState GetCurrentState()
    {
        return currentState;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(gameObject.tag))
        {
            // They are both animals.
            // They both have to stop seeking each other and start attacking each other.
            currentState = AnimalState.ATTACKING;
            attackTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(gameObject.tag))
        {
            if ( currentState == AnimalState.ATTACKING)
            {
                // They are both animals.
                // They both have to stop seeking each other and start attacking each other.
                if (foodGuarding != null && foodGuarding.transform != null)
                {
                    currentState = AnimalState.SEEKING;
                }
                else
                {
                    currentState = AnimalState.WANDERING;
                }
            }
        }
    }

    void AttackAnimal(GameObject _targetAnimal)
    {

        if ( null == _targetAnimal.GetComponent<Brain>())
        {
            return;
        }

        transform.LookAt(_targetAnimal.transform);

        // TODO: Face the target.
        if (null != GetComponent<Animation>())
        {

            // TODO: Do not initialise these guys in the loop. Take them out. Maybe remove the randomization as well.
            int randInt = Random.Range(1, 3);
            // string animationName = "Attack_" + randInt;

            GetComponent<Animation>().Play("Attack_1");
        }

        // TODO: Change this to sync with the animation and discrete chunks of attacks.
        _targetAnimal.GetComponent<Brain>().currentHealth -= Time.deltaTime * attackDamage;

    }
}
