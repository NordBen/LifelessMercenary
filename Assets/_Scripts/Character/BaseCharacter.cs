using System;
using UnityEngine;

namespace LM
{
    public abstract class BaseCharacter : Entity
    {
        protected readonly int IsDeadHash = Animator.StringToHash("bIsDead");
        protected readonly int DieHash = Animator.StringToHash("tDead");
        
        [Header("Movement")] [Tooltip("Walk speed")] [Range(0.1f, 10f)] [SerializeField]
        protected float walkSpeed = 0.6f;

        [Tooltip("Run speed")] [Range((int)0.1f, 20f)] [SerializeField]
        protected float runSpeed = 1.5f;

        [Tooltip("Knockback, how much default knockback is dealt in attacks")] [Range(0.1f, 10f)] [SerializeField]
        private float knockbackImpulse = 6;
        
        public event Action<int> OnHealingUsed;

        protected float _currentSpeed;
        protected Animator _animator;
        protected bool _isDead = false;
        protected int healing = 0;

        protected void Start()
        {
            this._currentSpeed = this.walkSpeed;
            _animator = GetComponent<Animator>();
        }

        protected void Update()
        {
            if (this._isDead) return;
        }
        
        public virtual void Die()
        {
            _isDead = true;
        }

        public bool IsDead() => this._isDead;
    }
}