namespace EmptyFrame
{
    public struct AudioPlayHandle
    {
        public int ID;
        public AudioGroup AudioGroup;
        public static readonly AudioPlayHandle Invalid = default;
        public bool IsValid => ID > 0;
        public AudioPlayHandle(int id, AudioGroup audioGroup)
        {
            ID = id;
            AudioGroup = audioGroup;
        }
    }
}
