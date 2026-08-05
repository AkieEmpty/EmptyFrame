using System;
using System.Reflection;
using EmptyFrame.Core;
using Unity.Collections;
using Unity.Netcode;

namespace EmptyFrame.Network
{
    /// <summary>
    /// 网络变量序列化
    /// </summary>
    internal static class NetworkVariableSerializtion
    {

        private static readonly Type[] builtInTypes = new Type[]
        {
            typeof(FixedString32Bytes),
            typeof(FixedString64Bytes),
            typeof(FixedString128Bytes),
           
        };
        public static void Init()
        {
            // 内置注册
            foreach (var type in builtInTypes)
            {
                RegisterType(type);
            }

            //扫描程序集中所有带 [NetworkVariable] 标记的类型并注册
            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblys)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute<NetworkVariableAttribute>() == null)
                        continue;

                    RegisterType(type);
                }
            }

        }

        

        /// <summary>
        /// 注册类型
        /// </summary>
        public static void Register<T>()
        {
            RegisterType(typeof(T));
        }
       
        private static void RegisterType(Type type)
        {
            //枚举
            if (type.IsEnum)
            {
                // 注册序列化器：用 memcpy 直接复制内存
                InvokeMethod(type, "InitializeSerializer_UnmanagedByMemcpy");
                // 注册相等性检查：用 memcmp 逐字节比较
                InvokeMethod(type, "InitializeEqualityChecker_UnmanagedValueEquals");
                return;
            }

            //结构体
            if (type.IsValueType)
            {
                InvokeMethod(type, "InitializeSerializer_UnmanagedByMemcpy");

                var equatableInterface = typeof(IEquatable<>).MakeGenericType(type);
                //若结构体实现IEquatable<T>
                if (equatableInterface.IsAssignableFrom(type))
                {
                    //用自定义 Equals 比较
                    InvokeMethod(type, "InitializeEqualityChecker_UnmanagedIEquatable");
                }
                else
                {
                    InvokeMethod(type, "InitializeEqualityChecker_UnmanagedValueEquals");
                }
                return;
            }

            LogSystem.Error($"不支持的网络变量类型: {type.Name}");
        }


        private static void InvokeMethod(Type targetType, string methodName)
        {
            var method = typeof(NetworkVariableSerializationTypes)
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                ?.MakeGenericMethod(targetType);

            if (method == null)
            {
                LogSystem.Error($"反射查找失败: {methodName}<{targetType.Name}>");
                return;
            }

            method.Invoke(null, null);
        }
    }
}
