
using System;

using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.PageNavi;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.PageNavi.Range {

[System.Serializable]
public class PageRangeBean {

    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
    protected int _currentPageNumber;
    protected int _allPageCount;
    protected PageRangeOption _pageRangeOption;
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
        AssertPageRangeValid();
        if (_cachedPageNumberList != null) {
            return _cachedPageNumberList;
        }
        int pageRangeSize = PageRangeOption.PageRangeSize;
        int allPageCount = AllPageCount;
        int currentPageNumber = CurrentPageNumber;

        System.Collections.Generic.IList<int> resultList = new System.Collections.Generic.List<int>();
        for (int i = currentPageNumber - pageRangeSize; i < currentPageNumber; i++) {
            if (i < 1) {
                continue;
            }
            resultList.Add(i);
        }

        resultList.Add(currentPageNumber);

        int endPageNumber = (currentPageNumber + pageRangeSize);
        for (int i = currentPageNumber + 1 ; i <= endPageNumber && i <= allPageCount; i++) {
            resultList.Add(i);
        }

        bool fillLimit = PageRangeOption.IsFillLimit;
        int limitSize = (pageRangeSize * 2) + 1;
        if (fillLimit && resultList.Count != 0 && resultList.Count < limitSize) {
            int firstElements = resultList[0];
            int lastElements = resultList[resultList.Count - 1];
            if (firstElements > 1) {
                for (int i = firstElements - 1 ; resultList.Count < limitSize && i > 0; i--) {
                    resultList.Insert(0, i);
                }
            }
            for (int i = lastElements + 1 ; resultList.Count < limitSize && i <= allPageCount; i++) {
                resultList.Add(i);
            }
        }
        _cachedPageNumberList = resultList;
        return _cachedPageNumberList;
    }

    public int[] CreatePageNumberArray() {
        AssertPageRangeValid();
        return ConvertListToIntArray(CreatePageNumberList());
    }

    // ===================================================================================
    //                                                                       Determination
    //                                                                       =============
    public bool IsExistPrePageRange() {
        AssertPageRangeValid();
        int[] array = CreatePageNumberArray();
        if (array.Length == 0) {
            return false;
        }
        return array[0] > 1;
    }

    public bool IsExistNextPageRange() {
        AssertPageRangeValid();
        int[] array = CreatePageNumberArray();
        if (array.Length == 0) {
            return false;
        }
        return array[array.Length-1] < AllPageCount;
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

    protected void AssertPageRangeValid() {
        if (PageRangeOption == null) {
            String msg = "The pageRangeOption should not be null. Please call setPageRangeOption().";
            throw new IllegalStateException(msg);
        }
        int pageRangeSize = PageRangeOption.PageRangeSize;
        if (pageRangeSize == 0) {
            String msg = "The pageRangeSize should be greater than 1. But the value is zero.";
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
        sb.append(", pageRangeOption=").append(PageRangeOption);
        sb.append("}");
        return sb.toString();
    }

    // ===================================================================================
    //                                                                            Property
    //                                                                            ========
    public int CurrentPageNumber { get { return _currentPageNumber; } set { _currentPageNumber = value; } }
    public int AllPageCount { get { return _allPageCount; } set { _allPageCount = value; } }
    public PageRangeOption PageRangeOption { get { return _pageRangeOption; } set { _pageRangeOption = value; } }

    // -----------------------------------------------------
    //                                   Calculated Property
    //                                   -------------------
    public int PreRangeNearestPageNumber { get {
        if (!IsExistPrePageRange()) {
            String msg = "The previous page range should exist when you use preRangeNearestPageNumber:";
            msg = msg + " currentPageNumber=" + _currentPageNumber + " allPageCount=" + _allPageCount;
            msg = msg + " pageRangeOption=" + _pageRangeOption;
            throw new IllegalStateException(msg);
        }
        return CreatePageNumberList()[0] - 1;
    }}

    public int NextRangeNearestPageNumber { get {
        if (!IsExistNextPageRange()) {
            String msg = "The next page range should exist when you use nextRangeNearestPageNumber:";
            msg = msg + " currentPageNumber=" + _currentPageNumber + " allPageCount=" + _allPageCount;
            msg = msg + " pageRangeOption=" + _pageRangeOption;
            throw new IllegalStateException(msg);
        }
        System.Collections.Generic.IList<int> ls = CreatePageNumberList();
        return ls[ls.Count - 1] + 1;
    }}
}

}
