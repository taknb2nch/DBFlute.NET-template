
using System;
using System.Text;

using Seasar.Framework.Util;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.Grouping;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.Mapping;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    [System.Serializable]
    public class ListResultBean<ENTITY> : System.Collections.Generic.IList<ENTITY>, System.Collections.IList {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        #region Attribute
        protected String _tableDbName;
        protected int _allRecordCount;
        protected System.Collections.Generic.IList<ENTITY> _selectedList = new System.Collections.Generic.List<ENTITY>();
        protected OrderByClause _orderByClause = new OrderByClause();
        protected bool _isSetterInvokedSelectedList;
        #endregion

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        #region Constructor
        public ListResultBean() {
        }
        #endregion

        // ===============================================================================
        //                                                                        Grouping
        //                                                                        ========
        #region Grouping
        /**
         * Group the list.
         * @param <ROW> The type of row.
         * @param groupingRowSetupper The setupper of grouping row. (NotNull)
         * @param groupingOption The option of grouping. (NotNull and it requires the breakCount or the determiner)
         * @return The grouped list. (NotNull)
         */
        public System.Collections.Generic.IList<ROW> GroupingList<ROW>(GroupingRowSetupper<ROW, ENTITY> groupingRowSetupper, GroupingOption<ENTITY> groupingOption) {
            System.Collections.Generic.IList<ROW> groupingList = new System.Collections.Generic.List<ROW>();
            GroupingRowEndDeterminer<ENTITY> rowEndDeterminer = groupingOption.GroupingRowEndDeterminer;
            if (rowEndDeterminer == null) {
                rowEndDeterminer = delegate(GroupingRowResource<ENTITY> currentRowResource, ENTITY nextEntity) {
                    return currentRowResource.IsSizeUpBreakCount;
                }; // as Default
            }
            GroupingRowResource<ENTITY> rowResource = new GroupingRowResource<ENTITY>();
            int breakCount = groupingOption.ElementCount;
            int rowElementIndex = 0;
            int allElementIndex = 0;
            foreach (ENTITY entity in _selectedList) {
                // Set up row resource.
                rowResource.AddGroupingRowList(entity);
                rowResource.ElementCurrentIndex = rowElementIndex;
                rowResource.BreakCount = breakCount;

                if (_selectedList.Count == (allElementIndex + 1)) { // Last Loop!
                    // Set up the object of grouping row!
                    ROW groupingRowObject = groupingRowSetupper.Invoke(rowResource);

                    // Register!
                    groupingList.Add(groupingRowObject);
                    break;
                }

                // Not last loop so the nextElement must exist.
                ENTITY nextElement = _selectedList[allElementIndex + 1];

                // Do at row end.
                if (rowEndDeterminer.Invoke(rowResource, nextElement)) { // Determine the row end!
                    // Set up the object of grouping row!
                    ROW groupingRowObject = groupingRowSetupper.Invoke(rowResource);

                    // Register!
                    groupingList.Add(groupingRowObject);

                    // Initialize!
                    rowResource = new GroupingRowResource<ENTITY>();
                    rowElementIndex = 0;
                    ++allElementIndex;
                    continue;
                }
                ++rowElementIndex;
                ++allElementIndex;
            }
            return groupingList;
        }
        #endregion

        // ===============================================================================
        //                                                                         Mapping
        //                                                                         =======
        #region Mapping
        public ListResultBean<DTO> MappingList<DTO>(EntityDtoMapper<ENTITY, DTO> entityDtoMapper) {
            ListResultBean<DTO> mappingList = new ListResultBean<DTO>();
            foreach (ENTITY entity in _selectedList) {
                mappingList.Add(entityDtoMapper.Invoke(entity));
            }
            mappingList.TableDbName = TableDbName;
            mappingList.AllRecordCount = AllRecordCount;
            mappingList.OrderByClause = OrderByClause;
            return mappingList;
        }
        #endregion

        // ===============================================================================
        //                                                                   Determination
        //                                                                   =============
        #region Determination
        public bool IsSelectedResult { get { return _tableDbName != null; } } // Whether table DB name is not null
        #endregion

        // ===============================================================================
        //                                                                  Basic Override
        //                                                                  ==============
        #region Basic Override
        public override String ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("{").Append(_tableDbName);
            sb.Append(",").Append(_allRecordCount);
            sb.Append(",").Append(_orderByClause != null ? _orderByClause.getOrderByClause() : null);
            sb.Append(",").Append(_selectedList != null ? ToStringUtil.ToString(_selectedList) : null);
            sb.Append("}");
            return sb.ToString();
        }
        #endregion

        // ===============================================================================
        //                                                                   List Elements
        //                                                                   =============
        #region List Elements
        public virtual void Add(ENTITY value) {
            _selectedList.Add(value);
        }

        public virtual bool Contains(ENTITY value) {
            return _selectedList.Contains(value);
        }

        public virtual void Clear() {
            _selectedList.Clear();
        }

        public virtual int IndexOf(ENTITY value) {
            return _selectedList.IndexOf(value);
        }

        public virtual void Insert(int index, ENTITY value) {
            _selectedList.Insert(index, value);
        }

        public virtual bool Remove(ENTITY value) {
            return _selectedList.Remove(value);
        }


        public virtual void RemoveAt(int index) {
            _selectedList.RemoveAt(index);
        }

        public virtual bool IsReadOnly {
            get {
                return _selectedList.IsReadOnly;
            }
        }

        public virtual void CopyTo(ENTITY[] array, int index) {
            _selectedList.CopyTo(array, index);
        }

        public virtual int Count {
            get {
                return _selectedList.Count;
            }
        }

        public virtual System.Collections.Generic.IEnumerator<ENTITY> GetEnumerator() {
            return _selectedList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _selectedList.GetEnumerator();
        }

        public virtual ENTITY this[int index] {
            get {
                return _selectedList[index];
            }
            set {
                _selectedList[index] = value;
            }
        }
        #endregion

        // =====================================================================================
        //                                                             Non Generic List Elements
        //                                                             =========================
        #region Non Generic List Elements
        public int Add(object value) {
            _selectedList.Add((ENTITY)value);
            return 1;
        }

        public bool Contains(object value) {
            return _selectedList.Contains((ENTITY)value);
        }

        public int IndexOf(object value) {
            return _selectedList.IndexOf((ENTITY)value);
        }

        public void Insert(int index, object value) {
            _selectedList.Insert(index, (ENTITY)value);
        }

        public bool IsFixedSize {
            get { return _selectedList.IsReadOnly; }
        }

        public void Remove(object value) {
            _selectedList.Remove((ENTITY)value);
        }

        object System.Collections.IList.this[int index] {
            get {
                return _selectedList[index];
            }
            set {
                _selectedList[index] = (ENTITY)value;
            }
        }

        public void CopyTo(Array array, int index) {
            System.Collections.IList nonGenericList = new System.Collections.ArrayList();
            foreach (ENTITY entity in _selectedList) {
                nonGenericList.Add(entity);
            }
            nonGenericList.CopyTo(array, index);
        }

        public bool IsSynchronized {
            get { return false; }
        }

        public object SyncRoot {
            get { throw new NotImplementedException("SyncRoot is unsupported: " + ToString()); }
        }
        #endregion

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        #region Accessor
        public virtual String TableDbName {
            get { return _tableDbName; }
            set { _tableDbName = value; }
        }

        public virtual int AllRecordCount {
            get { return _allRecordCount; }
            set { _allRecordCount = value; }
        }

        public virtual System.Collections.Generic.IList<ENTITY> SelectedList {
            get { return _selectedList; }
            set { if (value == null) { return; } _selectedList = value; }
        }

        public virtual OrderByClause OrderByClause {
            get { return _orderByClause; }
            set { if (value == null) { return; } _orderByClause = value; }
        }
        #endregion
    }
}
