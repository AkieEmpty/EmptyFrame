using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyFrame.Core
{
    /// <summary>
    /// 唯一ID生成器
    /// </summary>
    internal static class IdGenerator
    {
        private static long nextlongId = 1;

        public static long Generate()
        {
            return nextlongId++;
        }
    }
}
