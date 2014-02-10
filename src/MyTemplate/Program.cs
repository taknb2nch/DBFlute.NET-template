using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using log4net;
using log4net.Config;
using log4net.Util;

using Seasar.Quill;

using Aaa.Bbb.Ccc.Logic;


namespace Aaa.Bbb.Ccc
{
    class Program
    {
        private static readonly ILog _logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected myLogic myLogic;

        static void Main(string[] args)
        {
            // log4net�̏�����
            FileInfo info = new FileInfo(SystemInfo.AssemblyShortName(Assembly.GetExecutingAssembly()) + ".exe.config");
            XmlConfigurator.Configure(LogManager.GetRepository(), info);

            _logger.Info("application started.");

            Program p = new Program();

            // �K��Logic���܂�ł���N���X�ɑ΂���inject����
            QuillInjector injector = QuillInjector.GetInstance();
            injector.Inject(p);

            p.Execute();

            Console.WriteLine("push any key...");
            Console.ReadLine();

            _logger.InfoFormat("application finished. code: {0}", 0);
        }

        public void Execute()
        {
            try
            {
                ClientSession.User = "�e�X�g���[�U";

                myLogic.Test1();
                myLogic.Test2();
                myLogic.Test3();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
            }
        }
    }
}
