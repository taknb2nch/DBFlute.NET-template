
using System;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.Ado {

public class SqlResultInfo {

    // ===================================================================================
    //                                                                           Attribute
    //                                                                           =========
    protected Object _result;
    protected String _commandName;
    protected String _displaySql;
    protected DateTime _beforeDateTime;
    protected DateTime _afterDateTime;

    // ===================================================================================
    //                                                                            Accessor
    //                                                                            ========
    public Object Result { get {
        return _result;
    } set {
        this._result = value;
    }}

    public String CommandName { get {
        return _commandName;
    } set {
        this._commandName = value;
    }}

    public String DisplaySql { get {
        return _displaySql;
    } set {
        this._displaySql = value;
    }}

    public DateTime BeforeDateTime { get {
        return _beforeDateTime;
    } set {
        this._beforeDateTime = value;
    }}

    public DateTime AfterDateTime { get {
        return _afterDateTime;
    } set {
        this._afterDateTime = value;
    }}
}

}
