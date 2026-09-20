using UnityEngine;
using Asteroids.Enemies.AI;

namespace Asteroids.Enemies.AI
{
    public class UfoEntryState : IState
    {
        private readonly UfoController ufo;
        private readonly StateMachine stateMachine;
        private Vector2 entryDirection;

        public UfoEntryState(UfoController ufo, StateMachine stateMachine)
        {
            this.ufo = ufo;
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            entryDirection = ufo.GetDirectionToScreenCenter();
        }

        public void Update()
        {
            if (ufo.IsInsideScreen())
            {
                stateMachine.ChangeState(ufo.EngageState);
            }
        }

        public void FixedUpdate()
        {
            ufo.Move(entryDirection);
        }

        public void Exit() { }
    }
}