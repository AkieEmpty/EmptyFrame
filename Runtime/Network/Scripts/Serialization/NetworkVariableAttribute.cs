using System;

namespace EmptyFrame.Network
{
    /// <summary>
    /// 标记需要被 NetworkVariable&lt;T&gt; 包装的自定义类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum)]
    public class NetworkVariableAttribute : Attribute { }
}
