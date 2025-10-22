using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DialogueEditor;

public class QLiteAI : MonoBehaviour
{
    
    [Header("References")]
    public ToySoldierMovement movement;
    public AudioClip LaserGun;

    [Header("Learning Settings")]
    public float learningRate = 0.1f;
    public float discount = 0.9f;
    public float epsilon = 0.3f;//exploration chance
    public float actionDuration = 0.8f; // seconds to stick to an action

    [Header("Behavior Tuning")]
    public float minChaseDistance = 1.2f; // enemy won’t get closer than this

    [Header("Encounter Settings")]
    public float attackDistance = 1.2f;   // distance to trigger life loss
    public float attackCooldown = 8f;     // seconds between attacks
    public float retreatDistance = 5f;    // how far soldier retreats after attack
    public float retreatSpeed = 2f;       // how fast he backs away

    [Header("Debug")]
    public bool debugActions = false;

    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();
    private Vector3[] actions;

    private string savePath;
    private float actionTimer = 0f;
    private int currentAction = 0;
    private float previousDistance = 0f;
    private float attackTimer = 0f;
    private float postConversationTimer = 0f;

    private bool isRetreating = false;
    private Vector3 retreatDirection;

    private void Start()
    {
        if (movement == null)
            movement = GetComponent<ToySoldierMovement>();

        actions = new Vector3[]
        {
            Vector3.zero,    // Idle
            Vector3.forward, // Move forward
            Vector3.back,    // Move back
            Vector3.left,    // Move left
            Vector3.right    // Move right
        };

        savePath = Path.Combine(Application.persistentDataPath, "enemy_qtable.json");
        LoadQTable();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            previousDistance = Vector3.Distance(transform.position, player.transform.position);
    }

    private void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        if (ConversationManager.Instance.IsConversationActive)
        {
            postConversationTimer = 8f; // seconds of pause after conversation
            // Calculate direction away from the player
            Vector3 directionAway = (transform.position - player.transform.position).normalized;
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // If too close, move away
            if (distance < retreatDistance)
            {
                // Move away at normal run speed
                movement.MoveByDirection(directionAway);
            }
            else
            {
                // Once far enough, just idle
                movement.Idle();
            }

            return; // Stop AI logic while conversation is active
        }
        if (postConversationTimer > 0f)
        {
            postConversationTimer -= Time.deltaTime;
            movement.Idle();
            return;
        }

        // Handle attack cooldown
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        // Handle retreat logic
        if (isRetreating)
        {
            movement.MoveByDirection(retreatDirection * retreatSpeed);
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist >= retreatDistance)
                isRetreating = false;
            return; // skip normal AI while retreating
        }

        // Handle encounter (damage + retreat trigger)
        HandleEncounter(player);

        // Handle movement & Q-learning
        actionTimer -= Time.deltaTime;
        if (actionTimer <= 0f)
        {
            actionTimer = actionDuration;

            string state = GetStateKey(player);

            currentAction = ChooseAction(state);

            float reward = ComputeReward(player, currentAction);
            UpdateQValue(state, currentAction, reward);

            previousDistance = Vector3.Distance(transform.position, player.transform.position);

            if (debugActions)
                Debug.Log($"State: {state}, Action: {currentAction}, Reward: {reward}");
        }

        // Execute current action
        Vector3 moveDir;
        if (currentAction == 1) // Move toward player
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minChaseDistance)
                moveDir = Vector3.zero; // pause to reduce aggressiveness
            else
                moveDir = (player.transform.position - transform.position).normalized;
        }
        else
        {
            moveDir = actions[currentAction];
        }

        movement.MoveByDirection(moveDir);
    }

    //Q-Learning Methods 
    private string GetStateKey(GameObject player)
    {
        Vector3 relative = player.transform.position - transform.position;

        float dist = relative.magnitude;
        int distBucket = Mathf.Clamp(Mathf.FloorToInt(dist / 2f), 0, 10);

        float angle = Vector3.SignedAngle(transform.forward, relative, Vector3.up);
        int angleBucket = Mathf.FloorToInt((angle + 180f) / 90f);

        return $"{distBucket}_{angleBucket}";
    }

    private int ChooseAction(string state)
    {
        if (!qTable.ContainsKey(state))
            qTable[state] = new float[actions.Length];

        if (UnityEngine.Random.value < epsilon)
            return UnityEngine.Random.Range(0, actions.Length);

        float[] qValues = qTable[state];
        int best = 0;
        float max = qValues[0];
        for (int i = 1; i < qValues.Length; i++)
        {
            if (qValues[i] > max)
            {
                max = qValues[i];
                best = i;
            }
        }
        return best;
    }

    private float ComputeReward(GameObject player, int actionIndex)
    {
        float currentDistance = Vector3.Distance(transform.position, player.transform.position);

        float reward = previousDistance - currentDistance;

        //discourage idling when very close to player
        if (currentDistance < minChaseDistance && currentAction == 0)
            reward -= 0.3f;

        //discourage idling when far away
        if (actionIndex == 0 && currentDistance > 3f)
            reward -= 0.5f;

        // discourage sticking too close
        if (currentDistance < 2f && actionIndex == 1)
            reward -= 0.5f;

        return reward;
    }

    private void UpdateQValue(string state, int actionIndex, float reward)
    {
        float[] qValues = qTable[state];
        float maxNext = 0f;
        qValues[actionIndex] = qValues[actionIndex] + learningRate * (reward + discount * maxNext - qValues[actionIndex]);
        qTable[state] = qValues;
    }

    // Encounter
    private void HandleEncounter(GameObject player)
    {
        if (attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= attackDistance)
        {
            SoundManager.Instance?.PlaySFX(LaserGun);
            PlayerStats.Instance.TakeDamage(0.2f);

            // start cooldown + retreat
            attackTimer = attackCooldown;
            isRetreating = true;
            retreatDirection = (transform.position - player.transform.position).normalized;
        }
    }

    // Save / Load 
    public void SaveQTable()
    {
        string json = JsonUtility.ToJson(new SerializationWrapper(qTable));
        File.WriteAllText(savePath, json);
        Debug.Log($"Q-Table saved to {savePath}");
    }

    public void LoadQTable()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
        qTable = wrapper.ToDictionary();
        Debug.Log($"Q-Table loaded from {savePath}");
    }

    [Serializable]
    private class SerializationWrapper
    {
        public List<string> keys = new List<string>();
        public List<float[]> values = new List<float[]>();

        public SerializationWrapper(Dictionary<string, float[]> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<string, float[]> ToDictionary()
        {
            var dict = new Dictionary<string, float[]>();
            for (int i = 0; i < keys.Count; i++)
                dict[keys[i]] = values[i];
            return dict;
        }
    }
}



