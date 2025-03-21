using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;
using System.Threading;
using SeleniumExtras.WaitHelpers;
using NUnit.Framework.Internal;
using System.Globalization;


namespace TestLogIn
{
    public class TestXacNhanDon
    {
        private static IWebDriver driver6;
        //private static string filePath = "F:\\Nam3\\DBCLPM\\Lab\\test3.xlsx";
        [SetUp]
        public void SetUp()
        {
            driver6 = new EdgeDriver();
            driver6.Manage().Window.Maximize();
            driver6.Navigate().GoToUrl("https://localhost:44317/DKDN/Login");
        }


        [Test]
        public void Xacnhandon()
        {
            WebDriverWait wait = new WebDriverWait(driver6, TimeSpan.FromSeconds(15));
            Thread.Sleep(2000);

            IWebElement sdt = wait.Until(d => d.FindElement(By.Name("Sdt")));
            sdt.SendKeys("0906483258");
            Thread.Sleep(2000);

            IWebElement mk = wait.Until(d => d.FindElement(By.Name("Matkhau")));
            mk.SendKeys("Thy01072004");
            Thread.Sleep(2000);

            IWebElement btn_click = wait.Until(d => d.FindElement(By.ClassName("login-btn")));
            btn_click.Click();
            Thread.Sleep(2000);

            IWebElement icon_user = wait.Until(d => d.FindElement(By.ClassName("user_img")));
            Assert.IsTrue(icon_user.Displayed);
            icon_user.Click();

            IWebElement click_lsdh = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"actions\"]/div[2]/div[2]/ul/li[2]/a")));
            click_lsdh.Click();
            Thread.Sleep(2000);

            IWebElement lsdh = wait.Until(d => d.FindElement(By.CssSelector("h2")));
            Assert.IsTrue(lsdh.Displayed);

            Thread.Sleep(1000);

            try
            {
                var orders = driver6.FindElements(By.CssSelector("tbody tr"));

                if (!orders.Any())
                {
                    Console.WriteLine("Không có đơn hàng nào trong danh sách.");
                    return;
                }

                foreach (var order in orders)
                {
                    string orderTimeText = order.FindElement(By.CssSelector("td:nth-child(1)")).Text.Trim();
                    DateTime orderTime;

                    if (DateTime.TryParseExact(orderTimeText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out orderTime))
                    {
                        string status = order.FindElement(By.CssSelector("td:nth-child(4)")).Text.Trim();

                        Console.WriteLine($"Đơn hàng: {orderTimeText}, Trạng thái: '{status}'");

                        Console.WriteLine($"Order Time: {orderTime}");
                        Console.WriteLine($"Order Time + 10 minutes: {orderTime.AddMinutes(10)}");
                        Console.WriteLine($"Current Time: {DateTime.Now}");
                        Console.WriteLine($"Condition Met: {orderTime.AddMinutes(10) <= DateTime.Now}");

                        if (status.Contains("Chờ xác nhận") && orderTime.AddMinutes(10) <= DateTime.Now)
                        {
                            Console.WriteLine($"Checking Order Time: {orderTime}, Now: {DateTime.Now}");
                            Console.WriteLine($"Admin - Đơn hàng lúc {orderTimeText}");

                            driver6.Navigate().GoToUrl("https://localhost:44317/Login-admin");

                            IWebElement tk_admin = wait.Until(d => d.FindElement(By.Name("sdt")));
                            tk_admin.SendKeys("941256718");

                            Thread.Sleep(1000);

                            IWebElement mk_admin = wait.Until(d => d.FindElement(By.Name("password")));
                            mk_admin.SendKeys("345678");

                            IWebElement btn_admin = wait.Until(d => d.FindElement(By.ClassName("login-btn")));
                            btn_admin.Click();
                            Thread.Sleep(2000);

                            IWebElement home_admin = wait.Until(d => d.FindElement(By.CssSelector("div.title")));
                            Assert.IsTrue(home_admin.Displayed);

                            IWebElement nav_admin = wait.Until(d => d.FindElement(By.CssSelector("label.open")));
                            nav_admin.Click();
                            Thread.Sleep(2000);

                            IWebElement xacnhandon = wait.Until(d => d.FindElement(By.XPath("//nav//li[2]/a")));
                            xacnhandon.Click();
                            Thread.Sleep(2000);

                            IWebElement xacnhan = wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn-success')]")));
                            xacnhan.Click();

                            wait.Until(ExpectedConditions.AlertIsPresent());
                            IAlert alert = driver6.SwitchTo().Alert();
                            alert.Accept();

                            Thread.Sleep(3000);
                            driver6.Navigate().Refresh();
                            Thread.Sleep(2000);

                            IWebElement nav_admin1 = wait.Until(d => d.FindElement(By.CssSelector("label.open")));
                            nav_admin1.Click();
                            Thread.Sleep(2000);

                            IWebElement logout = wait.Until(d => d.FindElement(By.XPath("//nav//li[5]/a")));
                            logout.Click();

                            driver6.Navigate().GoToUrl("https://localhost:44317/DKDN/Login");

                            IWebElement sdt1 = wait.Until(d => d.FindElement(By.Name("Sdt")));
                            sdt1.SendKeys("0906483258");
                            Thread.Sleep(2000);

                            IWebElement mk1 = wait.Until(d => d.FindElement(By.Name("Matkhau")));
                            mk1.SendKeys("Thy01072004");
                            Thread.Sleep(2000);

                            IWebElement btn_click1 = wait.Until(d => d.FindElement(By.ClassName("login-btn")));
                            btn_click1.Click();
                            Thread.Sleep(2000);

                            IWebElement icon_user1 = wait.Until(d => d.FindElement(By.ClassName("user_img")));
                            Assert.IsTrue(icon_user1.Displayed);
                            icon_user1.Click();

                            IWebElement click_lsdh1 = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"actions\"]/div[2]/div[2]/ul/li[2]/a")));
                            click_lsdh1.Click();
                            Thread.Sleep(2000);

                            var updatedOrders = driver6.FindElements(By.CssSelector("tbody tr"));
                            foreach (var updatedOrder in updatedOrders)
                            {
                                string updatedOrderTimeText = updatedOrder.FindElement(By.CssSelector("td:nth-child(1)")).Text.Trim();
                                DateTime updatedOrderTime;

                                if (DateTime.TryParseExact(updatedOrderTimeText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out updatedOrderTime))
                                {
                                    string updatedStatus = updatedOrder.FindElement(By.CssSelector("td:nth-child(4)")).Text.Trim();

                                    if (updatedStatus.Contains("Hoàn thành") && updatedOrderTime == orderTime)
                                    {
                                        Console.WriteLine($"Đơn hàng {updatedOrderTimeText} đã chuyển sang trạng thái Hoàn thành!");
                                    }
                                }
                            }
                            break; 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
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
