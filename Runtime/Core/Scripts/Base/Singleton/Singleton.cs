namespace EmptyFrame.Core
{
    /// <summary>
    /// 单例模式基类(普通类)
    /// </summary>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }
}