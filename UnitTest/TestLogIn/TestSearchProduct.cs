using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using excel = Microsoft.Office.Interop.Excel;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace TestLogIn
{
    [TestFixture]
    public class TestSearchProduct
    {
        private static IWebDriver driver7;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void Setup()
        {
            driver7 = new EdgeDriver();
            driver7.Manage().Window.Maximize();
            driver7.Navigate().GoToUrl("https://localhost:44317/");

            WebDriverWait wait = new WebDriverWait(driver7, TimeSpan.FromSeconds(10));
            IWebElement loginButton = wait.Until(d => d.FindElement(By.ClassName("button_dn")));

            Assert.IsTrue(loginButton.Displayed, "Trang chủ không có nút Đăng nhập");
        }

        public static IEnumerable<TestCaseData> Lamthu()
        {
            excel.Application xapp = new excel.Application();
            excel.Workbook xbook = xapp.Workbooks.Open(filePath);
            excel.Worksheet worksheet = (excel.Worksheet)xbook.Sheets["Search"];
            excel.Range xrange = worksheet.UsedRange;

            string namesp;

            for (int i = 2; i <= xrange.Rows.Count; i++)
            {
                namesp = Convert.ToString((xrange.Cells[i, 1] as excel.Range)?.Value) ?? "";

                if (string.IsNullOrWhiteSpace(namesp))
                    continue;

                yield return new TestCaseData(namesp);
            }

            xbook.Close(false);
            xapp.Quit();
            Marshal.ReleaseComObject(xbook);
            Marshal.ReleaseComObject(xapp);
        }

        [Test, TestCaseSource(nameof(Lamthu))]
        public void Search(string namesp)
        {
            WebDriverWait wait = new WebDriverWait(driver7, TimeSpan.FromSeconds(20));

            IWebElement showhome = wait.Until(d => d.FindElement(By.CssSelector("div.button-container")));
            Assert.IsTrue(showhome.Displayed);

            Thread.Sleep(2000);

            IWebElement click_sp = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"menu\"]/div[2]/a")));
            click_sp.Click();

            Thread.Sleep(2000);

            IWebElement sotrang = wait.Until(d => d.FindElement(By.CssSelector("div.tttrang")));
            ((IJavaScriptExecutor)driver7).ExecuteScript("arguments[0].scrollIntoView(true);", sotrang);
            Assert.IsTrue(sotrang.Displayed);

            Thread.Sleep(1000);
            ((IJavaScriptExecutor)driver7).ExecuteScript("window.scrollTo(0, 0);");

            IWebElement searchpro = wait.Until(d => d.FindElement(By.CssSelector("div.timkiemsp input")));
            searchpro.SendKeys(namesp);

            Thread.Sleep(2000);

            searchpro.SendKeys(Keys.Enter);

            Thread.Sleep(5000);

            bool found = false;
            bool nexttrang = true;

            while (nexttrang)
            {
                ((IJavaScriptExecutor)driver7).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                Thread.Sleep(1500);

                ((IJavaScriptExecutor)driver7).ExecuteScript("window.scrollTo(0, 270);");
                Thread.Sleep(1000);

                var dssanpham = driver7.FindElements(By.CssSelector("div.detail h4"));

                foreach (var product in dssanpham)
                {
                    string tensp = product.Text.Trim();
                    if (tensp.Contains(namesp, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Sản phẩm có : " + tensp);
                        found = true;
                    }
                }

                var btnnextpage = driver7.FindElements(By.CssSelector("li.PagedList-skipToNext a"));
                if (btnnextpage.Count > 0)
                {
                    IWebElement quatrang = btnnextpage[0];
                    ((IJavaScriptExecutor)driver7).ExecuteScript("arguments[0].scrollIntoView(true);", quatrang);
                    Thread.Sleep(1000);
                    ((IJavaScriptExecutor)driver7).ExecuteScript("arguments[0].click();", quatrang);
                    Thread.Sleep(3000);
                }
                else
                {
                    nexttrang = false;
                }
            }

            TrongExcel(namesp, found ? "Pass" : "Fail");
            Assert.IsTrue(found, $"Không tìm thấy sản phẩm '{namesp}' trên các trang.");
        }

        private void TrongExcel(string namesp, string result)
        {
            excel.Application xapp = new excel.Application();
            excel.Workbook xbook = xapp.Workbooks.Open(filePath);
            excel.Worksheet worksheet = (excel.Worksheet)xbook.Sheets["Search"];
            excel.Range xrange = worksheet.UsedRange;

            for (int i = 2; i <= xrange.Rows.Count; i++)
            {
                string crnamesp = Convert.ToString((xrange.Cells[i, 1] as excel.Range)?.Value) ?? "";

                if (crnamesp.Equals(namesp, StringComparison.OrdinalIgnoreCase))
                {
                    worksheet.Cells[i, 2] = result;
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
            driver7.Quit();
            driver7.Dispose();
        }
    }
}
