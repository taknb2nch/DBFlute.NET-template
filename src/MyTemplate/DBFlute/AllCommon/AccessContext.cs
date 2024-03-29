
using System;
using System.Threading;
using System.Collections.Generic;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

    public class AccessContext {

        private static LocalDataStoreSlot _accessContextSlot = Thread.AllocateDataSlot();

        public static AccessContext GetAccessContextOnThread() {
            return (AccessContext)Thread.GetData(_accessContextSlot);
        }

        public static void SetAccessContextOnThread(AccessContext context) {
            if (context == null) {
                String msg = "The argument[context] must not be null.";
                throw new ArgumentNullException(msg);
            }
            Thread.SetData(_accessContextSlot, context);
        }

        public static void ClearAccessContextOnThread() {
            Thread.SetData(_accessContextSlot, null);
        }

        public static bool IsExistAccessContextOnThread() {
            return (Thread.GetData(_accessContextSlot) != null);
        }

		public static DateTime? GetAccessTimestampOnThread() {
            if (IsExistAccessContextOnThread()) {
                AccessContext userContextOnThread = GetAccessContextOnThread();
                DateTime? accessTimestamp = userContextOnThread.AccessTimestamp;
                if (accessTimestamp != null) {
                    return accessTimestamp;
                }
            }
            return DateTime.Now; // as Default
		}

		public static String GetAccessUserOnThread() {
            if (IsExistAccessContextOnThread()) {
                AccessContext userContextOnThread = GetAccessContextOnThread();
                String accessUser = userContextOnThread.AccessUser;
                if (accessUser != null) {
                    return accessUser;
                }
            }
            String msg;
            if (IsExistAccessContextOnThread()) {
                msg = "The access user was not found in AccessContext on thread: " + GetAccessContextOnThread();
            } else {
                msg = "The AccessContext was not found on thread!";
            }
            throw new SystemException(msg);
		}
		
		public static String GetAccessProcessOnThread() {
            if (IsExistAccessContextOnThread()) {
                AccessContext userContextOnThread = GetAccessContextOnThread();
                String accessProcess = userContextOnThread.AccessProcess;
                if (accessProcess != null) {
                    return accessProcess;
                }
            }
            String msg;
            if (IsExistAccessContextOnThread()) {
                msg = "The access process was not found in AccessContext on thread: " + GetAccessContextOnThread();
            } else {
                msg = "The AccessContext was not found on thread!";
            }
            throw new SystemException(msg);
		}
		
		public static String GetAccessModuleOnThread() {
            if (IsExistAccessContextOnThread()) {
                AccessContext userContextOnThread = GetAccessContextOnThread();
                String accessModule = userContextOnThread.AccessModule;
                if (accessModule != null) {
                    return accessModule;
                }
            }
            String msg;
            if (IsExistAccessContextOnThread()) {
                msg = "The access module was not found in AccessContext on thread: " + GetAccessContextOnThread();
            } else {
                msg = "The AccessContext was not found on thread!";
            }
            throw new SystemException(msg);
		}

		public static Object GetAccessValueOnThread(String key) {
            if (IsExistAccessContextOnThread()) {
                AccessContext userContextOnThread = GetAccessContextOnThread();
                IDictionary<String, Object> accessValueMap = userContextOnThread.AccessValueMap;
                if (accessValueMap != null) {
                    return accessValueMap[key];
                }
            }
            String msg;
            if (IsExistAccessContextOnThread()) {
                msg = "The access value was not found in AccessContext on thread:";
                msg = msg + " key=" + key + " " + GetAccessContextOnThread();
            } else {
                msg = "The AccessContext was not found on thread: key=" + key;
            }
            throw new SystemException(msg);
		}
		
        protected DateTime? _accessTimestamp;
        protected String _accessUser;
        protected String _accessProcess;
        protected String _accessModule;

        protected IDictionary<String, Object> _accessValueMap;

		public DateTime? AccessTimestamp {
		    get { return _accessTimestamp; }
			set { _accessTimestamp = value; }
		}

		public String AccessUser {
		    get { return _accessUser; }
			set { _accessUser = value; }
		}

		public String AccessProcess {
		    get { return _accessProcess; }
			set { _accessProcess = value; }
		}

		public String AccessModule {
		    get { return _accessModule; }
			set { _accessModule = value; }
		}

		public IDictionary<String, Object> AccessValueMap {
		    get { return _accessValueMap; }
		}
        public void RegisterAccessValue(String key, Object value) {
            if (_accessValueMap == null) {
                _accessValueMap = new Dictionary<String, Object>();
            }
            _accessValueMap.Add(key, value);
        }
    }
}
