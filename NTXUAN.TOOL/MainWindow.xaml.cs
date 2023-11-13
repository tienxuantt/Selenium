using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NTXUAN.TOOL
{
    public partial class MainWindow : Window
    {
        public static List<ChromeDriver> drivers = new List<ChromeDriver>();
        public static ChromeDriver driver { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitChromeDriver();
        }

        private void InitChromeDriver()
        {
            OpenChrome();
        }

        private static ChromeDriverService GetDriverService()
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            return driverService;
        }

        private void Test_Element()
        {
            var userName = driver.FindElement(By.Id("iptUserName"));
            var userName2 = driver.FindElement(By.XPath("//*[@id='iptUserName']"));
            var userName3 = driver.FindElement(By.CssSelector("#iptUserName"));
            var btnLogin = driver.FindElement(By.CssSelector("#login"));

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

        public static void SendMessageSlack(string message)
        {
            WebClient client = new WebClient();

            string webhook = "https://hooks.slack.com/services/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";
            string payload = "{\"text\": \"" + message + "\"}";

            client.Headers.Add("Content-Type", "application/json");

            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        private async Task CloseWindowAsync()
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

        public static void OpenChrome()
        {
            ChromeOptions options = new ChromeOptions();

            options.AddArgument(@"user-data-dir=D:\Chrome\Profile");

            driver = new ChromeDriver(GetDriverService(), options);
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            var listIssue = driver.FindElements(By.CssSelector("#issuetable .summary"));

            if(listIssue != null && listIssue.Count > 0)
            {
                for (int i = 0; i < listIssue.Count; i++)
                {
                    var content = listIssue[i].Text;
                    SendMessageSlack(@"Có mã mới: " + content);
                }
            }
        }

        private void Button_Click_End(object sender, RoutedEventArgs e)
        {

        }
    }
}
