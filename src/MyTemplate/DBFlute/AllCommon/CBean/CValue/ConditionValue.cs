
using System;
using System.Text;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.COption;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.CValue {

[System.Serializable]
public class ConditionValue {

    // ==================================================================================
    //                                                                              Equal
    //                                                                              =====
    protected Object _equalValue;

    public Object Equal {
        get { return _equalValue; }
        set { _equalValue = value; }
    }

    public bool HasEqual { get {
        return _equalValue != null;
    }}

    public bool EqualEqual(Object value) {
        return HasEqual ? _equalValue.Equals(value) : value == null;
    }

    public ConditionValue OverrideEqual(Object value) {
        _equalValue = value; return this;
    }

    protected String _equalLocation;

    public String getEqualLocation() {
        return _equalLocation;
    }

    public ConditionValue setEqualLocation(String location) {
        _equalLocation = location;
        return this;
    }

    // ==================================================================================
    //                                                                          Not Equal
    //                                                                          =========
    protected Object _notEqualValue;

    public Object NotEqual {
        get { return _notEqualValue; }
        set { _notEqualValue = value; }
    }

    public bool HasNotEqual { get {
        return _notEqualValue != null;
    }}

    public bool EqualNotEqual(Object value) {
        return HasNotEqual ? _notEqualValue.Equals(value) : value == null;
    }

    public ConditionValue OverrideNotEqual(Object value) {
        _notEqualValue = value; return this;
    }

    protected String _notEqualLocation;

    public String getNotEqualLocation() {
        return _notEqualLocation;
    }

    public ConditionValue setNotEqualLocation(String location) {
        _notEqualLocation = location;
        return this;
    }

    // ==================================================================================
    //                                                                        GreaterThan
    //                                                                        ===========
    protected Object _greaterThanValue;

    public Object GreaterThan {
        get { return _greaterThanValue; }
        set { _greaterThanValue = value; }
    }

    public bool HasGreaterThan { get {
        return _greaterThanValue != null;
    }}

    public bool EqualGreaterThan(Object value) {
        return HasGreaterThan ? _greaterThanValue.Equals(value) : value == null;
    }

    public ConditionValue OverrideGreaterThan(Object value) {
        _greaterThanValue = value; return this;
    }

    protected String _greaterThanLocation;

    public String getGreaterThanLocation() {
        return _greaterThanLocation;
    }

    public ConditionValue setGreaterThanLocation(String location) {
        _greaterThanLocation = location;
        return this;
    }

    // ==================================================================================
    //                                                                          Less Than
    //                                                                          =========
    protected Object _lessThanValue;

    public Object LessThan {
        get { return _lessThanValue; }
        set { _lessThanValue = value; }
    }

    public bool HasLessThan { get {
        return _lessThanValue != null;
    }}

    public bool EqualLessThan(Object value) {
        return HasLessThan ? _lessThanValue.Equals(value) : value == null;
    }

    public ConditionValue OverrideLessThan(Object value) {
        _lessThanValue = value; return this;
    }

    protected String _lessThanLocation;

    public String getLessThanLocation() {
        return _lessThanLocation;
    }

    public ConditionValue setLessThanLocation(String location) {
        _lessThanLocation = location;
        return this;
    }

    // ==================================================================================
    //                                                                      Greater Equal
    //                                                                      =============
    protected Object _greaterEqualValue;

    public Object GreaterEqual {
        get { return _greaterEqualValue; }
        set { _greaterEqualValue = value; }
    }

    public bool HasGreaterEqual { get {
        return _greaterEqualValue != null;
    }}

    public bool EqualGreaterEqual(Object value) {
        return HasGreaterEqual ? _greaterEqualValue.Equals(value) : value == null;
    }

    public ConditionValue OverrideGreaterEqual(Object value) {
        _greaterEqualValue = value; return this;
    }

    protected String _greaterEqualLocation;

    public String getGreaterEqualLocation() {
        return _greaterEqualLocation;
    }

    public ConditionValue setGreaterEqualLocation(String location) {
        _greaterEqualLocation = location;
        return this;
    }

    // ==================================================================================
    //                                                                         Less Equal
    //                                                                         ==========
    protected Object _lessEqualValue;

    public Object LessEqual {
        get { return _lessEqualValue; }
        set { _lessEqualValue = value; }
    }

    public bool HasLessEqual { get {
        return _lessEqualValue != null;
    }}

    public bool EqualLessEqual(Object value) {
        return HasLessEqual ? _lessEqualValue.Equals(value) : value == null;
    }

    public ConditionValue OverrideLessEqual(Object value) {
        _lessEqualValue = value; return this;
    }

    protected String _lessEqualLocation;

    public String getLessEqualLocation() {
        return _lessEqualLocation;
    }

    public ConditionValue setLessEqualLocation(String location) {
        _lessEqualLocation = location;
        return this;
    }

    // ==================================================================================
    //                                                                      Prefix Search
    //                                                                      =============
    protected Object _prefixSearch;

    public Object PrefixSearch {
        get { return _prefixSearch; }
        set { _prefixSearch = value; }
    }

    public bool HasPrefixSearch { get {
        return _prefixSearch != null;
    }}

    public bool EqualPrefixSearch(Object value) {
        return HasPrefixSearch ? _prefixSearch.Equals(value) : value == null;
    }

    public ConditionValue OverridePrefixSearch(Object value) {
        _prefixSearch = value; return this;
    }

    protected String _prefixSearchLocation;

    public String getPrefixSearchLocation() {
        return _prefixSearchLocation;
    }

    public ConditionValue setPrefixSearchLocation(String location) {
        _prefixSearchLocation = location;
        return this;
    }

    // ===================================================================================
    //                                                                            In Scope
    //                                                                            ========
    protected System.Collections.IList _inScope;
    protected System.Collections.IList _inScope4Spare;

    public System.Collections.IList InScope { get {
        if (_inScope == null) {
            return null;
        }
        if (_inScope.Count == 0 && _inScope4Spare.Count > 0) {
            for (int index = 0; index < _inScope4Spare.Count; index++) {
                _inScope.Add(_inScope4Spare[index]);
            }
        }
        System.Collections.IList inScopeValue = (System.Collections.IList)_inScope[0];
        _inScope.RemoveAt(0);
        return inScopeValue;
    } set {
        if (_inScope == null) {
            _inScope = new System.Collections.ArrayList();
            _inScope4Spare = new System.Collections.ArrayList();
        }
        if (_inScope.Count == 0 && _inScope4Spare.Count > 0) {
            for (int index = 0; index < _inScope4Spare.Count; index++) {
                _inScope.Add(_inScope4Spare[index]);
            }
        }
        _inScope.Add(value);
        _inScope4Spare.Add(value);
    }}

    protected String _inScopeLocation;

    public String getInScopeLocation() {
        return _inScopeLocation;
    }

    public ConditionValue setInScopeLocation(String location) {
        _inScopeLocation = location;
        return this;
    }

    // ===================================================================================
    //                                                                        Not In Scope
    //                                                                        ============
    protected System.Collections.IList _notInScope;
    protected System.Collections.IList _notInScope4Spare;

    public System.Collections.IList NotInScope { get {
        if (_notInScope == null) {
            return null;
        }
        if (_notInScope.Count == 0 && _notInScope4Spare.Count > 0) {
            for (int index = 0; index < _notInScope4Spare.Count; index++) {
                _notInScope.Add(_notInScope4Spare[index]);
            }
        }
        System.Collections.IList notInScopeValue = (System.Collections.IList)_notInScope[0];
        _notInScope.RemoveAt(0);
        return notInScopeValue;
    } set {
        if (_notInScope == null) {
            _notInScope = new System.Collections.ArrayList();
            _notInScope4Spare = new System.Collections.ArrayList();
        }
        if (_notInScope.Count == 0 && _notInScope4Spare.Count > 0) {
            for (int index = 0; index < _notInScope4Spare.Count; index++) {
                _notInScope.Add(_notInScope4Spare[index]);
            }
        }
        _notInScope.Add(value);
        _notInScope4Spare.Add(value);
    }}

    protected String _notInScopeLocation;

    public String getNotInScopeLocation() {
        return _notInScopeLocation;
    }

    public ConditionValue setNotInScopeLocation(String location) {
        _notInScopeLocation = location;
        return this;
    }

    // ===================================================================================
    //                                                                         Like Search
    //                                                                         ===========
    protected List<LikeSearchValue> _likeSearch;
    protected List<LikeSearchValue> _likeSearch4Spare;

    public String LikeSearch { get {
        if (_likeSearch == null) {
            return null;
        }
        if (_likeSearch.isEmpty() && !_likeSearch4Spare.isEmpty()) {
            for (int index=0; index < _likeSearch4Spare.size(); index++) {
                _likeSearch.add(_likeSearch4Spare.get(index));
            }
        }
        LikeSearchValue likeSearchValue = (LikeSearchValue)_likeSearch.remove(0);
        return likeSearchValue.generateRealValue();
    }}

    public ConditionValue setLikeSearch(String value, LikeSearchOption option) {
        if (_likeSearch == null) {
            _likeSearch = new ArrayList<LikeSearchValue>();
            _likeSearch4Spare= new ArrayList<LikeSearchValue>();
        }
        if (_likeSearch.isEmpty() && !_likeSearch4Spare.isEmpty()) {
            for (int index=0; index < _likeSearch4Spare.size(); index++) {
                _likeSearch.add(_likeSearch4Spare.get(index));
            }
        }
        LikeSearchValue likeSearchValue = new LikeSearchValue(value, option);
        _likeSearch.add(likeSearchValue);
        _likeSearch4Spare.add(likeSearchValue);
        return this;
    }

    protected String _likeSearchLocation;

    public String getLikeSearchLocation() {
        return _likeSearchLocation;
    }

    public ConditionValue setLikeSearchLocation(String location) {
        _likeSearchLocation = location;
        return this;
    }

    protected class LikeSearchValue {
        protected String _value;
        protected LikeSearchOption _option;
        public LikeSearchValue(String value, LikeSearchOption option) {
            _value = value;
            _option = option;
        }
        public String getValue() {
            return _value;
        }
        public LikeSearchOption getOption() {
            return _option;
        }
        public String generateRealValue() {
            if (_option == null) {
                return _value;
            }
            return _option.generateRealValue(_value);
        }
    }

    // ===================================================================================
    //                                                                     Not Like Search
    //                                                                     ===============
    protected List<NotLikeSearchValue> _notLikeSearch;
    protected List<NotLikeSearchValue> _notLikeSearch4Spare;

    public String NotLikeSearch { get {
        if (_notLikeSearch == null) {
            return null;
        }
        if (_notLikeSearch.isEmpty() && !_notLikeSearch4Spare.isEmpty()) {
            for (int index=0; index < _notLikeSearch4Spare.size(); index++) {
                _notLikeSearch.add(_notLikeSearch4Spare.get(index));
            }
        }
        NotLikeSearchValue notLikeSearchValue = (NotLikeSearchValue)_notLikeSearch.remove(0);
        return notLikeSearchValue.generateRealValue();
    }}

    public ConditionValue setNotLikeSearch(String value, LikeSearchOption option) {
        if (_notLikeSearch == null) {
            _notLikeSearch = new ArrayList<NotLikeSearchValue>();
            _notLikeSearch4Spare= new ArrayList<NotLikeSearchValue>();
        }
        if (_notLikeSearch.isEmpty() && !_notLikeSearch4Spare.isEmpty()) {
            for (int index=0; index < _notLikeSearch4Spare.size(); index++) {
                _notLikeSearch.add(_notLikeSearch4Spare.get(index));
            }
        }
        NotLikeSearchValue notLikeSearchValue = new NotLikeSearchValue(value, option);
        _notLikeSearch.add(notLikeSearchValue);
        _notLikeSearch4Spare.add(notLikeSearchValue);
        return this;
    }

    protected String _notLikeSearchLocation;

    public String getNotLikeSearchLocation() {
        return _notLikeSearchLocation;
    }

    public ConditionValue setNotLikeSearchLocation(String location) {
        _notLikeSearchLocation = location;
        return this;
    }

    protected class NotLikeSearchValue {
        protected String _value;
        protected LikeSearchOption _option;
        public NotLikeSearchValue(String value, LikeSearchOption option) {
            _value = value;
            _option = option;
        }
        public String getValue() {
            return _value;
        }
        public LikeSearchOption getOption() {
            return _option;
        }
        public String generateRealValue() {
            if (_option == null) {
                return _value;
            }
            return _option.generateRealValue(_value);
        }
    }

    // ===================================================================================
    //                                                                             Is Null
    //                                                                             =======
    protected Object _isNullValue;

    public Object IsNull {
        get { return _isNullValue; }
        set { _isNullValue = value; }
    }

    public bool HasIsNull { get {
        return _isNullValue != null;
    }}

    protected String _isNullLocation;

    public String getIsNullLocation() {
        return _isNullLocation;
    }

    public ConditionValue setIsNullLocation(String location) {
        _isNullLocation = location;
        return this;
    }

    // ===================================================================================
    //                                                                         Is Not Null
    //                                                                         ===========
    protected Object _isNotNullValue;

    public Object IsNotNull {
        get { return _isNotNullValue; }
        set { _isNotNullValue = value; }
    }

    public bool HasIsNotNull { get {
        return _isNotNullValue != null;
    }}

    protected String _isNotNullLocation;

    public String getIsNotNullLocation() {
        return _isNotNullLocation;
    }

    public ConditionValue setIsNotNullLocation(String location) {
        _isNotNullLocation = location;
        return this;
    }
}
	
}
