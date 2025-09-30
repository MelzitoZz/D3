using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum OrbiterState
{
    Orbiting,
    Charging,
    Retreating,
    WaitingToAttack
}

public class GroupCirclingFollow : MonoBehaviour
{
    public static event System.Action<GroupCirclingFollow > OnOrbiterChargeStart;
    public static event System.Action<GroupCirclingFollow > OnOrbiterChargeEnd; // Or hit confirmation
    public static event System.Action<GroupCirclingFollow > OnOrbiterRetreatEnd;


    private Transform playerTransform;
    private Rigidbody2D rb;
    private EnemyStatus enemyStatus; // Reference to enemy status script

    [Header("Orbital Movement")]
    [SerializeField] private float baseOrbitRadius = 7f; // Base radius, will be dynamic
    [SerializeField] private float orbitSpeed = 4f;
    [SerializeField] private float orbitSmoothTime = 0.5f; // Time to smooth orbit position
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Group Coordination")]
    [SerializeField] private float repulsionForce = 5f; // How strongly orbiters push away from each other
    [SerializeField] private float repulsionRadius = 1.5f; // Radius for repulsion
    [SerializeField] private float maxDynamicOrbitRadius = 12f; // Max radius when many orbiters are present
    [SerializeField] private float minDynamicOrbitRadius = 5f; // Min radius when few orbiters are present

    [Header("Attack Behavior")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDuration = 0.5f;
    [SerializeField] private float retreatSpeed = 7f;
    [SerializeField] private float retreatDuration = 1f;
    [SerializeField] private float damageAmount = 5f;
    [SerializeField] private float attackDetectionRadius = 10f;
    [SerializeField] private float chargeAnticipationTime = 0.2f; // How far ahead to predict player movement
    [SerializeField] private float attackStaggerTime = 1f; // How long to wait if another orbiter just attacked

    [Header("Debug")]
    [SerializeField] private OrbiterState currentState = OrbiterState.Orbiting;

    private Vector2 currentOrbitTargetPosition;
    private float currentOrbitAngle;
    private float timeSinceLastAttack;
    private Vector2 orbitSmoothDampVelocity; // Velocity for SmoothDamp
    private Vector2 chargeTargetPosition; // Store the target for the charge
    private float currentEffectiveOrbitRadius; // The radius actively being used

    private static List<GroupCirclingFollow > allOrbiters = new List<GroupCirclingFollow >();
    private List<GroupCirclingFollow > fellowOrbiters = new List<GroupCirclingFollow >();

    private Coroutine attackSequenceCoroutine; // To stop attack sequence if needed
    private bool canAttack = true; // Flag to control staggered attacks

    void OnEnable()
    {
        allOrbiters.Add(this);
        UpdateFellowOrbiters();
        // Subscribe to other orbiter's attack events to stagger our own
        OnOrbiterChargeStart += HandleOtherOrbiterChargeStart;
    }

    void OnDisable()
    {
        allOrbiters.Remove(this);
        UpdateFellowOrbiters();
        // Unsubscribe
        OnOrbiterChargeStart -= HandleOtherOrbiterChargeStart;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStatus = GetComponent<EnemyStatus>(); // Ensure this component is present
        timeSinceLastAttack = attackCooldown; // Ready to attack initially
        currentEffectiveOrbitRadius = baseOrbitRadius; // Initialize
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found. Please ensure the player has the 'Player' tag.");
            this.enabled = false;
            return;
        }

        UpdateFellowOrbiters();
        // Random initial angle for better distribution
        currentOrbitAngle = Random.Range(0f, 360f);
        // Initialize currentOrbitTargetPosition to avoid a "jump" at start
        currentOrbitTargetPosition = (Vector2)playerTransform.position + new Vector2(Mathf.Cos(currentOrbitAngle * Mathf.Deg2Rad), Mathf.Sin(currentOrbitAngle * Mathf.Deg2Rad)) * currentEffectiveOrbitRadius;
    }

    // Called when another orbiter starts its charge attack
    void HandleOtherOrbiterChargeStart(GroupCirclingFollow  chargingOrbiter)
    {
        if (chargingOrbiter != this && currentState == OrbiterState.Orbiting)
        {
            // If another orbiter attacks, this orbiter has to wait a bit
            StartCoroutine(StaggerAttackCoroutine());
        }
    }

