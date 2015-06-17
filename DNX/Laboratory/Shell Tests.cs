using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shell;


namespace Laboratory
{
    [TestClass]
    public class Shell_Tests
    {
        static object locker = new object();

        bool isShellClosed = false;
        public bool IsShellClosed { get { lock (locker) { return isShellClosed; } } set { lock (locker) { isShellClosed = value; } } }


        [TestMethod]
        public void Init_test()
        {
            IsShellClosed = false;
             ShellForm form;
            try
            {
               form = new ShellForm();
            }
            catch (Exception EX)
            {
                Assert.Fail("could not create form " + EX.Message);
                return;
            }

            form.FormClosed += form_FormClosed;

            try
            {
                //form.Show();
            }
            catch (Exception EX)
            {
                Assert.Fail("could not show form" + EX.Message);
                return;
            }

            while (!IsShellClosed)
            { 
                
            }

            

        }

        

        void form_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            isShellClosed = true;
        }
    }
}
