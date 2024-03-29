
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado {

    /**
     * The handler of SQL result. <br />
     * This handler is called back after executing the SQL and mapping entities. <br />
     * (before you get the result)
     * <pre>
     * [SqlResultInfo]
     * o result : The result(mapped object) of executed SQL. (NullAllowed)
     * o commandName : The name of executed command. (for display only) (NotNull)
     * o displaySql : The latest executed SQL for display. (for display only) (NullAllowed: if the SQL would be not executed)
     * o beforeDateTime : The date time before executing command(after initializing executions).
     * o afterDateTime : The date time after executing command(after mapping entities).
     * </pre>
     * <p>
     * Attention: <br />
     * If the SQL would be not executed, the displaySql in the information is null.
     * For example, update() that the entity has no modification. <br />
     * And if the command would be for batch, this is called back only once in a command.
     * So the displaySql is the latest SQL in a command at that time.
     * </p>
     * @param info The information of executed SQL result. (NotNull)
     */
    public delegate void SqlResultHandler(SqlResultInfo info);
}