    IEnumerator StaggerAttackCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackStaggerTime);
        canAttack = true;
    }

    void UpdateFellowOrbiters()
    {
        fellowOrbiters.Clear();
        // Use LINQ for cleaner filtering (if performance is critical in a large game, a manual loop might be marginally faster)
        fellowOrbiters.AddRange(allOrbiters.Where(orbiter => orbiter != this && orbiter != null && orbiter.enabled));
    }

    void Update()
    {
        if (playerTransform == null) return;

        HandleStateTransitions();
        RotateTowardsTarget();
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        HandleMovement();
    }

    void HandleStateTransitions()
    {
        switch (currentState)
        {
            case OrbiterState.Orbiting:
                timeSinceLastAttack += Time.deltaTime;
                // Check for attack conditions: cooldown ready, player in range, and allowed to attack
                if (timeSinceLastAttack >= attackCooldown &&
                    Vector2.Distance(transform.position, playerTransform.position) < attackDetectionRadius &&
                    canAttack)
                {
                    // Predict player's future position
                    Vector2 playerVelocity = rb.linearVelocity; // Assuming player also has a Rigidbody2D
                    if (playerTransform.gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D playerRb))
                    {
                        playerVelocity = playerRb.linearVelocity;
                    }
                    chargeTargetPosition = (Vector2)playerTransform.position + playerVelocity * chargeAnticipationTime;

                    attackSequenceCoroutine = StartCoroutine(ChargeAttackSequence());
                }
                break;
            case OrbiterState.WaitingToAttack:
                // Potentially add logic here for what an orbiter does while waiting
                // Maybe it subtly shifts its orbit, or plays an idle animation
                break;
        }
    }

    void HandleMovement()
    {
        switch (currentState)
        {
            case OrbiterState.Orbiting:
                CalculateOrbitPosition(); // This also updates currentEffectiveOrbitRadius
                Vector2 combinedForce = Vector2.zero;

                // Repulsion from fellow orbiters (prevents stacking)
                foreach (GroupCirclingFollow  fellow in fellowOrbiters)
                {
                    if (fellow == null) continue; // Ensure the fellow orbiter still exists
                    float dist = Vector2.Distance(rb.position, fellow.rb.position);
                    if (dist < repulsionRadius && dist > 0.01f) // Avoid division by zero
                    {
                        Vector2 repulsionDirection = (rb.position - fellow.rb.position).normalized;
                        // Inverse square law for stronger repulsion closer up
                        combinedForce += repulsionDirection * (repulsionForce / (dist * dist));
                    }
                }

                Vector2 targetPositionWithRepulsion = currentOrbitTargetPosition + combinedForce;

                // SmoothDamp for smooth movement towards the calculated target
                rb.position = Vector2.SmoothDamp(rb.position, targetPositionWithRepulsion, ref orbitSmoothDampVelocity, orbitSmoothTime, orbitSpeed);
                break;

            case OrbiterState.Charging:
                Vector2 chargeDirection = (chargeTargetPosition - rb.position).normalized;
                rb.MovePosition(rb.position + chargeDirection * chargeSpeed * Time.fixedDeltaTime);
                break;

            case OrbiterState.Retreating:
                // Retreat to a strategic point on the orbit line, not just away
                Vector2 retreatPointOnOrbit = (Vector2)playerTransform.position + (rb.position - (Vector2)playerTransform.position).normalized * currentEffectiveOrbitRadius;
                rb.MovePosition(Vector2.MoveTowards(rb.position, retreatPointOnOrbit, retreatSpeed * Time.fixedDeltaTime));
                // If close to the retreat point, transition early to orbiting
                if (Vector2.Distance(rb.position, retreatPointOnOrbit) < 0.5f)
                {
                    if (attackSequenceCoroutine != null)
                    {
                        StopCoroutine(attackSequenceCoroutine); // Stop the coroutine early
                    }
                    currentState = OrbiterState.Orbiting;
                    timeSinceLastAttack = 0f;
                    OnOrbiterRetreatEnd?.Invoke(this); // Invoke event
                }
                break;
        }
    }


    void RotateTowardsTarget()
    {
        Vector2 lookDirection = (playerTransform.position - transform.position).normalized;

        if (lookDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void CalculateOrbitPosition()
    {
        // Filter out null or disabled orbiters from the group
        List<GroupCirclingFollow > activeOrbiters = allOrbiters.Where(orbiter => orbiter != null && orbiter.enabled && orbiter.currentState == OrbiterState.Orbiting).ToList();
        activeOrbiters.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID())); // Consistent sorting

        float totalOrbitersInGroup = activeOrbiters.Count;
        int myIndexInGroup = activeOrbiters.IndexOf(this);

        if (totalOrbitersInGroup == 0)
        {
            // If somehow alone, just orbit at base radius
            currentEffectiveOrbitRadius = baseOrbitRadius;
            return;
        }

        // Dynamic Orbit Radius based on number of orbiters
        // More orbiters = larger radius to spread them out
        currentEffectiveOrbitRadius = Mathf.Lerp(minDynamicOrbitRadius, maxDynamicOrbitRadius, (totalOrbitersInGroup - 1) / (float)Mathf.Max(1, allOrbiters.Count - 1));
        currentEffectiveOrbitRadius = Mathf.Clamp(currentEffectiveOrbitRadius, minDynamicOrbitRadius, maxDynamicOrbitRadius);


        // Calculate individual orbit angle based on index and time for natural spacing
        float timeFactor = Time.time * orbitSpeed; // Faster overall rotation
        float groupOffset = (myIndexInGroup * (360f / totalOrbitersInGroup));

        // Adding a slight oscillation to the radius for a more "alive" feel
        float wobble = Mathf.Sin(Time.time * orbitSpeed * 2f + myIndexInGroup) * (currentEffectiveOrbitRadius * 0.05f);
        float dynamicRadiusWithWobble = currentEffectiveOrbitRadius + wobble;

        currentOrbitAngle = (timeFactor + groupOffset) % 360f;

        float angleRad = currentOrbitAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * dynamicRadiusWithWobble;
        currentOrbitTargetPosition = (Vector2)playerTransform.position + offset;
    }

    IEnumerator ChargeAttackSequence()
    {
        canAttack = false; // Prevent immediate re-attack
        currentState = OrbiterState.Charging;
        OnOrbiterChargeStart?.Invoke(this); // Invoke event

        yield return new WaitForSeconds(chargeDuration);

        // After charging, transition to retreat
        currentState = OrbiterState.Retreating;
        OnOrbiterChargeEnd?.Invoke(this); // Invoke event (e.g., impact sound)

        // Wait for retreat duration or until close to orbit point
        float retreatTimer = 0f;
        while (retreatTimer < retreatDuration && Vector2.Distance(rb.position, (Vector2)playerTransform.position + (rb.position - (Vector2)playerTransform.position).normalized * currentEffectiveOrbitRadius) > 0.5f)
        {
            retreatTimer += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        currentState = OrbiterState.Orbiting;
        timeSinceLastAttack = 0f; // Reset cooldown
        canAttack = true; // Allow attacking again after full cycle
        OnOrbiterRetreatEnd?.Invoke(this); // Invoke event
    }

    //void OnCollisionEnter2D(Collision2D collision)
    //{
        // Only deal damage if currently charging
        //if (currentState == OrbiterState.Charging && collision.gameObject.CompareTag("Player"))
        //{
            // Assuming player has a Health component or similar
            //PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>(); // You'd need to create this
            //if (playerHealth != null)
            //{
                //playerHealth.TakeDamage(damageAmount);
                // Optionally, immediately retreat after hitting the player
                //if (attackSequenceCoroutine != null)
                //{
                    //StopCoroutine(attackSequenceCoroutine);
                    //attackSequenceCoroutine = StartCoroutine(ChargeAttackSequence()); // Restart to ensure retreat
                //}
            //}
        //}
    //}

    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            // Draw current effective orbit radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, currentEffectiveOrbitRadius);

            // Draw base orbit radius for reference
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, baseOrbitRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackDetectionRadius);

            // Draw repulsion radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, repulsionRadius);

            Gizmos.color = Color.red;
            if (rb != null)
            {
                // Draw a line to the current orbit target position
                Gizmos.DrawLine(transform.position, currentOrbitTargetPosition);

                // Draw charge target
                if (currentState == OrbiterState.Charging)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, chargeTargetPosition);
                    Gizmos.DrawWireSphere(chargeTargetPosition, 0.5f);
                }
            }
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f); // Indicate an error if player not found
        }
    }
}