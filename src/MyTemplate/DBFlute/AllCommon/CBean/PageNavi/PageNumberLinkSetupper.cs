
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.PageNavi {

    public delegate LINK PageNumberLinkSetupper<LINK>(int pageNumberElement, bool current) where LINK : PageNumberLink;
}

