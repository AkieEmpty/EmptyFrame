using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyFrame
{
    public enum SceneLoadState
    {
        None,
        /// <summary>
        /// 加载中
        /// </summary>
        Loading,
        /// <summary>
        /// 等待中
        /// </summary>
        Awaiting,
        /// <summary>
        /// 激活中
        /// </summary>
        Activating,
        /// <summary>
        /// 已完成
        /// </summary>
        Completed           
    }
}
