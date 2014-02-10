
using System;
using System.Collections;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean.SClause;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean {

    /// <summary>
    /// The condition-bean as interface.
    /// Author: DBFlute(AutoGenerator)
    /// </summary>
    public interface OrderByBean {

        /// <summary>
        /// The property of sql component of order-by clause.
        /// </summary>
        OrderByClause SqlComponentOfOrderByClause { get; }

        /**
         * Clear order-by.
         * 
         * @return this. (NotNull)
         */
        OrderByBean ClearOrderBy();

	    /**
	     * Ignore order-by.
	     * 
	     * @return this. (NotNull)
	     */
        OrderByBean IgnoreOrderBy();

	    /**
	     * Make order-by effective.
	     * 
	     * @return this. (NotNull)
	     */
        OrderByBean MakeOrderByEffective();

	    /**
	     * Get order-by clause.
	     * 
	     * @return Order-by clause. (NotNull)
	     */
	    String OrderByClause { get; }
    }
}
