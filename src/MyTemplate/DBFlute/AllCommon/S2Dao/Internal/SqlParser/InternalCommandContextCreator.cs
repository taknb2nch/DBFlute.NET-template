
using System;
using Seasar.Dao;
using Seasar.Dao.Context;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao.Internal.SqlParser {

    public class InternalCommandContextCreator {
    
        // ===================================================================================
        //                                                                           Attribute
        //                                                                           =========
        protected String[] _argNames;
        protected Type[] _argTypes;
    
        // ===================================================================================
        //                                                                         Constructor
        //                                                                         ===========
    	public InternalCommandContextCreator(String[] argNames, Type[] argTypes) {
    	    this._argNames = (argNames != null ? argNames : new String[0]);
    	    this._argTypes = (argTypes != null ? argTypes : new Type[0]);
    	}
    	
        // ===================================================================================
        //                                                                              Create
        //                                                                              ======
        public virtual ICommandContext CreateCommandContext(object[] args) {
            ICommandContext ctx = NewCommandContext();
            if (args != null) {
                for (int i = 0; i < args.Length; ++i) {
                    Type argType = null;
                    if (args[i] != null) {
                        if (i < _argTypes.Length) {
                            argType = _argTypes[i];
						} else if (args[i] != null) {
                            argType = args[i].GetType();
						}
                    }
                    if (i < _argNames.Length) {
                        ctx.AddArg(_argNames[i], args[i], argType);
					} else {
                        ctx.AddArg("$" + (i + 1), args[i], argType);
					}
                }
            }
            return ctx;
        }
		
        protected virtual ICommandContext NewCommandContext() {
            return new CommandContextImpl();
        }
    }
}
