using System.Collections.Generic;
using UnityEngine;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 层级管理器：根据窗口定义的 Layer 将窗口分发到对应 UILayer
    /// </summary>
    internal class UILayerManager
    {
        private readonly List<UILayer> layerList;

        public UILayerManager(UILayer[] layers)
        {
            layerList = new List<UILayer>(layers.Length);
            for (int i = 0; i < layers.Length; i++)
                layerList.Add(layers[i]);
        }

        /// <summary>
        /// 获取窗口应挂载的父节点
        /// </summary>
        public Transform GetParent(UIWindowDefinition definition)
        {
            return GetLayer(definition.Layer).transform;
        }

        /// <summary>
        /// 将窗口添加到对应层级
        /// </summary>
        public void Add(UIWindowDefinition definition, UIWindowInstance instance)
        {
            UILayer layer = GetLayer(definition.Layer);
            layer.AddWindow(instance.Window);
        }

        /// <summary>
        /// 将窗口从对应层级移除
        /// </summary>
        public void Remove(UIWindowDefinition definition, UIWindowInstance instance)
        {
            UILayer layer = GetLayer(definition.Layer);
            layer.RemoveWindow(instance.Window);
        }

        private UILayer GetLayer(int layerIndex) => layerList[layerIndex];
    }
}
