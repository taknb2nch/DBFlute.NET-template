
using System;
using System.Collections.Generic;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon {

    /// <summary>
    /// The interface of entity.
    /// Author: DBFlute(AutoGenerator)
    /// </summary>
    public interface Entity {

        // ===============================================================================
        //                                                                      Table Name
        //                                                                      ==========
        /// <summary>
        /// The property of table db-name. (readonly)
        /// </summary>
        String TableDbName { get; }

        /// <summary>
        /// The property of table property-name. (readonly)
        /// </summary>
        String TablePropertyName { get; }

        // ===============================================================================
        //                                                                          DBMeta
        //                                                                          ======
        /// <summary>
        /// The property of DBMeta. (readonly)
        /// </summary>
        DBMeta DBMeta { get; }

        // ===============================================================================
        //                                                                   Determination
        //                                                                   =============
        /// <summary>
        /// Has primary-key value? (readonly)
        /// </summary>
        bool HasPrimaryKeyValue { get; }

        // ===============================================================================
        //                                                             Modified Properties
        //                                                             ===================
        IDictionary<String, Object> ModifiedPropertyNames { get; }
        void ClearModifiedPropertyNames();

        // ===============================================================================
        //                                                                  Display String
        //                                                                  ==============
        String ToStringWithRelation();
        String BuildDisplayString(String name, bool column, bool relation);
    }

    /// <summary>
    /// The modified properties of entity.
    /// Author: DBFlute(AutoGenerator)
    /// </summary>
    [System.Serializable]
    public class EntityModifiedProperties {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected IDictionary<String, Object> _propertiesSet = new Dictionary<String, Object>();

        // ===============================================================================
        //                                                                             Add
        //                                                                             ===
        public void AddPropertyName(String propertyName) {
            if (_propertiesSet.ContainsKey(propertyName)) {
                return;
            }
            _propertiesSet.Add(propertyName, null);
        }

        // ===============================================================================
        //                                                                           Other
        //                                                                           =====
        public void Clear() {
            _propertiesSet.Clear();
        }

        public void Remove(String propertyName) {
            _propertiesSet.Remove(propertyName);
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public IDictionary<String, Object> PropertyNames {
            get { return _propertiesSet; }
        }

    }
}
