﻿using LeoDeg.Inventories;
using LeoDeg.Managers;
using LeoDeg.Properties;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace LeoDeg.StateActions
{
    public class StateMachine : MonoBehaviour, IHittable
    {
        [Header ("Base")]
        public State currentState;
        public Scriptables.FloatScriptable deltaTime;
        public Scriptables.FloatScriptable fixedDeltaTime;
        public Transform resetPosition;

        [Header ("Weapon")]
        public Inventory inventory;
        public Transform leftHand;
        public Transform rightHand;

        [Header ("Effects")]
        public ParticleSystem hitEffect;
        public ParticleSystem deathEffect;
        public AudioClip deathSound;
        public AudioClip takeHitSound;

        [Header ("Assign at Start")]
        public bool useNavMeshAgent = false;
        public bool useBoxCollider = false;
        public bool useRigidbody = false;
        public bool useAnimator = false;
        public bool useMaterial = false;
        public bool useRenderer = false;

        [Header ("References")]
        public BoxCollider boxColliderInstance;
        public NavMeshAgent meshAgentInstance;
        public Rigidbody rigidbodyInstance;
        public Animator animatorInstance;
        public Material materialInstance;
        public Renderer rendererInstance;

        [Header ("Properties")]
        public Stats statsProperties;
        public StateProperties stateProperties;

        [Header ("Events")]
        public UnityEvent OnDeath;
        public UnityEvent OnDamage;

        [Header ("Debug")]
        public bool isGod;

        [HideInInspector]
        public Transform transformInstance;


        #region State Machine Methods

        public void SetCurrentState (State state)
        {
            currentState = state;
        }

        public State GetCurrentState ()
        {
            return currentState;
        }

        private void Initialize ()
        {
            transformInstance = GetComponent<Transform> ();

            if (useRigidbody)
                rigidbodyInstance = GetComponent<Rigidbody> ();

            if (useAnimator)
                animatorInstance = GetComponent<Animator> ();

            if (useNavMeshAgent)
                meshAgentInstance = GetComponent<NavMeshAgent> ();

            if (useMaterial)
                materialInstance = GetComponent<Renderer> ().material;

            if (useBoxCollider)
                boxColliderInstance = GetComponent<BoxCollider> ();

            if (useRenderer)
                rendererInstance = GetComponent<Renderer> ();

            statsProperties.SetScore (0);
            statsProperties.SetHealth (100);

            if (this.tag == "Enemy")
            {
                StateMachine player = GameObject.FindGameObjectWithTag ("Player").GetComponent<StateMachine> ();

                if (player != null)
                    OnDeath.AddListener (player.IncreaseScore);
                else Debug.Log ("Player is not found!");
            }

            if (inventory != null)
            {
                inventory.InitializeWeapon (leftHand, rightHand);
            }
        }

        #endregion

        #region Unity Methods

        private void OnEnable ()
        {
            currentState.OnEnter (this);
        }

        private void OnDisable ()
        {
            currentState.OnExit (this);
        }

        private void Awake ()
        {
            Initialize ();
            currentState.OnAwake (this);
        }

        private void Start ()
        {
            currentState.OnStart (this);
        }

        private void FixedUpdate ()
        {
            currentState.OnFixed (this);
        }

        private void Update ()
        {
            currentState.OnUpdate (this);
        }

        private void LateUpdate ()
        {
            currentState.OnLateUpdate (this);
        }

        #endregion

        #region IHittable

        public void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDirection, bool showHitEffect = true)
        {
            if (takeHitSound != null)
                AudioManager.Instance.PlaySound2D (takeHitSound);

            if (showHitEffect)
            {
                Destroy (Instantiate (hitEffect.gameObject, hitPoint, Quaternion.FromToRotation (Vector3.forward, hitDirection)), hitEffect.main.startLifetimeMultiplier);
            }
            TakeDamage (damage);
        }

        public void TakeDamage (float damage)
        {
            if (!isGod)
            {
                statsProperties.AddHealth (-damage);

                if (statsProperties.GetHealth () <= 0)
                {
                    if (!stateProperties.isDead)
                    {
                        Die ();
                    }

                    stateProperties.isDead = true;
                }
            }
        }

        private void Die ()
        {
            if (deathSound != null)
            {
                AudioManager.Instance.PlaySound2D (deathSound);
            }

            if (deathEffect != null)
            {
                Destroy (Instantiate (deathEffect.gameObject, transformInstance.position, Quaternion.identity), deathEffect.main.startLifetimeMultiplier);
            }

            statsProperties.SetHealth (0);

            if (OnDeath != null)
            {
                OnDeath.Invoke ();
            }

            Destroy (this.gameObject);
        }

        public void ResetPosition ()
        {
            if (resetPosition != null)
                transformInstance.position = resetPosition.position;
            else Debug.Log ("StateMachine: reset position is not assign!");
        }

        public void IncreaseScore ()
        {
            Debug.Log ("Increase Player Score");
            statsProperties.AddScore (1);
        }

        #endregion
    }
}