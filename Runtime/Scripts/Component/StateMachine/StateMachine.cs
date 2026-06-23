using System;
using System.Collections.Generic;

namespace EmptyFrame
{
    /// <summary>
    /// 状态机
    /// </summary>
    public class StateMachine
    {

        private IStateMachineOwner owner;
        private Type currentStateType;
        private StateBase currentState;


        private Dictionary<Type,StateBase> stateCacheDic = new Dictionary<Type,StateBase>();
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(IStateMachineOwner owner)
        {
            this.owner = owner;
        }
        /// <summary>
        /// 反初始化
        /// </summary>
        public void Uninit()
        {
            Reset();
            owner = null;
            PoolSystem.PushObject(this);
        }
        /// <summary>
        /// 重置状态机
        /// </summary>
        public void Reset()
        {
            //移除状态
            if (currentState != null)
            {
                currentState.Exit();
                MonoSystem.RemoveUpdateListener(currentState.Update);
                MonoSystem.RemoveLateUpdateListener(currentState.LateUpdate);
                MonoSystem.RemoveFixedUpdateListener(currentState.FixedUpdate);
                currentState = null;
            }
            
            currentStateType = null;

            foreach (var state in stateCacheDic.Values)
            {
                state.Uninit();
            }
            stateCacheDic.Clear();
        }
       

        /// <summary>
        /// 更新状态
        /// </summary>
        public bool ChangedState<T>(bool reCurrentState = false) where T : StateBase, new()
        {
            Type stateType = typeof(T);
            // 状态一致，并且不需要刷新状态
            if (stateType == currentStateType && !reCurrentState) return false;

            if (currentState != null)
            {
                currentState.Exit();
                MonoSystem.RemoveUpdateListener(currentState.Update);
                MonoSystem.RemoveLateUpdateListener(currentState.LateUpdate);
                MonoSystem.RemoveFixedUpdateListener(currentState.FixedUpdate);
            }

            currentState = GetState<T>();
            currentStateType = stateType;
            currentState.Enter();
            MonoSystem.AddUpdateListener(currentState.Update);
            MonoSystem.AddLateUpdateListener(currentState.LateUpdate);
            MonoSystem.AddFixedUpdateListener(currentState.FixedUpdate);

            return true;
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        private StateBase GetState<T>() where T : StateBase , new()
        {
            Type stateType = typeof(T);

            if (stateCacheDic.TryGetValue(stateType, out StateBase state)) return state;

            state = PoolSystem.GetObject<T>() ?? new T();
            state.InitInternalData(this);
            state.Init(owner);
            stateCacheDic.Add(stateType, state);
            return state;
        }
    }
}
