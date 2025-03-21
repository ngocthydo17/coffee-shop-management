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



namespace TestLogIn
{
    [TestFixture]
    public class TestThemSP
    {
        private static IWebDriver driver7;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void SetUp()
        {
            driver7 = new EdgeDriver();
            driver7.Manage().Window.Maximize();
            driver7.Navigate().GoToUrl("https://localhost:44317/");
        }

        public static IEnumerable<TestCaseData> Test_addsp()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {

                var worksheet = package.Workbook.Worksheets["ThemSP"];
                int rowCount = worksheet.Dimension.Rows;

                string ten, gia, dvt, mota, url, loai;

                for (int i = 2; i <= rowCount; i++)
                {
                    ten = worksheet.Cells[i, 1].Text;
                    gia = worksheet.Cells[i, 2].Text;
                    dvt = worksheet.Cells[i, 3].Text;
                    mota = worksheet.Cells[i, 4].Text;
                    url = worksheet.Cells[i, 5].Text;
                    loai = worksheet.Cells[i, 6].Text;


                    yield return new TestCaseData(ten, gia, dvt, mota, url, loai);
                }
            }
        }

        [Test, TestCaseSource(nameof(Test_addsp))]
        public void ThemSanPham(string ten, string gia, string dvt, string mota, string url, string loai)
        {
            WebDriverWait wait = new WebDriverWait(driver7, TimeSpan.FromSeconds(20));

            IWebElement showhome = wait.Until(d => d.FindElement(By.CssSelector("div.button-container")));
            Assert.IsTrue(showhome.Displayed);

            Thread.Sleep(1000);

            IWebElement click_dn = wait.Until(d => d.FindElement(By.ClassName("button_dn")));
            click_dn.Click();

            Thread.Sleep(2000);

            IWebElement click_admin = wait.Until(d => d.FindElement(By.CssSelector("div.login_admin a")));
            click_admin.Click();

            Thread.Sleep(1000);

            IWebElement tk_admin = wait.Until(d => d.FindElement(By.Name("sdt")));
            tk_admin.SendKeys("987654321");

            Thread.Sleep(1000);

            IWebElement mk_admin = wait.Until(d => d.FindElement(By.Name("password")));
            mk_admin.SendKeys("123456");

            IWebElement btn_admin = wait.Until(d => d.FindElement(By.ClassName("login-btn")));
            btn_admin.Click();

            IWebElement home_admin = wait.Until(d => d.FindElement(By.CssSelector("div.title")));
            Assert.IsTrue(home_admin.Displayed);

            Thread.Sleep(1000);
            IWebElement nav_admin = wait.Until(d => d.FindElement(By.CssSelector("label.open")));
            nav_admin.Click();

            Thread.Sleep(1000);
            IWebElement sp_admin = wait.Until(d => d.FindElement(By.XPath("/html/body/div[1]/nav/div/ul/li[3]/a")));
            sp_admin.Click();

            Thread.Sleep(1000);
            IWebElement add_sp = wait.Until(d => d.FindElement(By.CssSelector("div.addsp a.khokho")));
            add_sp.Click();

            IWebElement add_tensp = wait.Until(d => d.FindElement(By.Name("ten")));
            add_tensp.SendKeys(ten);

            IWebElement add_giasp = wait.Until(d => d.FindElement(By.Name("dongia")));
            add_giasp.SendKeys(gia);

            IWebElement add_dvtsp = wait.Until(d => d.FindElement(By.Name("dvt")));
            add_dvtsp.SendKeys(dvt);

            IWebElement add_motasp = wait.Until(d => d.FindElement(By.Name("mota")));
            add_motasp.SendKeys(mota);

            IWebElement add_urlsp = wait.Until(d => d.FindElement(By.Name("anh")));
            add_urlsp.SendKeys(url);

            ((IJavaScriptExecutor)driver7).ExecuteScript("window.scrollTo(0, 200);");
            Thread.Sleep(3000);

            IWebElement add_trangthai = wait.Until(d => d.FindElement(By.Name("trangThai")));
            if (add_trangthai.Displayed && add_trangthai.Enabled)
            {
                add_trangthai.Click();
            }

            IWebElement add_loai = wait.Until(d => d.FindElement(By.Name("maloai")));
            SelectElement dropdown = new SelectElement(add_loai);
            dropdown.SelectByText(loai);

            Thread.Sleep(1000);

            IWebElement btn_themsanpham = wait.Until(d => d.FindElement(By.ClassName("them-sp")));
            btn_themsanpham.Click();

            IWebElement lepetit = wait.Until(d => d.FindElement(By.CssSelector("div.about h2")));
            ((IJavaScriptExecutor)driver7).ExecuteScript("arguments[0].scrollIntoView(true);", lepetit);
            Thread.Sleep(3000);

            //string expectedProductName = ten.Trim();

            //var products = wait.Until(d => d.FindElements(By.XPath("//td[contains(@class, 'sanpham')]")));

            //bool found = false;

            //foreach (var product in products)
            //{
            //        string productName = product.Text.Trim();
            //        //Console.WriteLine($"Sản phẩm trong danh sách: '{productName}'");

            //        if (productName.Equals(expectedProductName, StringComparison.OrdinalIgnoreCase))
            //        {
            //            found = true;
            //            Console.WriteLine("Tên sản phẩm hiển thị đúng.");
            //            break;
            //        }
            //}

            //if (!found)
            //{
            //     Console.WriteLine("COOOK - Không tìm thấy sản phẩm đúng.");
            //}

            Thread.Sleep(3000);

            ((IJavaScriptExecutor)driver7).ExecuteScript("window.scrollTo(0, 0);");

            bool themtc = false; 

            try
            {
                string expectedProductName = ten.Trim();

                var products = wait.Until(d => d.FindElements(By.XPath("//td[contains(@class, 'sanpham')]")));

                foreach (var product in products)
                {
                    string productName = product.Text.Trim();
                    if (productName.Equals(expectedProductName, StringComparison.OrdinalIgnoreCase))
                    {
                        themtc = true;
                        Console.WriteLine("Tên sản phẩm hiển thị đúng.");
                        break;
                    }
                }

                if (!themtc)
                {
                    Console.WriteLine("COOOK - Không tìm thấy sản phẩm đúng.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                themtc = false; 
            }


            TrongExcel(ten, gia, dvt, mota, url, loai, themtc ? "Pass" : "Fail");
            Assert.IsTrue(themtc);
            Thread.Sleep(4000);
        }

        private void TrongExcel(string ten, string gia, string dvt, string mota, string url, string loai, string result)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["ThemSP"];
                int rowCount = worksheet.Dimension.Rows;

                string crten, crgia, crdvt, crmota, crurl, crloai;

                bool found = false;

                for (int i = 2; i <= rowCount; i++)
                {
                    crten = worksheet.Cells[i, 1].Text;
                    crgia = worksheet.Cells[i, 2].Text;
                    crdvt = worksheet.Cells[i, 3].Text;
                    crmota = worksheet.Cells[i, 4].Text;
                    crurl = worksheet.Cells[i, 5].Text;
                    crloai = worksheet.Cells[i, 6].Text;

                    if (crten == ten && crgia == gia && crdvt == dvt && crmota == mota && crurl == url && crloai == loai)
                    {
                        worksheet.Cells[i, 7].Value = result;
                        found = true;
                        package.Save();
                        break;
                    }
                }

                if (!found)
                {
                    Console.WriteLine($"Không tìm thấy dữ liệu trong Excel: {ten}");
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver7.Quit();
            driver7.Dispose();
        }
    }
}
