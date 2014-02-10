
using System;
using System.Collections;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Bhv {

    public interface BehaviorWritable : BehaviorReadable {

        /// <summary>
        /// Create entity.
        /// </summary>
        /// <param name="entity">Entity. (NotNull)</param>
        void Create(Entity entity);

        /// <summary>
        /// Modify entity.
        /// </summary>
        /// <param name="entity">Entity. (NotNull)</param>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityAlreadyUpdatedException">When the entity has already been updated by other thread.</exception>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityAlreadyDeletedException">When the entity has already been deleted by other thread.</exception>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityDuplicatedException">When the entity has been duplicated.</exception>
        void Modify(Entity entity);

        /// <summary>
        /// Remove entity.
        /// </summary>
        /// <param name="entity">Entity. (NotNull)</param>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityAlreadyUpdatedException">When the entity has already been updated by other thread.</exception>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityAlreadyDeletedException">When the entity has already been deleted by other thread.</exception>
        /// <exception cref="Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp.EntityDuplicatedException">When the entity has been duplicated.</exception>
        void Remove(Entity entity);
    }
}
