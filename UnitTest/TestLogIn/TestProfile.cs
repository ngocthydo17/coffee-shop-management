using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using OpenQA.Selenium.Edge;
using excel = Microsoft.Office.Interop.Excel;

namespace TestLogIn
{
    [TestFixture]
    public class TestProfile
    {
        private static IWebDriver driver5;
        private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void Setup()
        {
            driver5 = new EdgeDriver();
            driver5.Manage().Window.Maximize();
            driver5.Navigate().GoToUrl("https://localhost:44317/");

            WebDriverWait wait = new WebDriverWait(driver5, TimeSpan.FromSeconds(10));

            IWebElement loginButton = wait.Until(d => d.FindElement(By.ClassName("button_dn")));
            Assert.IsTrue(loginButton.Displayed, "Trang chủ không có nút Đăng nhập");

            IWebElement btn_login = wait.Until(d => d.FindElement(By.ClassName("button_dn")));
            btn_login.Click();

            IWebElement thy = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"Sdt\"]")));
            thy.SendKeys("0906483257");

            IWebElement doa = driver5.FindElement(By.XPath("//*[@id=\"Matkhau\"]"));
            doa.SendKeys("Thy01072004");

            IWebElement btn_click_pass = driver5.FindElement(By.XPath("//*[@id=\"wrapped\"]/div/div/div[2]/form/button"));
            btn_click_pass.Click();
        }

        public static IEnumerable<TestCaseData> Test_User()
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open(filePath);
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Worksheets[6];
            excel.Range range = wsheet.UsedRange;

            string user, address, email, gender;

            for (int i = 2; i <= range.Rows.Count; i++)
            {
                user = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                address = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";
                email = Convert.ToString((range.Cells[i, 3] as excel.Range)?.Value) ?? "";
                gender = Convert.ToString((range.Cells[i, 4] as excel.Range)?.Value) ?? "";

                yield return new TestCaseData(user, address, email, gender);
            }
            wbook.Close(false);
            app.Quit();
            Marshal.ReleaseComObject(wsheet);
            Marshal.ReleaseComObject(wbook);
            Marshal.ReleaseComObject(app);
        }

        [Test, TestCaseSource(nameof(Test_User))]
        public void Test_Profile(string user, string address, string email, string gender)
        {
            WebDriverWait wait = new WebDriverWait(driver5, TimeSpan.FromSeconds(20));

            driver5.FindElement(By.ClassName("user_img")).Click();
            driver5.FindElement(By.XPath("//*[@id=\"actions\"]/div[2]/div[2]/ul/li[1]")).Click();
            driver5.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/div/div[4]/a[2]")).Click();

            IWebElement hotenc = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"Ten\"]")));
            hotenc.Clear();
            hotenc.SendKeys(user);

            IWebElement diachi = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"Diachi\"]")));
            diachi.Clear();
            diachi.SendKeys(address);

            IWebElement meo = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"Email\"]")));
            meo.Clear();
            meo.SendKeys(email);

            IWebElement genderElement = driver5.FindElement(By.XPath(gender == "Nam" ? "//*[@id=\"Nam\"]" : "//*[@id=\"Nu\"]"));
            if (!genderElement.Selected)
            {
                genderElement.Click();
            }

            driver5.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/div/div[2]/form/button")).Click();

            bool luutc;
            try
            {
                IWebElement welcomeMessage = wait.Until(d =>
                {
                    var elements = d.FindElements(By.ClassName("btn_card2"));
                    return elements.Count > 0 ? elements[0] : null;
                });

                luutc = welcomeMessage != null && welcomeMessage.Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                luutc = false;
            }

            Console.WriteLine($"Test Result for {user}: {(luutc ? "Pass" : "Fail")}");
            TrongExcel(user, address, email, gender, luutc ? "Pass" : "Fail");

            Assert.IsTrue(luutc, $"Lưu thông tin thất bại cho {user}");
        }

        private void TrongExcel(string user, string address, string email, string gender, string result)
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open(filePath);
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Worksheets[6];
            excel.Range range = wsheet.UsedRange;

            bool found = false;

            for (int i = 2; i <= range.Rows.Count; i++)
            {
                string cruser = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                string craddress = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";
                string cremail = Convert.ToString((range.Cells[i, 3] as excel.Range)?.Value) ?? "";
                string crgender = Convert.ToString((range.Cells[i, 4] as excel.Range)?.Value) ?? "";

                if (cruser == user && craddress == address && cremail == email && crgender == gender)
                {
                    wsheet.Cells[i, 5] = result;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine($"Không tìm thấy dữ liệu trong Excel: {user}, {address}, {email}, {gender}");
            }

            app.DisplayAlerts = false;
            wbook.Save();
            app.DisplayAlerts = true;

            wbook.Close(false);
            app.Quit();

            Marshal.ReleaseComObject(wsheet);
            Marshal.ReleaseComObject(wbook);
            Marshal.ReleaseComObject(app);
        }

        [TearDown]
        public void TearDown()
        {
            driver5.Quit();
            driver5.Dispose();
        }
    }
}
