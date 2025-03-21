using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OfficeOpenXml;
using SeleniumExtras.WaitHelpers;
using NUnit.Framework;

namespace TestLogIn
{
    [TestFixture]
    public class TestAddCartProduct
    {
        private static IWebDriver driver;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void SetUp()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://localhost:44317/");
        }

        public static IEnumerable<TestCaseData> addproduct()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["AddProduct"];
                int rowCount = worksheet.Dimension.Rows;

                for (int i = 2; i <= rowCount; i++)
                {
                    string tensp = worksheet.Cells[i, 1].Text;
                    string size = worksheet.Cells[i, 2].Text;
                    string da = worksheet.Cells[i, 3].Text;
                    string duong = worksheet.Cells[i, 4].Text;
                    string topping = worksheet.Cells[i, 5].Text;

                    if (!string.IsNullOrWhiteSpace(tensp))
                    {
                        yield return new TestCaseData(tensp, size, da, duong, topping);
                    }
                }
            }
        }

        [Test, TestCaseSource(nameof(addproduct))]
        public void AddProduct(string tensp, string size, string da, string duong, string topping)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            try
            {
                IWebElement menuItem = wait.Until(d => d.FindElement(By.XPath("//*[@id='menu']/div[2]/a")));
                menuItem.Click();
                Thread.Sleep(5000);

                IWebElement sotrang = wait.Until(d => d.FindElement(By.CssSelector("div.tttrang")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", sotrang);
                Assert.IsTrue(sotrang.Displayed);
                Thread.Sleep(1000);

                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0);");
                Thread.Sleep(2000);

                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 200);");
                Thread.Sleep(2000);

                IWebElement product = wait.Until(d => d.FindElement(By.XPath($"//h4[contains(text(), '{tensp}')]//ancestor::div[contains(@class, 'product-card')]/a")));
                product.Click();
                Thread.Sleep(2000);

                IWebElement productTypeElement = wait.Until(d => d.FindElement(By.CssSelector("dl.thydo dd.tensp")));
                string productType = productTypeElement.Text.Trim().ToLower();
                Console.WriteLine($"Loại sản phẩm: {productType}");

                if (!productType.Contains("bánh") && !productType.Contains("Buổi"))
                {
                    SelectOption("input[name='idsize']", size);
                    Thread.Sleep(2000);
                    SelectOption("input[name='idda']", da);
                    Thread.Sleep(2000);
                    SelectOption("input[name='idduong']", duong);
                    Thread.Sleep(2000);

                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 250);");

                    if (productType.Contains("trà") && !string.IsNullOrEmpty(topping))
                    {
                        IWebElement chontopping = wait.Until(d => d.FindElement(By.CssSelector("div.hihi")));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", chontopping);
                        Thread.Sleep(2000);
                    }
                }

                Thread.Sleep(3000);

                IWebElement addToCartButton = wait.Until(d => d.FindElement(By.CssSelector("div.button-group button")));
                addToCartButton.Click();

                Console.WriteLine("Đã thêm sản phẩm vào giỏ hàng.");
                Thread.Sleep(5000);

                TrongExcel(tensp, "Pass");
            }
            catch (Exception ex)
            {
                Console.WriteLine("thất bại: " + ex.Message);
                TrongExcel(tensp, "Fail");
                Assert.Fail("thất bại: " + ex.Message);
            }
        }

        private void SelectOption(string selector, string value)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            try
            {
                IWebElement element = wait.Until(d => d.FindElement(By.CssSelector(selector)));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"Không tìm thấy: {value}");
            }
        }

        private void TrongExcel(string tensp, string result)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["AddProduct"];
                int rowCount = worksheet.Dimension.Rows;

                for (int i = 2; i <= rowCount; i++)
                {
                    if (worksheet.Cells[i, 1].Text == tensp)
                    {
                        worksheet.Cells[i, 6].Value = result;
                        package.Save();
                        break;
                    }
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
