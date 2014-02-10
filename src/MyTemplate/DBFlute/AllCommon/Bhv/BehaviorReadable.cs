
using System;
using System.Collections.Generic;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.Dbm;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv {

    public interface BehaviorReadable {

        // ===============================================================================
        //                                                                Initialized Mark
        //                                                                ================
        bool IsInitialized { get; }

        // ===============================================================================
        //                                                                      Table Name
        //                                                                      ==========
        String TableDbName { get; }

        // ===============================================================================
        //                                                                          DBMeta
        //                                                                          ======
        DBMeta DBMeta { get; }

        // ===============================================================================
        //                                                                    New Instance
        //                                                                    ============
        Entity NewEntity();
        ConditionBean NewConditionBean();

        // ===============================================================================
        //                                                                    Basic Select
        //                                                                    ============
        /// <summary>
        /// Read count.
        /// </summary>
        /// <param name="cb">Condition-bean. (NotNull)</param>
        /// <returns>Read count.</returns>
        int ReadCount(ConditionBean cb);

        /// <summary>
        /// Read entity.
        /// </summary>
        /// <param name="cb">Condition-bean. (NotNull)</param>
        /// <returns>Read entity. (NullAllowed)</returns>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityDuplicatedException">When the entity has been duplicated.</exception>
        Entity ReadEntity(ConditionBean cb);

        /// <summary>
        /// Read entity with deleted check.
        /// </summary>
        /// <param name="cb">Condition-bean. (NotNull)</param>
        /// <returns>Read entity. (NotNull)</returns>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityAlreadyDeletedException">When the entity has been deleted by other thread.</exception>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityDuplicatedException">When the entity has been duplicated.</exception>
        Entity ReadEntityWithDeletedCheck(ConditionBean cb);

        // unsupported for generic problem
        //ListResultBean<Entity> ReadList(ConditionBean cb);

        // unsupported for generic problem
        //PagingResultBean<Entity> ReadPage(ConditionBean cb);
    }
}
