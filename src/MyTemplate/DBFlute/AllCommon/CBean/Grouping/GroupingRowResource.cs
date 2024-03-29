
using System;
using System.Collections.Generic;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.Grouping {

public class GroupingRowResource<ENTITY> {

    // =====================================================================================
    //                                                                             Attribute
    //                                                                             =========
    protected IList<ENTITY> _groupingRowList = new List<ENTITY>();
    protected int _elementCurrentIndex;
    protected int _breakCount;

    // =====================================================================================
    //                                                                           Easy-to-Use
    //                                                                           ===========
    /**
     * @return Does the list of grouping row size up the break count?
     */
    public bool IsSizeUpBreakCount { get {
        return _elementCurrentIndex == (_breakCount-1);
    }}

    // =====================================================================================
    //                                                                              Accessor
    //                                                                              ========
    /**
     * @return The list of grouping row. (NotNull and NotEmpty)
     */
    public IList<ENTITY> GroupingRowList { get {
        return this._groupingRowList;
    }}

    /**
     * Add the element entity to the list of grouping row. {INTERNAL METHOD}
     * @param groupingRow The element entity to the list of grouping row.
     */
    public void AddGroupingRowList(ENTITY groupingRow) {
        this._groupingRowList.Add(groupingRow);
    }

    /**
     * @return The entity of element current index. (NotNull)
     */
    public ENTITY CurrentEntity { get {
        return _groupingRowList[_elementCurrentIndex];
    }}

    public int ElementCurrentIndex { get {
        return this._elementCurrentIndex;
    } set {
        this._elementCurrentIndex = value;
    }}

    public int BreakCount { get {
        return this._breakCount;
    } set {
        this._breakCount = value;
    }}
}

}
