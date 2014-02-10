
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Exp {

    /// <summary>
    /// The exception of when the entity already exists on the database.
    /// Author: DBFlute(AutoGenerator)
    /// </summary>
    public class EntityAlreadyExistsException : SQLFailureException {

        public EntityAlreadyExistsException(String msg, Exception e)
        : base(msg,e) {}
    }
}
