using System;
using System.Collections.Generic;
using System.Text;

using Seasar.Framework.Aop;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;

namespace Aaa.Bbb.Ccc.Interceptors
{
    public class AccessContextInterceptor : IMethodInterceptor
    {
        #region IMethodInterceptor ÉÅÉìÉo

        public object Invoke(IMethodInvocation invocation)
        {
            AccessContext context = new AccessContext();
            context.AccessTimestamp = DateTime.Now;
            context.AccessUser = ClientSession.User;

            AccessContext.SetAccessContextOnThread(context);

            object ret = invocation.Proceed();

            return ret;
        }

        #endregion
    }
}
