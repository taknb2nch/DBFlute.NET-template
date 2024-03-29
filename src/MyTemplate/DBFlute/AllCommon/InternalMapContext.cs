
using System;
using System.Collections.Generic;
using System.Threading;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

    public class InternalMapContext {

        private static LocalDataStoreSlot _internalMapSlot = Thread.AllocateDataSlot();

        protected static void Initialize() {
            if (Thread.GetData(_internalMapSlot) != null) {
                return;
            }
            Thread.SetData(_internalMapSlot, new Dictionary<String, Object>());
        }

        public static IDictionary<String, Object> GetInternalMap() {
            Initialize();
            return (IDictionary<String, Object>)Thread.GetData(_internalMapSlot);
        }

        public static Object GetObject(String key) {
            Initialize();
            IDictionary<String, Object> map = (IDictionary<String, Object>)Thread.GetData(_internalMapSlot);
            if (!map.ContainsKey(key)) {
                return null;
            }
            return map[key];
        }

        public static void SetObject(String key, Object value) {
            Initialize();
            IDictionary<String, Object> map = (IDictionary<String, Object>)Thread.GetData(_internalMapSlot);
            map.Add(key, value);
        }

        public static void ClearInternalMapOnThread() {
            Thread.SetData(_internalMapSlot, null);
        }

        public static bool IsExistInternalMapOnThread() {
            return (Thread.GetData(_internalMapSlot) != null);
        }
    }
}
