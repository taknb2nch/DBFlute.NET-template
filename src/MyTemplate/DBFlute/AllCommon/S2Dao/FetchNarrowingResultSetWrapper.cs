
using System;
using System.Collections;
using System.Reflection;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao {

    public class FetchNarrowingResultSetWrapper : System.Data.IDataReader {

        // ===============================================================================
        //                                                                       Attribute
        //                                                                       =========
        protected System.Data.IDataReader _dataReader;
        protected FetchNarrowingBean _fetchNarrowingBean;
        protected long _fetchCounter;
        protected long _requestCounter;
        protected bool _offsetByCursorForcedly;
        protected bool _limitByCursorForcedly;

        // ===============================================================================
        //                                                                     Constructor
        //                                                                     ===========
        public FetchNarrowingResultSetWrapper(System.Data.IDataReader dataReader, FetchNarrowingBean fetchNarrowingBean
                                                 , bool offsetByCursorForcedly, bool limitByCursorForcedly) {
            _dataReader = dataReader;
            _fetchNarrowingBean = fetchNarrowingBean;
            _offsetByCursorForcedly = offsetByCursorForcedly;
            _limitByCursorForcedly = limitByCursorForcedly;

            skip();
        }

        // ===============================================================================
        //                                                                            Skip
        //                                                                            ====
        private void skip() {
            if (!IsAvailableSkipRecord()) {
                return;
            }
            int skipStartIndex = GetFetchNarrowingSkipStartIndex();
            if (IsCursorUsed()) {
                //if (0 == skipStartIndex) {
                //    _dataReader.beforeFirst();
                //} else {
                //    _dataReader.absolute(skipStartIndex);
                //}
                //_fetchCounter = _dataReader.getRow();
                throw new NotSupportedException("Cursor is unsupported!!!");
            } else {
                while (_fetchCounter < skipStartIndex && _dataReader.Read()) {
                    ++_fetchCounter;
                }
            }
        }

        protected bool IsAvailableSkipRecord() {
            if (!IsFetchNarrowingEffective()) {
                return false;
            }
            if (IsOffsetByCursorForcedly) {
                return true;
            }
            if (IsFetchNarrowingSkipStartIndexEffective()) {
                return true;
            }
            return false;
        }

        // ===============================================================================
        //                                                                            Read
        //                                                                            ====
        public bool Read() {
            bool hasNext = _dataReader.Read();
            ++_requestCounter;
            if (!IsAvailableLimitLoopCount()) {
                CheckSafetyResult(hasNext);
                return hasNext;
            }

            if (hasNext && _fetchCounter < GetFetchNarrowingSkipStartIndex() + GetFetchNarrowingLoopCount()) {
                ++_fetchCounter;
                CheckSafetyResult(true);
                return true;
            } else {
                return false;
            }
        }

        protected bool IsAvailableLimitLoopCount() {
            if (!IsFetchNarrowingEffective()) {
                return false;
            }
            if (IsLimitByCursorForcedly) {
                return true;
            }
            if (IsFetchNarrowingLoopCountEffective()) {
                return true;
            }
            return false;
        }

        protected void CheckSafetyResult(bool hasNext) {
            int safetyMaxResultSize = GetSafetyMaxResultSize();
            if (hasNext && safetyMaxResultSize > 0 && _requestCounter > safetyMaxResultSize) {
                throwDangerousResultSizeException(safetyMaxResultSize);
            }
        }

        protected void throwDangerousResultSizeException(int safetyMaxResultSize) {
            String msg = "Look! Read the message below." + ln();
            msg = msg + "/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *" + ln();
            msg = msg + "You've already been in DANGER ZONE. (check is working)" + ln();
            msg = msg + ln();
            msg = msg + "[Safety Max Result Size]" + ln();
            msg = msg + safetyMaxResultSize + ln();
            msg = msg + ln();
            if (_fetchNarrowingBean is ConditionBean) {
                msg = msg + "[Display SQL]" + ln();
                msg = msg + ((ConditionBean) _fetchNarrowingBean).ToDisplaySql() + ln();
            } else {
                msg = msg + "[Fetch Bean]" + ln();
                msg = msg + _fetchNarrowingBean + ln();
            }
            msg = msg + "* * * * * * * * * */";
            throw new DangerousResultSizeException(msg, safetyMaxResultSize);
        }

        protected virtual String ln() {
            return Environment.NewLine;
        }

        // ===============================================================================
        //                                                            Fetch Narrowing Bean
        //                                                            ====================
        protected bool IsFetchNarrowingEffective() {
            return _fetchNarrowingBean.IsFetchNarrowingEffective;
        }

        protected bool IsFetchNarrowingSkipStartIndexEffective() {
            return _fetchNarrowingBean.IsFetchNarrowingSkipStartIndexEffective;
        }

        protected bool IsFetchNarrowingLoopCountEffective() {
            return _fetchNarrowingBean.IsFetchNarrowingLoopCountEffective;
        }

        protected int GetFetchNarrowingSkipStartIndex() {
            return _fetchNarrowingBean.FetchNarrowingSkipStartIndex;
        }

        protected int GetFetchNarrowingLoopCount() {
            return _fetchNarrowingBean.FetchNarrowingLoopCount;
        }

        protected int GetSafetyMaxResultSize() {
            return _fetchNarrowingBean.SafetyMaxResultSize;
        }

        // ===============================================================================
        //                                                                   Assist Helper
        //                                                                   =============
        protected bool IsCursorUsed() {
            return IsCursorSupported(_dataReader);
        }

        public static bool IsCursorSupported(System.Data.IDataReader dataReader) {
            return false; // Is Cursor Unsupported at C#?
        }

        // ===============================================================================
        //                                                                        Accessor
        //                                                                        ========
        public FetchNarrowingBean FetchNarrowingBean {
            get { return _fetchNarrowingBean; }
        }

        public bool IsOffsetByCursorForcedly {
            get { return _offsetByCursorForcedly; }
        }

        public bool IsLimitByCursorForcedly {
            get { return _limitByCursorForcedly; }
        }

        // ===============================================================================
        //                                                   Implementation of IDataReader
        //                                                   =============================
        #region Implementation of IDataReader
        void System.Data.IDataReader.Close() {
            _dataReader.Close();
        }

        int System.Data.IDataReader.Depth {
            get { return _dataReader.Depth; }
        }

        System.Data.DataTable System.Data.IDataReader.GetSchemaTable() {
            return _dataReader.GetSchemaTable();
        }

        bool System.Data.IDataReader.IsClosed {
            get { return _dataReader.IsClosed; }
        }

        bool System.Data.IDataReader.NextResult() {
            return _dataReader.NextResult();
        }

        int System.Data.IDataReader.RecordsAffected {
            get { return _dataReader.RecordsAffected; }
        }
        #endregion

        // ===============================================================================
        //                                                   Implementation of IDisposable
        //                                                   =============================
        #region Implementation of IDisposable
        void System.IDisposable.Dispose() {
            _dataReader.Dispose();
        }
        #endregion

        // ===============================================================================
        //                                                   Implementation of IDataRecord
        //                                                   =============================
        #region Implementation of IDataRecord
        int System.Data.IDataRecord.FieldCount {
            get { return _dataReader.FieldCount; }
        }

        bool System.Data.IDataRecord.GetBoolean(int i) {
            return _dataReader.GetBoolean(i);
        }

        byte System.Data.IDataRecord.GetByte(int i) {
            return _dataReader.GetByte(i);
        }

        long System.Data.IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            return _dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        char System.Data.IDataRecord.GetChar(int i) {
            return _dataReader.GetChar(i);
        }

        long System.Data.IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            return _dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        System.Data.IDataReader System.Data.IDataRecord.GetData(int i) {
            return _dataReader.GetData(i);
        }

        string System.Data.IDataRecord.GetDataTypeName(int i) {
            return _dataReader.GetDataTypeName(i);
        }

        System.DateTime System.Data.IDataRecord.GetDateTime(int i) {
            return _dataReader.GetDateTime(i);
        }

        decimal System.Data.IDataRecord.GetDecimal(int i) {
            return _dataReader.GetDecimal(i);
        }

        double System.Data.IDataRecord.GetDouble(int i) {
            return _dataReader.GetDouble(i);
        }

        System.Type System.Data.IDataRecord.GetFieldType(int i) {
            return _dataReader.GetFieldType(i);
        }

        float System.Data.IDataRecord.GetFloat(int i) {
            return _dataReader.GetFloat(i);
        }

        System.Guid System.Data.IDataRecord.GetGuid(int i) {
            return _dataReader.GetGuid(i);
        }

        short System.Data.IDataRecord.GetInt16(int i) {
            return _dataReader.GetInt16(i);
        }

        int System.Data.IDataRecord.GetInt32(int i) {
            return _dataReader.GetInt32(i);
        }

        long System.Data.IDataRecord.GetInt64(int i) {
            return _dataReader.GetInt64(i);
        }

        string System.Data.IDataRecord.GetName(int i) {
            return _dataReader.GetName(i);
        }

        int System.Data.IDataRecord.GetOrdinal(string name) {
            return _dataReader.GetOrdinal(name);
        }

        string System.Data.IDataRecord.GetString(int i) {
            return _dataReader.GetString(i);
        }

        object System.Data.IDataRecord.GetValue(int i) {
            return _dataReader.GetValue(i);
        }

        int System.Data.IDataRecord.GetValues(object[] values) {
            return _dataReader.GetValues(values);
        }

        bool System.Data.IDataRecord.IsDBNull(int i) {
            return _dataReader.IsDBNull(i);
        }

        object System.Data.IDataRecord.this[string name] {
            get { return _dataReader[name]; }
        }

        object System.Data.IDataRecord.this[int i] {
            get { return _dataReader[i]; }
        }
        #endregion
    }
}
