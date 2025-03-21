using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using excel = Microsoft.Office.Interop.Excel;



namespace TestLogIn
{
    [TestFixture]
    public class TestForgotPass
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
            driver6.Navigate().GoToUrl("https://localhost:44317/login");
        }
        public static IEnumerable<TestCaseData> Test_ForgotPass()
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open(filePath);
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Worksheets[7];
            excel.Range range = wsheet.UsedRange;

            string femail, emailgg, passgg, passnew, confirmpass;

            for (int i = 2; i <= range.Rows.Count; i++)
            {
                femail = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                emailgg = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";
                passgg = Convert.ToString((range.Cells[i, 3] as excel.Range)?.Value) ?? "";
                passnew = Convert.ToString((range.Cells[i, 4] as excel.Range)?.Value) ?? "";
                confirmpass = Convert.ToString((range.Cells[i, 5] as excel.Range)?.Value) ?? "";


                yield return new TestCaseData(femail, emailgg, passgg, passnew, confirmpass);
            }
            wbook.Close(false);
            app.Quit();
            Marshal.ReleaseComObject(wsheet);
            Marshal.ReleaseComObject(wbook);
            Marshal.ReleaseComObject(app);
        }


        [Test, TestCaseSource(nameof(Test_ForgotPass))]
        public void ForgotPass(string femail, string emailgg, string passgg, string passnew, string confirmpass)
        {
            WebDriverWait wait = new WebDriverWait(driver6, TimeSpan.FromSeconds(20));

            Thread.Sleep(5000);

            IWebElement text_forgot = wait.Until(d => d.FindElement(By.XPath("/html/body/div/div[2]/form/div[3]/a")));
            text_forgot.Click();

            IWebElement xacnhantext = wait.Until(d => d.FindElement(By.CssSelector("div.pass h1")));
            Assert.IsTrue(xacnhantext.Displayed);

            IWebElement nhap_email = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"Email\"]")));
            nhap_email.SendKeys(femail);

            IWebElement btn_forgot = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/div/form/div[2]/input")));
            btn_forgot.Click();

            Thread.Sleep(5000);

            var sendemail = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/div/div[1]")));
            Assert.IsTrue(sendemail.Displayed);

            Thread.Sleep(5000);

            driver6.Navigate().GoToUrl("https://mailtrap.io/");

            IWebElement login_mailtrap = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"screen-reader-shortcut-header\"]/nav/div/div/div[2]/div[4]/a[1]")));
            login_mailtrap.Click();

            IWebElement gg_mailtrap = wait.Until(d => d.FindElement(By.XPath("/html/body/main/div/div[1]/div[2]/div/a[1]")));
            gg_mailtrap.Click();

            Thread.Sleep(5000);

            IWebElement tk_mailtrap = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"identifierId\"]")));
            tk_mailtrap.SendKeys(emailgg);

            Thread.Sleep(5000);

            IWebElement click_mailtrap = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"identifierNext\"]/div/button")));
            click_mailtrap.Click();

            Thread.Sleep(4500);

            IWebElement mk_mailtrap = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"password\"]/div[1]/div/div[1]/input")));
            mk_mailtrap.SendKeys(passgg);

            Thread.Sleep(4500);

            IWebElement btndn_mailtrap = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"passwordNext\"]/div/button")));
            btndn_mailtrap.Click();


            IWebElement xacnhanhome = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"falconApp\"]/div/aside/div/a")));
            Assert.IsTrue(xacnhanhome.Displayed);

            Thread.Sleep(4000);

            IWebElement inboxLink = wait.Until(d => d.FindElement(By.XPath("//a[@title='My Inbox']")));
            inboxLink.Click();

            Thread.Sleep(8000);

            var emailList = driver6.FindElements(By.CssSelector("ul.messages_list li a"));

            Console.WriteLine("Tổng số email tìm thấy: " + emailList.Count);

            IWebElement selectedEmail = null;

            foreach (var email in emailList)
            {
                Console.WriteLine("Tiêu đề email: " + email.Text);

                if (email.Text.Contains(femail)) 
                {
                    selectedEmail = email;
                    break; 
                }
            }

            if (selectedEmail != null)
            {
                selectedEmail.Click();
                Thread.Sleep(5000);
            }
            else
            {
                Console.WriteLine("Không tìm thấy email phù hợp");
                return;
            }

            Thread.Sleep(20000);

            var iframes = driver6.FindElements(By.TagName("iframe"));
            Console.WriteLine("Tổng số iframe trên trang: " + iframes.Count);

            for (int i = 0; i < iframes.Count; i++)
            {
                driver6.SwitchTo().Frame(iframes[i]);
                Console.WriteLine($"Đã chuyển vào iframe {i}");

                var elements = driver6.FindElements(By.XPath("//a[contains(text(), 'Reset password')]"));
                if (elements.Count > 0)
                {
                    Console.WriteLine("Tìm thấy Reset password trong iframe " + i);
                    elements[0].Click();
                    break;
                }
                driver6.SwitchTo().DefaultContent();
            }

            Thread.Sleep(5000);
            var resetLink = driver6.FindElements(By.XPath("//a[contains(text(), 'Reset password')]"));
            Console.WriteLine("Số lượng link tìm thấy: " + resetLink.Count);

            Thread.Sleep(10000);

            string originalTab = driver6.CurrentWindowHandle;
            foreach (string window in driver6.WindowHandles)
            {
                if (window != originalTab)
                {
                    driver6.SwitchTo().Window(window);
                    break;
                }
            }

            Console.WriteLine("Chuyển sang tab mới thành công!");

            Thread.Sleep(5000);

            IWebElement resetpass = wait.Until(d => d.FindElement(By.Name("NewPassword")));
            resetpass.SendKeys(passnew);

            IWebElement cfresetpass = wait.Until(d => d.FindElement(By.Name("ConfirmNewPassword")));
            cfresetpass.SendKeys(confirmpass);

            IWebElement btnreset = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"wrapped\"]/div[1]/div/div/form/div[3]/input")));
            btnreset.Click();

            bool resettc;
            try
            {
                IWebElement thanhcong = wait.Until(d =>
                {
                    var datlaitc = d.FindElements(By.CssSelector("div.alert p"));
                    return datlaitc.Count > 0 ? datlaitc[0] : null;
                });

                resettc = thanhcong != null && thanhcong.Displayed;
            }
            catch (WebDriverTimeoutException)
            {
                resettc = false;
            }

            TrongExcel(femail, emailgg, passgg, passnew, confirmpass, resettc ? "Pass" : "Fail");

            //IWebElement datlaitc = wait.Until(d => d.FindElement(By.CssSelector("div.alert p")));
            Assert.IsTrue(resettc);
            Thread.Sleep(4000);
        }

        private void TrongExcel(string femail, string emailgg, string passgg, string passnew, string confirmpass, string result)
        {
            excel.Application app = new excel.Application();
            excel.Workbook wbook = app.Workbooks.Open(filePath);
            excel.Worksheet wsheet = (excel.Worksheet)wbook.Worksheets[7];
            excel.Range range = wsheet.UsedRange;

            string crfemail, cremailgg, crpassgg, crpassnew, crconfirmpass;

            bool found = false;

            for (int i = 2; i <= range.Rows.Count; i++)
            {
                crfemail = Convert.ToString((range.Cells[i, 1] as excel.Range)?.Value) ?? "";
                cremailgg = Convert.ToString((range.Cells[i, 2] as excel.Range)?.Value) ?? "";
                crpassgg = Convert.ToString((range.Cells[i, 3] as excel.Range)?.Value) ?? "";
                crpassnew = Convert.ToString((range.Cells[i, 4] as excel.Range)?.Value) ?? "";
                crconfirmpass = Convert.ToString((range.Cells[i, 5] as excel.Range)?.Value) ?? "";

                if (crfemail == femail && cremailgg == emailgg && crpassgg == passgg && crpassnew == passnew && crconfirmpass == crconfirmpass)
                {
                    wsheet.Cells[i, 6] = result;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine($"Không tìm thấy dữ liệu trong Excel: {emailgg}");
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
            driver6.Quit();
            driver6.Dispose();
        }
    }
}
