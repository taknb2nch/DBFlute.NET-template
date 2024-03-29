
using System;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.JavaLike;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.PageNavi {

[System.Serializable]
public class PageNumberLink {

    // ===================================================================================
    //                                                                         Initializer
    //                                                                         ===========
    public PageNumberLink Initialize(int pageNumberElement, bool current, String pageNumberLinkHref) {
        PageNumberElement = pageNumberElement;
        IsCurrent = current;
        PageNumberLinkHref = pageNumberLinkHref;
        return this;
    }

    // ===================================================================================
    //                                                                      Basic Override
    //                                                                      ==============
    public override String ToString() {
        StringBuilder sb = new StringBuilder();

        sb.append(" pageNumberElement=").append(PageNumberElement);
        sb.append(" pageNumberLinkHref=").append(PageNumberLinkHref);
        sb.append(" current=").append(IsCurrent);

        return sb.toString();
    }

    // ===================================================================================
    //                                                                            Accessor
    //                                                                            ========
    public int _pageNumberElement;
    public int PageNumberElement { get { return _pageNumberElement; } set { _pageNumberElement = value; } }
    public bool _isCurrent;
    public bool IsCurrent { get { return _isCurrent; } set { _isCurrent = value; } }
    public String _pageNumberLinkHref;
    public String PageNumberLinkHref { get { return _pageNumberLinkHref; } set { _pageNumberLinkHref = value; } }
}

}
