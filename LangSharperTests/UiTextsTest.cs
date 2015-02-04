using System.IO;
using LangSharper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LangSharperTests
{
    [TestClass]
    public class UiTextsTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            UiTexts ut = new UiTexts(TestGlobals.Path + "uitexttest.ls");
            Assert.AreEqual(5, ut.GetLength());


            Assert.AreEqual(ut.Dict.Count, 5);

            try
            {
                ut = new UiTexts("wrongfile.txt");
            }
            catch (IOException e)
            {
                return;
            }

            Assert.Fail("No exception was thrown");

        }
    }
}
