
using System;
using System.Text;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Util {

    public class DfCollectionUtil {
    
        // ===================================================================================
        //                                                                              System
        //                                                                              ======
        public static System.Collections.Generic.IList<ENTITY> ToGenericList<ENTITY>(System.Collections.IList plainList) {
            System.Collections.Generic.IList<ENTITY> resultList;
            if (!plainList.GetType().IsGenericType) {
                resultList = new System.Collections.Generic.List<ENTITY>();
                foreach (Object obj in plainList) {
                    resultList.Add((ENTITY)obj);
                }
            } else {
                resultList = (System.Collections.Generic.IList<ENTITY>)plainList;
            }
            return resultList;
        }
    }
}
