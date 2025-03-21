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
using Microsoft.Office.Interop.Excel;

namespace TestLogIn
{
    [TestFixture]
    public class TestDangKyMail
    {
        private static IWebDriver driver4;

        [SetUp]
        public void Setup()
        {
            driver4 = new EdgeDriver();
            driver4.Manage().Window.Maximize();
            driver4.Navigate().GoToUrl("https://localhost:44317/");

            WebDriverWait wait = new WebDriverWait(driver4, TimeSpan.FromSeconds(10));

            IWebElement loginButton = wait.Until(d => d.FindElement(By.ClassName("button_dk")));
            Assert.IsTrue(loginButton.Displayed, "Trang chủ không có nút Đăng ky");

        }

        public static IEnumerable<TestCaseData> Test_DK_Mail()
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open("F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx");
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Worksheets[5];
            excel.Range range = wsheet.UsedRange;

            string fname, lname, email, pass, cfpass;

            for (int i = 2; i <= range.Rows.Count; i++)
            {
                fname = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                lname = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";
                email = Convert.ToString((range.Cells[i, 3] as excel.Range)?.Value) ?? "";
                pass = Convert.ToString((range.Cells[i, 4] as excel.Range)?.Value) ?? "";
                cfpass = Convert.ToString((range.Cells[i, 5] as excel.Range)?.Value) ?? "";

                
                yield return new TestCaseData(fname, lname, email, pass, cfpass);
            }
            wbook.Close(false);
            app.Quit();
            Marshal.ReleaseComObject(wsheet);
            Marshal.ReleaseComObject(wbook);
            Marshal.ReleaseComObject(app);
        }


        [Test, TestCaseSource(nameof(Test_DK_Mail))]
        public void Test_PassLogin(string fname, string lname, string email, string pass, string cfpass)
        {
            WebDriverWait wait = new WebDriverWait(driver4, TimeSpan.FromSeconds(15));

            wait.Until(d => d.FindElement(By.ClassName("button_dk"))).Click();
            wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div/div/div[2]/div[2]/a"))).Click();

            wait.Until(d => d.FindElement(By.Id("FirstName"))).SendKeys(fname);
            wait.Until(d => d.FindElement(By.Id("LastName"))).SendKeys(lname);
            wait.Until(d => d.FindElement(By.Id("Email"))).SendKeys(email);
            wait.Until(d => d.FindElement(By.Id("Password"))).SendKeys(pass);
            wait.Until(d => d.FindElement(By.Id("ConfirmPassword"))).SendKeys(cfpass);

            wait.Until(d => d.FindElement(By.XPath("/html/body/div/div[2]/form/button"))).Click();

            bool isRedirected;
            try
            {
                isRedirected = wait.Until(d => d.Url.Contains("login")); 
            }
            catch (WebDriverTimeoutException)
            {
                isRedirected = false;
            }

            if (!isRedirected)
            {
                Console.WriteLine("Đăng ký thất bại");
                TrongExcel(fname, lname, email, pass, cfpass, "Fail");
                driver4.Quit();

                Assert.Fail("Đăng ký thất bại");
                return;
            }


            Console.WriteLine("Đăng ký thành công");
            driver4.Navigate().GoToUrl("https://localhost:44317/login");
            wait.Until(d => d.FindElement(By.Id("Email"))).SendKeys(email);
            wait.Until(d => d.FindElement(By.Id("Password"))).SendKeys(pass);
            wait.Until(d => d.FindElement(By.ClassName("login-btn"))).Click();

            bool loginSuccess;
            try
            {
                IWebElement dashboard = wait.Until(d =>
                {
                    var elements = d.FindElements(By.ClassName("user_img"));
                    return elements.Count > 0 ? elements[0] : null;
                });

                loginSuccess = dashboard != null && dashboard.Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                loginSuccess = false;
            }

            TrongExcel(fname, lname, email, pass, cfpass, loginSuccess ? "Pass" : "Fail");

            driver4.Quit();
        }




        private void TrongExcel(string fname, string lname,string email, string pass, string cfpass, string result)
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open("F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx");
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Sheets[5];
            excel.Range range = wsheet.UsedRange;

            for (int i = 2; i <= range.Rows.Count; i++)
            {

                string crfname = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                string crlname = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";
                string cremail = Convert.ToString((range.Cells[i, 3] as excel.Range)?.Value) ?? "";
                string crpass = Convert.ToString((range.Cells[i, 4] as excel.Range)?.Value) ?? "";
                string crcfpass = Convert.ToString((range.Cells[i, 5] as excel.Range)?.Value) ?? "";


                if (crfname == fname && crlname == lname &&   cremail == email && crpass == pass && crcfpass == cfpass)
                {
                    wsheet.Cells[i, 6] = result;
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
            driver4.Quit();
            driver4.Dispose();
        }
    }
}
