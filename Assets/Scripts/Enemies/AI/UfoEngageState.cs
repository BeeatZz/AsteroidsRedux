using UnityEngine;
using Asteroids.Enemies.AI;

namespace Asteroids.Enemies.AI
{
    public class UfoEngageState : IState
    {
        private readonly UfoController ufo;
        private readonly StateMachine stateMachine;
        private float fireTimer;
        private float directionChangeTimer;
        private Vector2 moveDirection;

        public UfoEngageState(UfoController ufo, StateMachine stateMachine)
        {
            this.ufo = ufo;
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            fireTimer = 0f;
            ChooseEngageVector();
        }

        public void Update()
        {
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                ChooseEngageVector();
            }

            fireTimer += Time.deltaTime;
            if (fireTimer >= ufo.FireRate)
            {
                fireTimer = 0f;
                ufo.ShootAtPlayer();
            }

            ufo.WrapScreen();
        }

        public void FixedUpdate()
        {
            ufo.Move(moveDirection);
        }

        public void Exit() { }

        private void ChooseEngageVector()
        {
            directionChangeTimer = Random.Range(1.2f, 2.5f);

            Vector2 toPlayer = ufo.GetDirectionToPlayer();
            Vector2 perpendicular = new Vector2(-toPlayer.y, toPlayer.x) * (Random.value > 0.5f ? 1f : -1f);

            moveDirection = (toPlayer * 0.6f + perpendicular * 0.4f).normalized;
        }
    }
}