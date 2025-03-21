using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium.Edge;
using excel = Microsoft.Office.Interop.Excel;
using OpenQA.Selenium.Chrome;
using System;

namespace TestLogIn
{
    [TestFixture]
    public class TestMatKhauLogIn
    {
        private static IWebDriver driver2;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void Setup()
        {
           driver2 = new EdgeDriver();
           driver2.Manage().Window.Maximize();
           driver2.Navigate().GoToUrl("https://localhost:44317/DKDN/Login");

        }

        public static IEnumerable<TestCaseData> Test_Pass()
        { 
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open("F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx");
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Worksheets[3];
            excel.Range range = wsheet.UsedRange;

            string taikhoansdt, matkhau;

            for (int i = 2; i <= range.Rows.Count; i++) 
            {
                taikhoansdt = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                matkhau = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";

                if(string.IsNullOrWhiteSpace(taikhoansdt) || string.IsNullOrWhiteSpace(matkhau))
                    continue;

                yield return new TestCaseData(taikhoansdt,matkhau);
            }
            wbook.Close(false);
            app.Quit();
            Marshal.ReleaseComObject(wsheet);
            Marshal.ReleaseComObject(wbook);
            Marshal.ReleaseComObject(app);
        }


        [Test, TestCaseSource(nameof(Test_Pass))]
        public void Test_PassLogin(string taikhoansdt, string matkhau)
        {
            WebDriverWait wait = new WebDriverWait(driver2, TimeSpan.FromSeconds(20));
            IWebElement tksdt = wait.Until(d => d.FindElement(By.Name("Sdt")));
            tksdt.SendKeys(taikhoansdt);

            IWebElement mksdt = driver2.FindElement(By.Name("Matkhau"));
            mksdt.SendKeys(matkhau);

            IWebElement btn_click_pass = driver2.FindElement(By.ClassName("login-btn"));
            btn_click_pass.Click();


            bool dntc;
            try
            {
                IWebElement welcomeMessage = wait.Until(d =>
                {
                    var elements = d.FindElements(By.ClassName("user_img"));
                    return elements.Count > 0 ? elements[0] : null;
                });
                dntc = welcomeMessage != null && welcomeMessage.Displayed;
            }
            catch (NoSuchElementException)
            {
                dntc = false;
            }
            catch (WebDriverTimeoutException)
            {
                dntc = false;
            }

            TrongExcel(taikhoansdt, matkhau, dntc ? "Pass" : "Fail");

            if (dntc)
            {
                driver2.FindElement(By.XPath("//*[@id=\"actions\"]/div[2]/div[1]/div/img")).Click();
                wait.Until(d => d.FindElement(By.XPath("//*[@id=\"actions\"]/div[2]/div[2]/ul/li[3]/form"))).Click();       
            }
            Assert.IsTrue(dntc, $"Login failed for {taikhoansdt}");
    }
        private void TrongExcel(string taikhoansdt, string matkhau, string result)
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open("F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx");
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Worksheets[3];
            excel.Range range = wsheet.UsedRange;


            for (int i = 2; i <= range.Rows.Count; i++)
            {
                string crtaikhoansdt = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                string crmatkhau = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";

                if (crtaikhoansdt == taikhoansdt && crmatkhau == matkhau)
                {
                    wsheet.Cells[i, 3] = result;
                    break;
                }
            }

            wbook.Save();
            wbook.Close(false);
            app.Quit();

            Marshal.ReleaseComObject(wbook);
            Marshal.ReleaseComObject(app);
        }

        [TearDown]
        public void TearDown()
        {
            driver2.Quit();
            driver2.Dispose();
        }
    }
}
