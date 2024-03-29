
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.PageNavi;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.PageNavi.Group {

[System.Serializable]
public class PageGroupBean {

    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
    protected int _currentPageNumber;
    protected int _allPageCount;
    protected PageGroupOption _pageGroupOption;
    protected System.Collections.Generic.IList<int> _cachedPageNumberList;

    // ===================================================================================
    //                                                                    Page Number List
    //                                                                    ================
    public System.Collections.Generic.IList<LINK> BuildPageNumberLinkList<LINK>(PageNumberLinkSetupper<LINK> pageNumberLinkSetupper) where LINK : PageNumberLink {
        System.Collections.Generic.IList<int> pageNumberList = CreatePageNumberList();
        System.Collections.Generic.IList<LINK> pageNumberLinkList = new System.Collections.Generic.List<LINK>();
        foreach (int pageNumber in pageNumberList) {
            pageNumberLinkList.Add(pageNumberLinkSetupper.Invoke(pageNumber, pageNumber == CurrentPageNumber));
        }
        return pageNumberLinkList;
    }

    public System.Collections.Generic.IList<int> CreatePageNumberList() {
        AssertPageGroupValid();
        if (_cachedPageNumberList != null) {
            return _cachedPageNumberList;
        }
        int pageGroupSize = PageGroupOption.PageGroupSize;
        int allPageCount = AllPageCount;
        int currentPageGroupStartPageNumber = CalculateStartPageNumber();
        if (!(currentPageGroupStartPageNumber > 0)) {
            String msg = "currentPageGroupStartPageNumber should be greater than 0. {> 0} But:";
            msg = msg + " currentPageGroupStartPageNumber=" + currentPageGroupStartPageNumber;
            throw new IllegalStateException(msg);
        }
        int nextPageGroupStartPageNumber = currentPageGroupStartPageNumber + pageGroupSize;

        System.Collections.Generic.IList<int> resultList = new System.Collections.Generic.List<int>();
        for (int i=currentPageGroupStartPageNumber; i < nextPageGroupStartPageNumber && i <= allPageCount; i++) {
            resultList.Add(i);
        }
        _cachedPageNumberList = resultList;
        return _cachedPageNumberList;
    }

    protected int CalculateStartPageNumber() {
        AssertPageGroupValid();
        int pageGroupSize = PageGroupOption.PageGroupSize;
        int currentPageNumber = CurrentPageNumber;

        int currentPageGroupNumber = (currentPageNumber / pageGroupSize);
        if ((currentPageNumber % pageGroupSize) == 0)
        {
            currentPageGroupNumber--;
        }
        int currentPageGroupStartPageNumber = (pageGroupSize * currentPageGroupNumber) + 1;
        if (!(currentPageNumber >= currentPageGroupStartPageNumber)) {
            String msg = "currentPageNumber should be greater equal currentPageGroupStartPageNumber. But:";
            msg = msg + " currentPageNumber=" + currentPageNumber;
            msg = msg + " currentPageGroupStartPageNumber=" + currentPageGroupStartPageNumber;
            throw new IllegalStateException(msg);
        }
        return currentPageGroupStartPageNumber;
    }

    public int[] CreatePageNumberArray() {
        AssertPageGroupValid();
        return ConvertListToIntArray(CreatePageNumberList());
    }

    // ===================================================================================
    //                                                                       Determination
    //                                                                       =============
    public bool IsExistPrePageGroup() {
        AssertPageGroupValid();
        return (CurrentPageNumber > PageGroupOption.PageGroupSize);
    }

    public bool IsExistNextPageGroup() {
        AssertPageGroupValid();
        int currentPageGroupStartPageNumber = CalculateStartPageNumber();
        if (!(currentPageGroupStartPageNumber > 0)) {
            String msg = "currentPageGroupStartPageNumber should be greater than 0. {> 0} But:";
            msg = msg + " currentPageGroupStartPageNumber=" + currentPageGroupStartPageNumber;
            throw new IllegalStateException(msg);
        }
        int nextPageGroupStartPageNumber = currentPageGroupStartPageNumber + PageGroupOption.PageGroupSize;
        return (nextPageGroupStartPageNumber <= AllPageCount);
    }

    // ===================================================================================
    //                                                                       Assist Helper
    //                                                                       =============
    protected int[] ConvertListToIntArray(System.Collections.Generic.IList<int> ls) {
        int[] resultArray = new int[ls.Count];
        int arrayIndex = 0;
        foreach (int pageNumber in ls) {
            resultArray[arrayIndex] = pageNumber;
            arrayIndex++;
        }
        return resultArray;
    }

    protected void AssertPageGroupValid() {
        if (PageGroupOption == null) {
            String msg = "The pageGroupOption should not be null. Please call setPageGroupOption().";
            throw new IllegalStateException(msg);
        }
        if (PageGroupOption.PageGroupSize == 0) {
            String msg = "The pageGroupSize should be greater than 1. But the value is zero.";
            msg = msg + " pageGroupSize=" + PageGroupOption.PageGroupSize;
            throw new IllegalStateException(msg);
        }
        if (PageGroupOption.PageGroupSize == 1) {
            String msg = "The pageGroupSize should be greater than 1. But the value is one.";
            msg = msg + " pageGroupSize=" + PageGroupOption.PageGroupSize;
            throw new IllegalStateException(msg);
        }
    }

    // ===================================================================================
    //                                                                      Basic Override
    //                                                                      ==============
    public override String ToString() {
        StringBuilder sb = new StringBuilder();
        sb.append("{");
        sb.append("currentPageNumber=").append(CurrentPageNumber);
        sb.append(", allPageCount=").append(AllPageCount);
        sb.append(", pageGroupOption=").append(PageGroupOption);
        sb.append("}");
        return sb.toString();
    }

    // ===================================================================================
    //                                                                            Property
    //                                                                            ========
    public int CurrentPageNumber { get { return _currentPageNumber; } set { _currentPageNumber = value; } }
    public int AllPageCount { get { return _allPageCount; } set { _allPageCount = value; } }
    public PageGroupOption PageGroupOption { get { return _pageGroupOption; } set { _pageGroupOption = value; } }

    // -----------------------------------------------------
    //                                   Calculated Property
    //                                   -------------------
    public int PreGroupNearestPageNumber { get {
        if (!IsExistPrePageGroup()) {
            String msg = "The previous page range should exist when you use preGroupNearestPageNumber:";
            msg = msg + " currentPageNumber=" + _currentPageNumber + " allPageCount=" + _allPageCount;
            msg = msg + " pageGroupOption=" + _pageGroupOption;
            throw new IllegalStateException(msg);
        }
        return CreatePageNumberList()[0] - 1;
    }}

    public int NextGroupNearestPageNumber { get {
        if (!IsExistNextPageGroup()) {
            String msg = "The next page range should exist when you use nextGroupNearestPageNumber:";
            msg = msg + " currentPageNumber=" + _currentPageNumber + " allPageCount=" + _allPageCount;
            msg = msg + " pageGroupOption=" + _pageGroupOption;
            throw new IllegalStateException(msg);
        }
        System.Collections.Generic.IList<int> ls = CreatePageNumberList();
        return ls[ls.Count - 1] + 1;
    }}
}

}
