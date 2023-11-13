using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NTXUAN.TOOL
{
    public partial class MainWindow : Window
    {
        public static List<ChromeDriver> drivers = new List<ChromeDriver>();
        public static ChromeDriver driver { get; set; }

        public static int tab { get; set; } = 1;

        public MainWindow()
        {
            InitializeComponent();
            //initChromeDriver();
            //SendMessageSlack("Nguyen tien xuan");
        }

        // Khoi tao driver
        private void initChromeDriver()
        {
            driver = new ChromeDriver(GetDriverService());

            driver.Url = "https://whoer.net/";
            driver.Navigate();
        }

        private static ChromeDriverService GetDriverService()
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            return driverService;
        }

        private void Test_Navigate()
        {
            switch (tab)
            {
                case 1:
                    driver.Url = "https://facebook.com";
                    driver.Navigate();
                    break;
                case 2:
                    driver.Url = "https://youtube.com";
                    driver.Navigate();
                    break;
                case 3:
                    driver.Url = "https://google.com";
                    driver.Navigate();
                    break;
                case 4:
                    driver.Navigate().Back();
                    break;
                case 5:
                    driver.Navigate().Forward();
                    break;
                case 6:
                    driver.Navigate().Refresh();
                    break;
            }

            tab++;
        }

        private void Test_WindowHandles()
        {
            ReadOnlyCollection<string> windowHandles = driver.WindowHandles;

            string firstTab = windowHandles.First();

            string lastTab = windowHandles.Last();

            driver.SwitchTo().Window(lastTab);
            driver.Manage().Window.Maximize();
        }

        private void Test_Cookie()
        {
            //Cookie cookie = new OpenQA.Selenium.Cookie("key", "value");

            //driver.Manage().Cookies.AddCookie(cookie);
            //var cookies = driver.Manage().Cookies.AllCookies;
            //driver.Manage().Cookies.DeleteCookieNamed("CookieName");
            //driver.Manage().Cookies.DeleteAllCookies();
        }

        private void Test_Screenshot()
        {
            Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();

            screenshot.SaveAsFile("D:/Images/qlts.png");
        }

        private void Test_Frame()
        {
            driver.SwitchTo().Frame(1);

            driver.SwitchTo().Frame("frameName");

            IWebElement element = driver.FindElement(By.Id("id"));

            driver.SwitchTo().Frame(element);
        }

        private void Test_Element()
        {
            var userName = driver.FindElement(By.Id("iptUserName"));
            var userName2 = driver.FindElement(By.XPath("//*[@id='iptUserName']"));
            var userName3 = driver.FindElement(By.CssSelector("#iptUserName"));
            var btnLogin = driver.FindElement(By.CssSelector("#login"));

            //userName.SendKeys("misaqlts");
            userName3.SendKeys("misaqlts2");
            userName3.Clear();
            btnLogin.Click();

            var display = btnLogin.Displayed;
            var child = btnLogin.FindElement(By.CssSelector("#demo"));
            var className = btnLogin.GetAttribute("class");
            var toa_do = btnLogin.Location;
            var selected = btnLogin.Selected;
            var tagName = btnLogin.TagName;
        }

        private void Test_Script()
        {
            var btnLogin = driver.FindElement(By.CssSelector("#login"));

            // driver.ExecuteScript("alert('hello');");

            IJavaScriptExecutor js = driver as IJavaScriptExecutor;

            var dataFromJS = (string)js.ExecuteScript("var content = document.getElementById('login').innerHTML;return content;");
            MessageBox.Show(dataFromJS);
        }

        private void Test_Profile()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument(@"user-data-dir=D:\Selenium\Profile\Profile_02");

            IWebDriver driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://qltsapp.misa.vn");
        }

        public static void SendMessageSlack(string message)
        {
            WebClient client = new WebClient();

            string webhook = "https://hooks.slack.com/services/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";
            string payload = "{\"text\": \"" + message + "\"}";

            client.Headers.Add("Content-Type", "application/json");

            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        public static void Login_TapHoaMMO(ChromeDriver driver)
        {
            Task.Delay(2000);

            try
            {
                var txtUserName = driver.FindElement(By.CssSelector("#login_email"));
                var txtPassword = driver.FindElement(By.CssSelector("#login_password"));
                var btnLogin = driver.FindElement(By.CssSelector("#loginButton"));

                var URL = driver.Url;

                if (txtUserName != null)
                {
                    txtUserName.SendKeys("tienxuantt@gmail.com");
                    txtPassword.SendKeys("Login@12345");

                    Task.Delay(2000);

                    if(URL == "https://taphoammo.net/login.html")
                    {
                        btnLogin.Click();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task CloseWindowAsync(ChromeDriver driver)
        {
            if (driver != null)
            {
                await Task.Run(() =>
                {
                    driver.Close();
                    driver.Quit();
                });
            }
        }

        public static async Task OpenChrome(int index)
        {
            await Task.Run(() =>
            {
                ChromeOptions options = new ChromeOptions();
                int width = 640;
                int height = 500;
                string profile = string.Format("Profile_00{0}", index + 1);

                //options.AddHttpProxy("104.143.252.190", 5804, "tonggiang", "Zxcv123123");
                //options.AddArgument(@"user-data-dir=D:\Selenium\Profile\" + profile);

                var driverNew = new ChromeDriver(GetDriverService(), options);

                if (index > 2)
                {
                    driverNew.Manage().Window.Position = new System.Drawing.Point((index % 3) * width + 5 * (index % 3), 10 + height);
                }
                else
                {
                    driverNew.Manage().Window.Position = new System.Drawing.Point(index * width + 5 * index, 10);
                }

                driverNew.Manage().Window.Size = new System.Drawing.Size(width, height);

                driverNew.Navigate().GoToUrl("https://taphoammo.net/login.html");

                drivers.Add(driverNew);

                Login_TapHoaMMO(driverNew);
            });
        }

        private void Button_Click_OpenAsync(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                OpenChrome(i).GetAwaiter();
            }
        }

        private void Button_Click_CloseAsync(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < drivers.Count; i++)
            {
                CloseWindowAsync(drivers[i]).GetAwaiter();
            }

            drivers = new List<ChromeDriver>();
        }
    }
}
