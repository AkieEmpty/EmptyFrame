

namespace EmptyFrame
{
    /// <summary>
    /// 状态基类
    /// </summary>
    public abstract class StateBase
    {
        protected StateMachine stateMachine;
        public void InitInternalData(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }
        public virtual void Init(IStateMachineOwner onwner) { }
        public virtual void Uninit()
        {
            stateMachine = null;
            PoolSystem.PushObject(this);
        }
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }

    }
}
