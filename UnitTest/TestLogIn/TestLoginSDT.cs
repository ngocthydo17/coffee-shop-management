using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using excel = Microsoft.Office.Interop.Excel;
using System;
using System.Runtime.InteropServices;
using System.IO;

namespace TestLogIn
{
    [TestFixture]
    public class TestLoginSDT
    {
        private static IWebDriver driver1;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void Setup()
        {
            driver1 = new EdgeDriver();
            driver1.Manage().Window.Maximize();
            driver1.Navigate().GoToUrl("https://localhost:44317/");

            WebDriverWait wait = new WebDriverWait(driver1, TimeSpan.FromSeconds(10));
            IWebElement loginButton = wait.Until(d => d.FindElement(By.ClassName("button_dn")));

            Assert.IsTrue(loginButton.Displayed, "Trang chủ không có nút Đăng nhập");
        }

        public static IEnumerable<TestCaseData> Lamthu()
        {
            excel.Application xapp = new excel.Application();
            excel.Workbook xbook = xapp.Workbooks.Open(filePath);
            excel.Worksheet worksheet = (excel.Worksheet)xbook.Sheets[1];
            excel.Range xrange = worksheet.UsedRange;

            string tk, pass;

            for (int i = 2; i <= xrange.Rows.Count; i++)
            {
                tk = Convert.ToString((xrange.Cells[i, 1] as excel.Range)?.Value) ?? "";
                pass = Convert.ToString((xrange.Cells[i, 2] as excel.Range)?.Value) ?? "";

                if (string.IsNullOrWhiteSpace(tk) || string.IsNullOrWhiteSpace(pass))
                    continue;

                yield return new TestCaseData(tk, pass);
            }

            xbook.Close(false);
            xapp.Quit();
            Marshal.ReleaseComObject(xbook);
            Marshal.ReleaseComObject(xapp);
        }

        [Test, TestCaseSource(nameof(Lamthu))]
        public void Test_Login(string tk, string pass)
        {
            WebDriverWait wait = new WebDriverWait(driver1, TimeSpan.FromSeconds(10));

            IWebElement btn_login = wait.Until(d=> d.FindElement(By.ClassName("button_dn")));
            btn_login.Click();

            IWebElement sdt = wait.Until(d => d.FindElement(By.Name("Sdt")));
            sdt.SendKeys(tk);

            IWebElement mk = driver1.FindElement(By.Name("Matkhau"));
            mk.SendKeys(pass);

            IWebElement btn_click = driver1.FindElement(By.ClassName("login-btn"));
            btn_click.Click();

            bool loginSuccess;
            try
            {
                IWebElement welcomeMessage = wait.Until(d =>
                {
                    var elements = d.FindElements(By.ClassName("user_img"));
                    return elements.Count > 0 ? elements[0] : null;
                });

                loginSuccess = welcomeMessage != null && welcomeMessage.Displayed;
            }
            catch (NoSuchElementException)
            {
                loginSuccess = false;
            }
            catch (WebDriverTimeoutException)
            {
                loginSuccess = false;
            }

            TrongExcel(tk, pass, loginSuccess ? "Pass" : "Fail");
            Assert.IsTrue(loginSuccess, $"Dang nhap that bai {tk}");
        }

        private void TrongExcel(string tk, string pass, string result)
        {
            excel.Application xapp = new excel.Application();
            excel.Workbook xbook = xapp.Workbooks.Open(filePath);
            excel.Worksheet worksheet = (excel.Worksheet)xbook.Sheets[1];
            excel.Range xrange = worksheet.UsedRange;

            for (int i = 2; i <= xrange.Rows.Count; i++)
            {
                string currentTk = Convert.ToString((xrange.Cells[i, 1] as excel.Range)?.Value) ?? "";
                string currentPass = Convert.ToString((xrange.Cells[i, 2] as excel.Range)?.Value) ?? "";

                if (currentTk == tk && currentPass == pass)
                {
                    worksheet.Cells[i, 3] = result;
                    break;
                }
            }

            xbook.Save();
            xbook.Close(false);
            xapp.Quit();

            Marshal.ReleaseComObject(xbook);
            Marshal.ReleaseComObject(xapp);
        }

        [TearDown]
        public void TearDown()
        {
            driver1.Quit();
            driver1.Dispose();
        }
    }
}
