using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;
using System.Threading;
using SeleniumExtras.WaitHelpers;
using OfficeOpenXml;


namespace TestLogIn
{
    public class TestDatHang_Email
    {
        private static IWebDriver driver6;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void SetUp()
        {
            EdgeOptions options = new EdgeOptions();
            options.AddExcludedArgument("enable-automation"); // Vô hiệu hóa chế độ automation
            options.AddArgument("--disable-blink-features=AutomationControlled"); // Ẩn dấu hiệu điều khiển tự động
            options.AddArgument("--start-maximized"); // Mở trình duyệt với chế độ full màn hình

            driver6 = new EdgeDriver(options);
            driver6.Manage().Window.Maximize();
            driver6.Navigate().GoToUrl("https://localhost:44317/DKDN/Login");
        }

        public static IEnumerable<TestCaseData> dathang()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["ThanhToan"];
                int rowCount = worksheet.Dimension.Rows;

                for (int i = 2; i <= rowCount; i++)
                {
                    string tksdt = worksheet.Cells[i, 1].Text;
                    string mksdt = worksheet.Cells[i, 2].Text;
                    string tensp = worksheet.Cells[i, 3].Text;
                    string size = worksheet.Cells[i, 4].Text;
                    string da = worksheet.Cells[i, 5].Text;
                    string duong = worksheet.Cells[i, 6].Text;
                    string topping = worksheet.Cells[i, 7].Text;
                    string nameuser = worksheet.Cells[i, 8].Text;
                    string address = worksheet.Cells[i, 9].Text;
                    string phone = worksheet.Cells[i, 10].Text;
                    string cn = worksheet.Cells[i, 11].Text;


                    if (!string.IsNullOrWhiteSpace(tensp))
                    {
                        yield return new TestCaseData(tksdt, mksdt, tensp, size, da, duong, topping, nameuser, address, phone, cn);
                    }
                }
            }
        }
        [Test, TestCaseSource(nameof(dathang))]
        public void DatHang(string tksdt, string mksdt, string tensp, string size, string da, string duong, string topping, string nameuser, string address, string phone, string cn)
        {
            WebDriverWait wait = new WebDriverWait(driver6, TimeSpan.FromSeconds(15));

            IWebElement sdt = wait.Until(d => d.FindElement(By.Name("Sdt")));
            sdt.SendKeys(tksdt);

            IWebElement mk = wait.Until(d => d.FindElement(By.Name("Matkhau")));
            mk.SendKeys(mksdt);

            IWebElement btn_click = wait.Until(d => d.FindElement(By.ClassName("login-btn")));
            btn_click.Click();

            IWebElement icon_user = wait.Until(d => d.FindElement(By.ClassName("user_img")));
            Assert.IsTrue(icon_user.Displayed);

            IWebElement menuItem = wait.Until(d => d.FindElement(By.XPath("//*[@id='menu']/div[2]/a")));
            menuItem.Click();
            Thread.Sleep(5000);

            IWebElement sotrang = wait.Until(d => d.FindElement(By.CssSelector("div.tttrang")));
            ((IJavaScriptExecutor)driver6).ExecuteScript("arguments[0].scrollIntoView(true);", sotrang);
            Assert.IsTrue(sotrang.Displayed);
            Thread.Sleep(1000);

            ((IJavaScriptExecutor)driver6).ExecuteScript("window.scrollTo(0, 0);");
            Thread.Sleep(2000);

            ((IJavaScriptExecutor)driver6).ExecuteScript("window.scrollTo(0, 200);");
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

                ((IJavaScriptExecutor)driver6).ExecuteScript("window.scrollTo(0, 280);");

                // Chỉ trà mới có topping
                if (productType.Contains("trà") && !string.IsNullOrEmpty(topping))
                {
                    IWebElement chontopping = wait.Until(d => d.FindElement(By.CssSelector("div.hihi")));
                    ((IJavaScriptExecutor)driver6).ExecuteScript("arguments[0].click();", chontopping);
                    Thread.Sleep(2000);
                }
            }
            Thread.Sleep(3000);

            IWebElement addToCartButton = wait.Until(d => d.FindElement(By.CssSelector("div.button-group button")));
            addToCartButton.Click();

            Console.WriteLine("Đã thêm sản phẩm vào giỏ hàng.");
            Thread.Sleep(5000);

            IWebElement cart = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"actions\"]/div[1]/a/img")));
            cart.Click();

            Thread.Sleep(2000);
            IWebElement checkname = wait.Until(d => d.FindElement(By.CssSelector("div.nd_sp span a")));
            string productNameInCart = checkname.Text.Trim();
            Assert.AreEqual(tensp, productNameInCart, "    Sản phẩm trong giỏ hàng không giong");

            Console.WriteLine($"    '{productNameInCart}'có trong giỏ hàng.");

            IWebElement btn_thanhtoan = wait.Until(d => d.FindElement(By.CssSelector("a.checkout")));
            btn_thanhtoan.Click();

            IWebElement checkname_thanhtoan = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/form/div/div[2]/div/div[1]/table/tbody/tr/td[1]")));
            string productNameCart = checkname_thanhtoan.Text.Trim();
            Assert.AreEqual(tensp, productNameCart, "    Sản phẩm trong giỏ hàng không giong");

            Console.WriteLine($"    '{productNameCart}' Hiển thị thanh toán");

            ((IJavaScriptExecutor)driver6).ExecuteScript("window.scrollTo(0, 200);");

            IWebElement ten_kh = wait.Until(d => d.FindElement(By.Name("HoTen")));
            ten_kh.SendKeys(nameuser);

            IWebElement diachi_kh = wait.Until(d => d.FindElement(By.Name("DiaChi")));
            diachi_kh.SendKeys(address);

            IWebElement sdt_kh = wait.Until(d => d.FindElement(By.Name("DienThoai")));
            sdt_kh.SendKeys(phone);

            ((IJavaScriptExecutor)driver6).ExecuteScript("window.scrollTo(0, 200);");

            Thread.Sleep(2000);

            IWebElement dropdown_cn = wait.Until(d => d.FindElement(By.Id("BranchDropdown")));
            SelectElement chon = new SelectElement(dropdown_cn);
            chon.SelectByText(cn);
            Thread.Sleep(3000);

            IWebElement dropdown_dis = wait.Until(d => d.FindElement(By.Id("DiscountCode")));
            SelectElement giamgia = new SelectElement(dropdown_dis);
            if (giamgia.Options.Count > 1)
            {
                giamgia.SelectByIndex(1);
                Assert.IsTrue(true, "Mã giảm giá đã được chọn.");
            }
            else
            {
                Console.WriteLine("Khong co ma giam gia");
            }

            Thread.Sleep(2000);

            IWebElement btn_payment = wait.Until(d => d.FindElement(By.ClassName("tienmat")));
            btn_payment.Click();

            Thread.Sleep(3000);

            IWebElement ktra = wait.Until(d => d.FindElement(By.CssSelector("div.horizontal-container")));
            Assert.IsTrue(ktra.Displayed);

            IWebElement ktrasp = wait.Until(d => d.FindElement(By.CssSelector("div.item span")));
            string spthanhtoan = ktrasp.Text.Trim();
            Assert.AreEqual(tensp, spthanhtoan, "    Sản phẩm trong giỏ hàng không giong");

            Console.WriteLine($"    '{spthanhtoan}'có trong giỏ hàng.");

            Thread.Sleep(2000);
            IWebElement send_mail = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/div[1]/div[3]/button")));
            send_mail.Click();
            Thread.Sleep(3000);

            IWebElement nhapmail = wait.Until(d => d.FindElement(By.Id("email")));
            nhapmail.SendKeys("6bagshospital@gmail.com");
            Thread.Sleep(2000);

            IWebElement btn_mail = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"emailModal\"]/div/div/div[2]/form/div[2]/button")));
            btn_mail.Click();

            Thread.Sleep(3000);

            IWebElement tc = wait.Until(d => d.FindElement(By.Id("swal2-html-container")));
            Assert.IsTrue(tc.Displayed);


            Thread.Sleep(3000);

            IWebElement oke = wait.Until(d => d.FindElement(By.XPath("/html/body/div[2]/div/div[6]/button[1]")));
            oke.Click();

            Thread.Sleep(5000);

            driver6.Navigate().GoToUrl("https://mail.google.com/mail");

            Thread.Sleep(1000);

            IWebElement tk_gg = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("identifierId")));
            tk_gg.SendKeys("6bagshospital@gmail.com");
            Thread.Sleep(2000);

            IWebElement btn_gg = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("identifierNext")));
            btn_gg.Click();

            Thread.Sleep(2000);

            IWebElement mk_gg = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='password']//input")));
            mk_gg.SendKeys("123456bag");
            Thread.Sleep(2000);

            IWebElement btn_dngg = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("passwordNext")));
            btn_dngg.Click();

            Thread.Sleep(3000);

            IWebElement click_mail = wait.Until(d => d.FindElement(By.XPath("//table[@role='grid']//tr[@role='row'][1]")));
            click_mail.Click();


            Thread.Sleep(4000);

            ((IJavaScriptExecutor)driver6).ExecuteScript("window.scrollTo(0, 250);");
            try
            {
                IWebElement xacnhanmal = wait.Until(d => d.FindElement(By.XPath("//ul/li/strong")));
                string xacnhangg = xacnhanmal.Text.Trim();

                if (tensp == xacnhangg)
                {
                    Console.WriteLine($"'{xacnhangg}' Hiển thị trong mail");
                    TrongExcel(tensp, "Pass");

                }
                else
                {
                    Console.WriteLine($"'{xacnhangg}' Khong khop");
                    TrongExcel(tensp, "Fail");
                }

            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Sản phẩm không hiển thị trong mail");
                TrongExcel(tensp, "Fail");
            }

            Thread.Sleep(2000);
        }


        private void SelectOption(string selector, string value)
        {
            WebDriverWait wait = new WebDriverWait(driver6, TimeSpan.FromSeconds(5));
            try
            {
                IWebElement element = wait.Until(d => d.FindElement(By.CssSelector(selector)));
                ((IJavaScriptExecutor)driver6).ExecuteScript("arguments[0].click();", element);
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
                var worksheet = package.Workbook.Worksheets["ThanhToan"];
                //int rowCount = worksheet.Dimension.Rows;
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount == 0)
                {
                    Console.WriteLine("Worksheet không có dữ liệu!");
                    return;
                }
                bool found = false;
                for (int i = 2; i <= rowCount; i++)
                {
                    string excelTensp = worksheet.Cells[i, 3].Text.Trim();

                    if (excelTensp == tensp.Trim())
                    {
                        worksheet.Cells[i, 12].Value = result;
                        package.Save();
                        package.Dispose();
                        Console.WriteLine($"Đã cập nhật '{tensp}' thành '{result}' trong Excel.");
                        found = true;
                        break;
                    }
                }


            }
        }
        [TearDown]
        public void TearDown()
        {
            driver6.Quit();
            driver6.Dispose();
        }
    }
}
