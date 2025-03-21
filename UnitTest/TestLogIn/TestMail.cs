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
    public class TestMail
    {
        public static IWebDriver driver;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";
        [SetUp]
        public void Setup()
        {
         
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://localhost:44317/");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement loginButton = wait.Until(d => d.FindElement(By.ClassName("button_dn")));

            Assert.IsTrue(loginButton.Displayed, "Trang chủ không có nút Đăng nhập");
        }
        public static IEnumerable<TestCaseData> test_Login_Email()
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open("F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx");
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Sheets[2];
            excel.Range range = wsheet.UsedRange;

            string email, matkhau;

            for (int i = 2; i <= range.Rows.Count; i++)
            {
                email = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                matkhau = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";

                //if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matkhau))
                //    continue;

                yield return new TestCaseData(email, matkhau);
            }

            wbook.Close(false);
            app.Quit();
            Marshal.ReleaseComObject(wsheet);
            Marshal.ReleaseComObject(wbook);
            Marshal.ReleaseComObject(app);
        }

        [Test, TestCaseSource(nameof(test_Login_Email))]
        public void Login_email(string email, string matkhau)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            IWebElement btn_login = wait.Until(d => d.FindElement(By.ClassName("button_dn")));
            btn_login.Click();

            IWebElement loginmeo = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div/div/div[2]/div[2]/a[2]")));
            loginmeo.Click();

            Thread.Sleep(1000);

            IWebElement tke = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"Email\"]")));
            tke.SendKeys(email);

            IWebElement mke = driver.FindElement(By.XPath("//*[@id=\"Password\"]"));
            mke.SendKeys(matkhau);

            IWebElement btn_clickmeo = driver.FindElement(By.XPath("/html/body/div/div[2]/form/button"));
            btn_clickmeo.Click();


            bool loginemail = false;
            try
            {
                var elements = wait.Until(d => d.FindElements(By.ClassName("user_img")));
                Console.WriteLine($"Number of user_img elements found: {elements.Count}");

                if (elements.Count > 0 && elements[0].Displayed && elements[0].Enabled)
                {
                    loginemail = true;
                }
            }
            catch (NoSuchElementException)
            {
                loginemail = false;
            }
            catch (WebDriverTimeoutException)
            {
                loginemail = false;
            }

            Console.WriteLine($"Login status for {email}: {(loginemail ? "Pass" : "Fail")}");
            TrongExcel(email, matkhau, loginemail ? "Pass" : "Fail");
            Assert.IsTrue(loginemail, $"Login failed for {email}");

        }

        private void TrongExcel(string email, string matkhau, string result)
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open("F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx");
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Sheets[2];
            excel.Range range = wsheet.UsedRange;

            for (int i = 2; i <= range.Rows.Count; i++)
            {

                string cremail = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                string crmatkhau = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";

                if (cremail == email && crmatkhau == matkhau)
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
            driver.Quit();
            driver.Dispose();
        }
    }
}
