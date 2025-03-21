using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using excel = Microsoft.Office.Interop.Excel;
using SeleniumExtras.WaitHelpers;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using static NUnit.Framework.Internal.OSPlatform;




namespace TestLogIn
{
    [TestFixture]
    public class CheckCart
    {
        private static IWebDriver driver9;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";
        [SetUp]
        public void SetUp()
        {
            driver9 = new EdgeDriver();
            driver9.Manage().Window.Maximize();
            driver9.Navigate().GoToUrl("https://localhost:44317/");
        }
        public static IEnumerable<TestCaseData> checkproduct()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["CheckCart"];
                int rowCount = worksheet.Dimension.Rows;

                for (int i = 2; i <= rowCount; i++)
                {
                    string tensp = worksheet.Cells[i, 1].Text;
                    string size = worksheet.Cells[i, 2].Text;
                    string da = worksheet.Cells[i, 3].Text;
                    string duong = worksheet.Cells[i, 4].Text;
                    string topping = worksheet.Cells[i, 5].Text;
                    string buttonc = worksheet.Cells[i, 6].Text;

                    if (!string.IsNullOrWhiteSpace(tensp))
                    {
                        yield return new TestCaseData(tensp, size, da, duong, topping, buttonc);
                    }
                }
            }
        }
        private List<string[]> logs = new List<string[]>();

        [Test, TestCaseSource(nameof(checkproduct))]

        public void CheckCartSP(string tensp, string size, string da, string duong, string topping, string buttonc)
        {

            WebDriverWait wait = new WebDriverWait(driver9, TimeSpan.FromSeconds(15));

            try
            {
                IWebElement menuItem = wait.Until(d => d.FindElement(By.XPath("//*[@id='menu']/div[2]/a")));
                menuItem.Click();
                Thread.Sleep(5000);

                IWebElement sotrang = wait.Until(d => d.FindElement(By.CssSelector("div.tttrang")));
                ((IJavaScriptExecutor)driver9).ExecuteScript("arguments[0].scrollIntoView(true);", sotrang);
                Assert.IsTrue(sotrang.Displayed);
                Thread.Sleep(1000);

                ((IJavaScriptExecutor)driver9).ExecuteScript("window.scrollTo(0, 0);");
                Thread.Sleep(2000);

                ((IJavaScriptExecutor)driver9).ExecuteScript("window.scrollTo(0, 200);");
                Thread.Sleep(2000);

                IWebElement product = wait.Until(d => d.FindElement(By.XPath($"//h4[contains(text(), '{tensp}')]//ancestor::div[contains(@class, 'product-card')]/a")));
                product.Click();
                Thread.Sleep(2000);

                IWebElement productTypeElement = wait.Until(d => d.FindElement(By.CssSelector("dl.thydo dd.tensp")));
                string productType = productTypeElement.Text.Trim().ToLower();
                Console.WriteLine($"Loại sản phẩm: {productType}");

                if (!productType.Contains("bánh") && !productType.Contains("buổi"))
                {
                    SelectOption("input[name='idsize']", size);
                    Thread.Sleep(2000);
                    SelectOption("input[name='idda']", da);
                    Thread.Sleep(2000);
                    SelectOption("input[name='idduong']", duong);
                    Thread.Sleep(2000);

                    ((IJavaScriptExecutor)driver9).ExecuteScript("window.scrollTo(0, 280);");

                    if (productType.Contains("trà") && !string.IsNullOrEmpty(topping))
                    {
                        IWebElement chontopping = wait.Until(d => d.FindElement(By.CssSelector("div.hihi")));
                        ((IJavaScriptExecutor)driver9).ExecuteScript("arguments[0].click();", chontopping);
                        Thread.Sleep(2000);
                    }
                }

                Thread.Sleep(3000);

                IWebElement addToCartButton = wait.Until(d => d.FindElement(By.CssSelector("div.button-group button")));
                addToCartButton.Click();

                Console.WriteLine("Đã thêm sản phẩm vào giỏ hàng.");
                Thread.Sleep(5000);

            }
            catch (Exception ex)
            {
                Console.WriteLine("thất bại: " + ex.Message);
                Assert.Fail("thất bại: " + ex.Message);
            }

            Console.WriteLine("TEST BUTTON +");
            logs.Add(new string[] { "TEST BUTTON +" });

            //test button +
            IWebElement cart = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"actions\"]/div[1]/a/img")));
            cart.Click();

            Thread.Sleep(2000);
            IWebElement checkname = wait.Until(d => d.FindElement(By.CssSelector("div.nd_sp span a")));
            string productNameInCart = checkname.Text.Trim();
            Assert.AreEqual(tensp, productNameInCart, "    Sản phẩm trong giỏ hàng không giong");

            Console.WriteLine($"    '{productNameInCart}'có trong giỏ hàng.");

            Thread.Sleep(2000);

            IWebElement input_quantity = wait.Until(d => d.FindElement(By.Name("quantity")));
            string initialQuantity = input_quantity.GetAttribute("value");
            Console.WriteLine($"    Số lượng luc đầu: {initialQuantity}");
            logs.Add(new string[] { $"    Số lượng lúc đầu: {initialQuantity}" });


            IWebElement btnn_cong = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/table/tbody[2]/tr/td[3]/div/a[2]")));
            btnn_cong.Click();

            Thread.Sleep(2000);

            input_quantity = wait.Until(d => d.FindElement(By.Name("quantity")));

            string updatedQuantity = input_quantity.GetAttribute("value");
            Console.WriteLine($"    Số lượng sau khi nhấn '+': {updatedQuantity}");
            logs.Add(new string[] { $"    Số lượng sau khi nhấn '+': {updatedQuantity}" });


            Assert.AreNotEqual(initialQuantity, updatedQuantity, "     Số lượng sản phẩm không tăng.");
            Console.WriteLine("    Số lượng sản phẩm đã tăng thành công.");
            logs.Add(new string[] { "    Số lượng sản phẩm đã tăng thành công." });


            Console.WriteLine("TEST GIA CO TANG K");
            logs.Add(new string[] { "TEST GIA CO TANG K" });

            //Test gia co tang khong
            Thread.Sleep(1000);
            IWebElement check_price = wait.Until(d => d.FindElement(By.CssSelector("div.tongtien span.total-amount")));
            string ktragia = check_price.Text.Trim();
            ktragia = Regex.Replace(ktragia, @"\D", "");
            int tonggia = int.Parse(ktragia);

            Console.WriteLine($"    Gia ban dau: {tonggia}");
            logs.Add(new string[] { $"    Giá ban đầu: {tonggia}" });
            IWebElement btnn_cong1 = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/table/tbody[2]/tr/td[3]/div/a[2]")));
            btnn_cong1.Click();
            Thread.Sleep(2000);

            check_price = wait.Until(d => d.FindElement(By.CssSelector("div.tongtien span.total-amount")));
            string upktragia = check_price.Text.Trim();
            upktragia = Regex.Replace(upktragia, @"\D", "");
            int uptonggia = int.Parse(upktragia);

            Console.WriteLine($"    Giá tiền sau khi tăng: {uptonggia}");
            logs.Add(new string[] { $"    Giá tiền sau khi tăng: {uptonggia}" });

            Assert.Greater(uptonggia, tonggia, "    Giá tiền không tăng khi bam'+'.");

            Console.WriteLine("TEST BUTTON -");
            logs.Add(new string[] { "TEST BUTTON -" });
            //Test button -
            IWebElement input_quantity_tru = wait.Until(d => d.FindElement(By.Name("quantity")));
            string tongsl_tru = input_quantity_tru.GetAttribute("value");
            Console.WriteLine($"    Số lượng luc đầu: {tongsl_tru}");
            logs.Add(new string[] { $"    Số lượng lúc đầu: {tongsl_tru}" });

            IWebElement btnn_tru = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/table/tbody[2]/tr/td[3]/div/a[1]")));
            btnn_tru.Click();

            Thread.Sleep(2000);

            input_quantity_tru = wait.Until(d => d.FindElement(By.Name("quantity")));

            string update_sl_tru = input_quantity_tru.GetAttribute("value");
            Console.WriteLine($"    Số lượng sau khi nhấn '-': {update_sl_tru}");
            logs.Add(new string[] { $"    Số lượng sau khi nhấn '-': {update_sl_tru}" });

            Assert.AreNotEqual(tongsl_tru, update_sl_tru, "    Số lượng sản phẩm không giam.");
            Console.WriteLine("    Số lượng sản phẩm đã giam thành công.");
            logs.Add(new string[] { "    Số lượng sản phẩm đã giảm thành công." });

            // Test gia giam
            Console.WriteLine("TEST COI GIA CO GIAM KHONG");
            logs.Add(new string[] { "TEST COI GIA CO GIAM KHONG" });

            Thread.Sleep(1000);
            IWebElement check_priceT = wait.Until(d => d.FindElement(By.CssSelector("div.tongtien span.total-amount")));
            string ktragiaT = check_priceT.Text.Trim();
            ktragiaT = Regex.Replace(ktragiaT, @"\D", "");
            int tonggiaT = int.Parse(ktragiaT);

            Console.WriteLine($"    Giá ban đầu: {tonggiaT}");
            logs.Add(new string[] { $"    Giá ban đầu: {tonggiaT}" });

            IWebElement btn_tru = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/table/tbody[2]/tr/td[3]/div/a[1]")));
            btn_tru.Click();
            Thread.Sleep(2000);

            check_priceT = wait.Until(d => d.FindElement(By.CssSelector("div.tongtien span.total-amount")));
            string upktragiaT = check_priceT.Text.Trim();
            upktragiaT = Regex.Replace(upktragiaT, @"\D", "");
            int uptonggiaT = int.Parse(upktragiaT);

            Console.WriteLine($"    Giá tiền sau khi giảm: {uptonggiaT}");
            logs.Add(new string[] { $"    Giá tiền sau khi giảm: {uptonggiaT}" });

            Assert.Less(uptonggiaT, tonggiaT, "    Giá tiền không giảm khi bam '-'.");

            //GIAM VE 0

            Console.WriteLine("GIAN SO LUONG VE 0");
            logs.Add(new string[] { "GIAN SO LUONG VE 0" });

            IWebElement tru = wait.Until(d => d.FindElement(By.Name("quantity")));
            string tsl_tru = tru.GetAttribute("value");
            Console.WriteLine($"    Số lượng luc đầu: {tsl_tru}");
            logs.Add(new string[] { $"    Số lượng lúc đầu: {tsl_tru}" });

            IWebElement btn_tru1 = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/table/tbody[2]/tr/td[3]/div/a[1]")));
            btn_tru1.Click();

            Thread.Sleep(2000);
            IWebElement text_rong = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/table/tbody[2]/tr/td")));
            Assert.IsTrue(text_rong.Displayed);
            Console.WriteLine("    Gio hang rong");
            logs.Add(new string[] { "    Giỏ hàng rỗng" });


            logs.Add(new string[] { "Đã thêm sản phẩm vào giỏ hàng." });
            logs.Add(new string[] { $"    '{productNameInCart}' có trong giỏ hàng." });
            //logs.Add(new string[] { "TEST BUTTON +" });
            //logs.Add(new string[] { $"    Số lượng lúc đầu: {initialQuantity}" });
            //logs.Add(new string[] { $"    Số lượng sau khi nhấn '+': {updatedQuantity}" });
            //logs.Add(new string[] { "    Số lượng sản phẩm đã tăng thành công." });
            //logs.Add(new string[] { "TEST GIA CO TANG K" });
            //logs.Add(new string[] { $"    Giá ban đầu: {tonggia}" });
            //logs.Add(new string[] { $"    Giá tiền sau khi tăng: {uptonggia}" });
            //logs.Add(new string[] { "TEST BUTTON -" });
            //logs.Add(new string[] { $"    Số lượng lúc đầu: {tongsl_tru}" });
            //logs.Add(new string[] { $"    Số lượng sau khi nhấn '-': {update_sl_tru}" });
            //logs.Add(new string[] { "    Số lượng sản phẩm đã giảm thành công." });
            //logs.Add(new string[] { "TEST COI GIA CO GIAM KHONG" });
            //logs.Add(new string[] { $"    Giá ban đầu: {tonggiaT}" });
            //logs.Add(new string[] { $"    Giá tiền sau khi giảm: {uptonggiaT}" });
            //logs.Add(new string[] { "GIAN SO LUONG VE 0" });
            //logs.Add(new string[] { $"    Số lượng lúc đầu: {tsl_tru}" });
            //logs.Add(new string[] { "    Giỏ hàng rỗng" });
            ExportToExcel(logs, "CheckCart");


        }
        //private List<string[]> logs = new List<string[]>();


        private void SelectOption(string selector, string value)
        {
            WebDriverWait wait = new WebDriverWait(driver9, TimeSpan.FromSeconds(5));
            try
            {
                IWebElement element = wait.Until(d => d.FindElement(By.CssSelector(selector)));
                ((IJavaScriptExecutor)driver9).ExecuteScript("arguments[0].click();", element);
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"Không tìm thấy: {value}");
            }
        }
        private void ExportToExcel(List<string[]> data, string sheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo fileInfo = new FileInfo(filePath);

            using (var package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetName] ?? package.Workbook.Worksheets.Add(sheetName);

                int startRow = worksheet.Dimension?.Rows + -1 ?? -1;

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[startRow + i, 6].Value = data[i][0];
                }

                package.Save();
                Console.WriteLine("Xuất dữ liệu ra Excel thành công!");
            }
        }

        //private void TrongExcel(string tensp, string result)
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (var package = new ExcelPackage(new FileInfo(filePath)))
        //    {
        //        var worksheet = package.Workbook.Worksheets["CheckCart"];
        //        int rowCount = worksheet.Dimension.Rows;

        //        for (int i = 2; i <= rowCount; i++)
        //        {
        //            if (worksheet.Cells[i, 1].Text == tensp)
        //            {
        //                worksheet.Cells[i, 6].Value = result;
        //                package.Save();
        //                break;
        //            }
        //        }
        //    }
        //}

        [TearDown]
        public void TearDown()
        {
            driver9.Quit();
            driver9.Dispose();
        }
    }
}
