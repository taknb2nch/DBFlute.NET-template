using System;
using System.Collections.Generic;
using System.Text;

using log4net;

using Seasar.Quill.Attrs;

using Aaa.Bbb.Ccc.DBFlute.AllCommon;
using Aaa.Bbb.Ccc.DBFlute.AllCommon.CBean;
using Aaa.Bbb.Ccc.DBFlute.CBean;
using Aaa.Bbb.Ccc.DBFlute.ExBhv;
using Aaa.Bbb.Ccc.DBFlute.ExDao.PmBean;
using Aaa.Bbb.Ccc.DBFlute.ExEntity;
using Aaa.Bbb.Ccc.DBFlute.ExEntity.Customize;
using Aaa.Bbb.Ccc.Interceptors;


namespace Aaa.Bbb.Ccc.Logic
{
    [Implementation]
    [Transaction]
    [Aspect(typeof(AccessContextInterceptor))]
    public class myLogic
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected MyTableBhv myTableBhv;

        // AOPを有効にするために必ずvirtualにする
        public virtual void Test1()
        {
            // 全件削除
            myTableBhv.QueryDelete(new MyTableCB());

            //
            Random r = new Random();

            for (int i = 1; i <= 10; i++)
            {
                MyTable et = new MyTable();
                et.UserName = "ユーザ" + i;
                et.Age = r.Next(99) + 1;

                if (r.Next(100) > 50)
                {
                    et.SetAttendanceFlag_True();
                }
                else
                {
                    et.SetAttendanceFlag_False();
                }

                myTableBhv.Insert(et);
            }
        }

        public virtual void Test2()
        {
            MyTableCB cb = new MyTableCB();
            cb.AddOrderBy_PK_Asc();

            ListResultBean<MyTable> list = myTableBhv.SelectList(cb);

            foreach (MyTable et in list)
            {
                _logger.InfoFormat("{0}, {1}, {2}, {3}", et.Id, et.UserName, et.Age, et.AttendanceFlagAlias);
            }
        }

        public virtual void Test3()
        {
            MyTableExPmb pmb = new MyTableExPmb();
            pmb.Age = 40;

            ListResultBean<MyTableEx> list =
                myTableBhv.OutsideSql().SelectList<MyTableEx>(MyTableBhv.PATH_selectMyTableEx, pmb);

            foreach (MyTableEx et in list)
            {
                _logger.InfoFormat("{0}, {1}, {2}, {3}", et.Id, et.UserName, et.Age, et.AttendanceFlagAlias);
            }
        }
    }
}
