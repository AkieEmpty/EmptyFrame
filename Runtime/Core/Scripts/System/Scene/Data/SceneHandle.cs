using System;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 场景实例标识
    /// <para>
    /// 用于唯一标识一个已加载的场景实例。
    /// </para>
    /// </summary>
    public readonly struct SceneHandle : IEquatable<SceneHandle>
    {

        /// <summary>
        /// 唯一标识
        /// </summary>
        internal long Id {  get; }

        internal string SceneKey { get; }

        public bool IsValid => Id != 0;

        internal SceneProvider Provider { get; }

        internal SceneHandle(long id, string sceneKey,SceneProvider provider)
        {
            Id = id;
            SceneKey = sceneKey;
            Provider = provider;
        }
       
        public bool Equals(SceneHandle other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is SceneHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
