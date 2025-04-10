using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OPH.Extend {
    public static class SystemEx {
        /// <summary>
        /// List内部数据交换位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="idx1"></param>
        /// <param name="idx2"></param>
        public static void Swap<T>(this List<T> self, int idx1, int idx2) {
            T temp = self[idx1];
            self[idx2] = self[idx1];
            self[idx1] = temp;
        }
    }
}
