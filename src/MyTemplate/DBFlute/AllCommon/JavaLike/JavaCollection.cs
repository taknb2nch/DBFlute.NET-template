
using System;
using System.Collections.Generic;
using System.Text;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike {

    // ===================================================================================
    //                                                                          Collection
    //                                                                          ==========
    public interface Collection<ELEMENT> : System.Collections.IEnumerable {
        bool add(ELEMENT element);
        bool addAll(Collection<ELEMENT> element);
        bool remove(ELEMENT element);
        int size();
        bool isEmpty();
        void clear();
		Iterator<ELEMENT> iterator();
        ICollection<ELEMENT> getCollection();
		String toString();
    }

    public interface NgCollection {
        bool addAsNg(Object element);
        bool removeAsNg(Object element);
        System.Collections.ICollection getCollectionAsNg();
    }

    public interface Iterator<ELEMENT> {
        bool hasNext();
        ELEMENT next();
    }

    // ===================================================================================
    //                                                                                List
    //                                                                                ====
    public interface List<ELEMENT> : Collection<ELEMENT> {
        ELEMENT get(int index);
		ELEMENT set(int index, ELEMENT element);
		ELEMENT remove(int index);
        List<ELEMENT> subList(int fromIndex, int toIndex);
        IList<ELEMENT> getList();
    }

    public interface NgList : NgCollection {
        Object getAsNg(int index);
        System.Collections.IList getListAsNg();
    }

    [System.Serializable]
    public class ArrayList<ELEMENT> : List<ELEMENT>, NgList {
        IList<ELEMENT> _res = new System.Collections.Generic.List<ELEMENT>();
        public ArrayList() {
        }
        public ArrayList(Collection<ELEMENT> col) {
            foreach (ELEMENT element in col) {
                add(element);
            }
        }
        public bool add(ELEMENT element) {
            _res.Add(element);
            return true;
        }
        public bool addAll(Collection<ELEMENT> elementList) {
            bool result = false;
            foreach (ELEMENT element in elementList) {
                if (add(element)) {
                    result = true;
                }
            }
            return result;
        }
        public ELEMENT get(int index) {
            return _res[index];
        }
        public ELEMENT set(int index, ELEMENT element) {
            ELEMENT result = _res[index];
            _res[index] = element;
            return result;
        }
        public ELEMENT remove(int index) {
		    ELEMENT result = _res[index]; 
            _res.Remove(result);
			return result;
        }
        public bool remove(ELEMENT element) {
            return _res.Remove(element);
        }
        public int size() {
            return _res.Count;
        }
        public bool isEmpty() {
            return _res.Count == 0;
        }
        public void clear() {
            _res.Clear();
        }
        // merely copied list so not related to original list
        public List<ELEMENT> subList(int fromIndex, int toIndex) {
            List<ELEMENT> resultList = new ArrayList<ELEMENT>();
            for (int i=fromIndex; i < toIndex; i++) {
                resultList.add(get(i));
            }
            return resultList;
        }
        public IList<ELEMENT> getList() {
            return _res;
        }
        public ICollection<ELEMENT> getCollection() {
            return _res;
        }
        public Object getAsNg(int index) {
            return get(index);
        }
        public System.Collections.IList getListAsNg() {
            return (System.Collections.IList)getList();
        }
        public bool addAsNg(Object element) {
            return add((ELEMENT)element);
        }
        public bool removeAsNg(Object element) {
            return remove((ELEMENT)element);
        }
        public System.Collections.ICollection getCollectionAsNg() {
            return (System.Collections.ICollection)getCollection();
        }
        public Iterator<ELEMENT> iterator() {
            return new MyEmumerator(this);
        }
        public System.Collections.IEnumerator GetEnumerator() {
            return new MyEmumerator(this);
        }
        [System.Serializable]
        protected class MyEmumerator : System.Collections.IEnumerator, Iterator<ELEMENT> {
            protected IEnumerator<ELEMENT> _target;
            public MyEmumerator(ArrayList<ELEMENT> target) {
                _target = target.getCollection().GetEnumerator();
            }
            public Object Current { get { return _target.Current; } }
            public bool MoveNext() {
                return _target.MoveNext();
            }
            public void Reset() {
                _target.Reset();
            }
            public bool hasNext() {
                return MoveNext();
            }
            public ELEMENT next() { // Not moving because hasNext() does it.
                return _target.Current;
            }
        }
        public override String ToString() {
            return toString();
        }
		public String toString() {
		    StringBuilder sb = new StringBuilder();
		    sb.append("{");
		    int index = 0;
		    foreach (ELEMENT element in this) {
		        if (index > 0) { sb.append("{"); }
		        sb.append(element);
		        ++index;
		    }
		    sb.append("}");
		    return sb.toString();
		}
    }

    // ===================================================================================
    //                                                                                 Set
    //                                                                                 ===
    public interface Set<ELEMENT> : Collection<ELEMENT> {
        bool contains(ELEMENT element);
    }

    public interface NgSet : NgCollection {
        bool containsAsNg(Object element);
    }

    [System.Serializable]
    public class HashSet<ELEMENT> : Set<ELEMENT>, NgSet {
        protected IDictionary<ELEMENT, Object> _res = new Dictionary<ELEMENT, Object>();
        public bool add(ELEMENT element) {
            if (_res.ContainsKey(element)) {
                return false;
            }
            _res.Add(element, null);
            return true;
        }
        public bool addAll(Collection<ELEMENT> elementList) {
            bool result = false;
            foreach (ELEMENT element in elementList) {
                if (add(element)) {
                    result = true;
                }
            }
            return result;
        }
        public bool remove(ELEMENT element) {
            if (_res.ContainsKey(element)) {
                _res.Remove(element);
                return true;
            }
            return false;
        }
        public int size() {
            return _res.Count;
        }
        public bool isEmpty() {
            return _res.Count == 0;
        }
        public void clear() {
            _res.Clear();
        }
        public bool contains(ELEMENT element) {
            return _res.ContainsKey(element);
        }
        public ICollection<ELEMENT> getCollection() {
            return _res.Keys;
        }
        public bool containsAsNg(Object element) {
            return contains((ELEMENT)element);
        }
        public bool addAsNg(Object element) {
            return add((ELEMENT)element);
        }
        public bool removeAsNg(Object element) {
            return remove((ELEMENT)element);
        }
        public System.Collections.ICollection getCollectionAsNg() {
            return (System.Collections.ICollection)getCollection();
        }
        public Iterator<ELEMENT> iterator() {
            return new MyEmumerator(this);
        }
        public System.Collections.IEnumerator GetEnumerator() {
            return new MyEmumerator(this);
        }
        [System.Serializable]
        protected class MyEmumerator : System.Collections.IEnumerator, Iterator<ELEMENT> {
            protected IEnumerator<ELEMENT> _target;
            protected int _index;
            public MyEmumerator(HashSet<ELEMENT> target) {
                _target = target.getCollection().GetEnumerator();
            }
            public Object Current { get { return _target.Current; } }
            public bool MoveNext() {
                return _target.MoveNext();
            }
            public void Reset() {
                _target.Reset();
            }
            public bool hasNext() {
                return MoveNext();
            }
            public ELEMENT next() { // Not moving because hasNext() does it.
                return _target.Current;
            }
        }
        public override String ToString() {
            return toString();
        }
		public String toString() {
		    StringBuilder sb = new StringBuilder();
		    sb.append("{");
		    int index = 0;
		    foreach (ELEMENT element in this) {
		        if (index > 0) { sb.append("{"); }
		        sb.append(element);
		        ++index;
		    }
		    sb.append("}");
		    return sb.toString();
		}
    }

    [System.Serializable]
    public class LinkedHashSet<ELEMENT> : Set<ELEMENT>, NgSet {
        protected IDictionary<ELEMENT, Object> _res = new Dictionary<ELEMENT, Object>();
        protected List<ELEMENT> _seq = new ArrayList<ELEMENT>();
        public ELEMENT get(int index) {
            return _seq.get(index);
        }
        public bool add(ELEMENT element) {
            if (_res.ContainsKey(element)) {
                return false;
            }
            _res.Add(element, null);
            _seq.add(element);
            return true;
        }
        public bool addAll(Collection<ELEMENT> elementList) {
            bool result = false;
            foreach (ELEMENT element in elementList) {
                if (add(element)) {
                    result = true;
                }
            }
            return result;
        }
        public bool remove(ELEMENT element) {
            if (_res.ContainsKey(element)) {
                _res.Remove(element);
                _seq.remove(element);
                return true;
            }
            return false;
        }
        public int size() {
            return _res.Count;
        }
        public bool isEmpty() {
            return _res.Count == 0;
        }
        public void clear() {
            _res.Clear();
        }
        public bool contains(ELEMENT element) {
            return _res.ContainsKey(element);
        }
        public ICollection<ELEMENT> getCollection() {
            return _seq.getCollection();
        }
        public bool containsAsNg(Object element) {
            return contains((ELEMENT)element);
        }
        public bool addAsNg(Object element) {
            return add((ELEMENT)element);
        }
        public bool removeAsNg(Object element) {
            return remove((ELEMENT)element);
        }
        public System.Collections.ICollection getCollectionAsNg() {
            return (System.Collections.ICollection)getCollection();
        }
        public Iterator<ELEMENT> iterator() {
            return new MyEmumerator(this);
        }
        public System.Collections.IEnumerator GetEnumerator() {
            return new MyEmumerator(this);
        }
        [System.Serializable]
        protected class MyEmumerator : System.Collections.IEnumerator, Iterator<ELEMENT> {
            protected LinkedHashSet<ELEMENT> _target;
            protected int _index = -1;
            public MyEmumerator(LinkedHashSet<ELEMENT> target) {
                _target = target;
            }
            public Object Current { get { return _target.get(_index); } }
            public bool MoveNext() {
                ++_index;
                return _target.size() > _index;
            }
            public void Reset() {
                _index = -1;
            }
            public bool hasNext() {
                return MoveNext();
            }
            public ELEMENT next() { // Not moving because hasNext() does it.
                if (_index == -1) { MoveNext(); } // For getting first element.
                return _target.get(_index);
            }
        }
        public override String ToString() {
            return toString();
        }
		public String toString() {
		    StringBuilder sb = new StringBuilder();
		    sb.append("{");
		    int index = 0;
		    foreach (ELEMENT element in this) {
		        if (index > 0) { sb.append("{"); }
		        sb.append(element);
		        ++index;
		    }
		    sb.append("}");
		    return sb.toString();
		}
    }

    // ===================================================================================
    //                                                                                 Map
    //                                                                                 ===
    public interface Map<KEY, VALUE> {
		VALUE get(KEY key);
        VALUE put(KEY key, VALUE value);
        VALUE remove(KEY obj);
		int size();
		bool isEmpty();
		void clear();
        bool containsKey(KEY key);
		Set<KEY> keySet();
		Collection<VALUE> values();
		Set<Entry<KEY, VALUE>> entrySet();
    }

    public interface NgMap {
        Object getAsNg(Object key);
        Object putAsNg(Object key, Object value);
        Object removeAsNg(Object key);
        bool containsKeyAsNg(Object key);
    }

    [System.Serializable]
    public class HashMap<KEY, VALUE> : Map<KEY, VALUE>, NgMap {
		protected IDictionary<KEY, VALUE> _res = new Dictionary<KEY, VALUE>();
        public VALUE get(KEY key) {
            return _res.ContainsKey(key) ? _res[key] : default(VALUE);
        }
        public VALUE put(KEY key, VALUE value) {
            VALUE result = default(VALUE);
		    if (_res.ContainsKey(key)) {
                result = _res[key];
			    _res.Remove(key);
			}
			_res.Add(key, value);
            return result;
		}
        public VALUE remove(KEY key) {
            VALUE result = default(VALUE);
            if (_res.ContainsKey(key)) {
                result = _res[key];
                _res.Remove(key);
            }
            return result;
        }
		public int size() {
            return _res.Count;
        }
        public bool isEmpty() {
            return _res.Count == 0;
        }
        public void clear() {
            _res.Clear();
        }
        public bool containsKey(KEY key) {
            return _res.ContainsKey(key);
        }
        public Set<KEY> keySet() {
            Set<KEY> keySet = new LinkedHashSet<KEY>();
            ICollection<KEY> keyCol = _res.Keys;
            foreach (KEY key in keyCol) {
                keySet.add(key);
            }
            return keySet;
        }
        public Collection<VALUE> values() {
            List<VALUE> valueList = new ArrayList<VALUE>();
            ICollection<VALUE> keyCol = _res.Values;
            foreach (VALUE value in keyCol) {
                valueList.add(value);
            }
            return valueList;
        }
        public Set<Entry<KEY, VALUE>> entrySet() {
            Set<Entry<KEY, VALUE>> entrySet = new LinkedHashSet<Entry<KEY, VALUE>>();
            ICollection<KEY> keyCol = _res.Keys;
            foreach (KEY key in keyCol) {
                entrySet.add(new Entry<KEY, VALUE>(key, _res[key]));
            }
            return entrySet;
        }
        public Object getAsNg(Object key) {
            return get((KEY)key);
        }
        public Object putAsNg(Object key, Object value) {
            return put((KEY)key, (VALUE)value);
        }
        public Object removeAsNg(Object key) {
            return remove((KEY)key);
        }
        public bool containsKeyAsNg(Object key) {
            return containsKey((KEY)key);
        }
		public override String ToString() {
		    return ToString();
		}
		public String toString() {
		    StringBuilder sb = new StringBuilder();
		    sb.append("{");
		    int index = 0;
		    Set<Entry<KEY, VALUE>> entrySet = this.entrySet();
		    foreach (Entry<KEY, VALUE> entry in entrySet) {
		        if (index > 0) { sb.append("{"); }
		        sb.append(entry.toString());
		        ++index;
		    }
		    sb.append("}");
		    return sb.toString();
		}
    }

    [System.Serializable]
    public class LinkedHashMap<KEY, VALUE> : Map<KEY, VALUE>, NgMap {
        protected Map<KEY, VALUE> _res = new HashMap<KEY, VALUE>();
        protected LinkedHashSet<KEY> _seq = new LinkedHashSet<KEY>();
        public VALUE get(KEY key) {
            return _res.containsKey(key) ? _res.get(key) : default(VALUE);
        }
        public VALUE get(int index) {
            return _res.get(_seq.get(index));
        }
        public VALUE put(KEY key, VALUE value) {
            VALUE result = default(VALUE);
            if (_res.containsKey(key)) {
                result = _res.get(key);
                _res.remove(key);
            } else {
                _seq.add(key);
            }
            _res.put(key, value);
            return result;
        }
        public VALUE remove(KEY key) {
            VALUE result = default(VALUE);
            if (_res.containsKey(key)) {
                result = _res.get(key);
                _res.remove(key);
                _seq.remove(key);
            }
            return result;
        }

        public int size() {
            return _res.size();
        }
        public bool isEmpty() {
            return _res.isEmpty();
        }
        public void clear() {
            _res.clear();
        }
        public bool containsKey(KEY obj) {
            return _res.containsKey(obj);
        }
        public Set<KEY> keySet() {
            return _seq;
        }
        public Collection<VALUE> values() {
            List<VALUE> valueList = new ArrayList<VALUE>();
            foreach (KEY key in _seq.getCollection()) {
                valueList.add(_res.get(key));
            }
            return valueList;
        }
        public Set<Entry<KEY, VALUE>> entrySet() {
            Set<Entry<KEY, VALUE>> entrySet = new LinkedHashSet<Entry<KEY, VALUE>>();
            Set<KEY> keyCol = _res.keySet();
            foreach (KEY key in keyCol) {
                entrySet.add(new Entry<KEY, VALUE>(key, _res.get(key)));
            }
            return entrySet;
        }
        public Object getAsNg(Object key) {
            return get((KEY)key);
        }
        public Object putAsNg(Object key, Object value) {
            return put((KEY)key, (VALUE)value);
        }
        public Object removeAsNg(Object key) {
            return remove((KEY)key);
        }
        public bool containsKeyAsNg(Object key) {
            return containsKey((KEY)key);
        }
		public override String ToString() {
		    return ToString();
		}
		public String toString() {
		    StringBuilder sb = new StringBuilder();
		    sb.append("{");
		    int index = 0;
		    Set<Entry<KEY, VALUE>> entrySet = this.entrySet();
		    foreach (Entry<KEY, VALUE> entry in entrySet) {
		        if (index > 0) { sb.append("{"); }
		        sb.append(entry.toString());
		        ++index;
		    }
		    sb.append("}");
		    return sb.toString();
		}
    }

    [System.Serializable]
    public class Entry<KEY, VALUE> {
        protected KEY _key;
        protected VALUE _value;
        public override String ToString() {
            return toString();
        }
        public String toString() {
            return _key + "=" + _value;
        }
        public Entry(KEY key, VALUE value) {
            _key = key;
            _value = value;
        }
        public KEY getKey() {
            return _key;
        }
        public VALUE getValue() {
            return _value;
        }
    }
}
