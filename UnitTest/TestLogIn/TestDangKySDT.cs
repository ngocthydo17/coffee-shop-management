using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using ClosedXML.Excel;
using excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace TestLogIn
{
    [TestFixture]
    public class TestDangKySDT
    {
        private IWebDriver driver3;
        private string excelFilePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";

        [SetUp]
        public void Setup()
        {
            driver3 = new EdgeDriver();
            driver3.Manage().Window.Maximize();
            WebDriverWait wait = new WebDriverWait(driver3, TimeSpan.FromSeconds(10));



        }

        public static IEnumerable<TestCaseData> Test_DK_SDT()
        {
            excel.Application xapp = new excel.Application();
            excel.Workbook xbook = xapp.Workbooks.Open("F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx");
            excel.Worksheet worksheet = (excel.Worksheet)xbook.Sheets[4];
            excel.Range xrange = worksheet.UsedRange;

            string name, phone, pass, confirmpass;
            for (int i = 2; i <= xrange.Rows.Count; i++)
            {
                name = Convert.ToString((xrange.Cells[i, 1] as excel.Range)?.Value)?.Trim() ?? "";
                phone = Convert.ToString((xrange.Cells[i, 2] as excel.Range)?.Value)?.Trim() ?? "";
                pass = Convert.ToString((xrange.Cells[i, 3] as excel.Range)?.Value)?.Trim() ?? "";
                confirmpass = Convert.ToString((xrange.Cells[i, 4] as excel.Range)?.Value)?.Trim() ?? "";

                yield return new TestCaseData(name, phone, pass, confirmpass);
            }

            xbook.Close(false);
            xapp.Quit();
            Marshal.ReleaseComObject(xbook);
            Marshal.ReleaseComObject(xapp);
        }

        [Test, TestCaseSource(nameof(Test_DK_SDT))]
        public void Test_Register_And_Login(string name, string phone, string pass, string confirmpass)
        {
            WebDriverWait wait = new WebDriverWait(driver3, TimeSpan.FromSeconds(15));

            driver3.Navigate().GoToUrl("https://localhost:44317/");

            IWebElement loginButton = wait.Until(d => d.FindElement(By.ClassName("button_dn")));

            Assert.IsTrue(loginButton.Displayed, "Trang chủ không có nút Đăng nhập");

            IWebElement click_dkmeo = wait.Until(d => d.FindElement(By.ClassName("button_dk")));
            click_dkmeo.Click(); 

            IWebElement ten = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"Ten\"]")));
            ten.SendKeys(name);
            driver3.FindElement(By.XPath("//*[@id=\"Sdt\"]")).SendKeys(phone);
            driver3.FindElement(By.XPath("//*[@id=\"Matkhau\"]")).SendKeys(pass);
            driver3.FindElement(By.XPath("//*[@id=\"MatkhauNhapLai\"]")).SendKeys(confirmpass);
            Thread.Sleep(1000);
            driver3.FindElement(By.ClassName("dk-btn")).Click();
            Thread.Sleep(1000);

            bool registerSuccess;
            try
            {
                IWebElement welcomeMessage = wait.Until(d =>
                {
                    var elements = d.FindElements(By.XPath("//*[@id=\"wrapped\"]/div/div/div[2]/h2"));
                    return elements.Count > 0 ? elements[0] : null;
                });
                registerSuccess = welcomeMessage != null && welcomeMessage.Displayed;
            }
            catch
            {
                registerSuccess = false;
            }

            bool hasErrorMessage = driver3.FindElements(By.ClassName("error-message")).Count > 0;
            if (hasErrorMessage)
            {
                registerSuccess = false;
            }

            Console.WriteLine($"Register result for {name}: {(registerSuccess ? "Pass" : "Fail")}");

            bool loginSuccess = false;
            if (registerSuccess)
            {
                driver3.Navigate().GoToUrl("https://localhost:44317/DKDN/Login");
                wait = new WebDriverWait(driver3, TimeSpan.FromSeconds(10));
                driver3.FindElement(By.Name("Sdt")).SendKeys(phone);
                driver3.FindElement(By.Name("Matkhau")).SendKeys(pass);
                driver3.FindElement(By.ClassName("login-btn")).Click();

                try
                {
                    IWebElement loginMessage = wait.Until(d =>
                    {
                        var elements = d.FindElements(By.ClassName("user_img"));
                        return elements.Count > 0 ? elements[0] : null;
                    });
                    loginSuccess = loginMessage != null && loginMessage.Displayed;
                }
                catch
                {
                    loginSuccess = false;
                }
            }

            Console.WriteLine($"Login result for {phone}: {(loginSuccess ? "Pass" : "Fail")}");

            UpdateExcelResults(name, phone, registerSuccess, loginSuccess);

            if (!registerSuccess)
            {
                Assert.Fail($"Registration failed for {name}");
            }

            if (!loginSuccess)
            {
                Assert.Fail($"Login failed for {phone}");
            }
        }

        private void UpdateExcelResults(string name, string phone, bool registerSuccess, bool loginSuccess)
        {
            using (var workbook = new XLWorkbook(excelFilePath))
            {
                var worksheet = workbook.Worksheet(4);

                if (worksheet.Cell(1, 5).IsEmpty())
                {
                    worksheet.Cell(1, 5).Value = "Register Success";
                    worksheet.Cell(1, 6).Value = "Login Success";
                }

                int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                for (int row = 2; row <= lastRow; row++)
                {
                    string cellName = worksheet.Cell(row, 1).GetString().Trim();
                    string cellPhone = worksheet.Cell(row, 2).GetString().Trim();

                    if (cellName == name && cellPhone == phone)
                    {
                        worksheet.Cell(row, 5).Value = registerSuccess ? "Pass" : "Fail";
                        worksheet.Cell(row, 6).Value = loginSuccess ? "Pass" : "Fail";
                        Console.WriteLine($"Updated Excel: Row {row} -> Register: {worksheet.Cell(row, 5).Value}, Login: {worksheet.Cell(row, 6).Value}");
                        break;
                    }
                }

                workbook.Save();
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver3.Quit();
            driver3.Dispose();
        }
    }
}
