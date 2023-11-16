using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NTXUAN.TOOL
{
    public partial class MainWindow : Window
    {
        public static List<ChromeDriver> drivers = new List<ChromeDriver>();
        public static List<string> ListJira = new List<string>();
        public static List<string> Logs = new List<string>();

        public static bool Flag = true;
        public static int CountErrors = 0;

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
            string payload = "{\"text\": \"" + RemoveSpecialCharacters(message) + "\"}";

            client.Headers.Add("Content-Type", "application/json");

            try
            {
                client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
            }
            catch (System.Exception)
            {
            }
        }

        public static string RemoveSpecialCharacters(string str)
        {
            str = str.Replace("*", "");
            str = str.Replace("\"", "");

            return str;
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

        private void Button_Click_StartAsync(object sender, RoutedEventArgs e)
        {
            InitListJira();

            while (Flag)
            {
                Thread.Sleep(3000);

                CheckJiraStatus();
            }
        }

        public void InitListJira()
        {
            try
            {
                var listIssue = driver.FindElements(By.CssSelector("#issuetable .issuerow"));

                if (listIssue != null && listIssue.Count > 0)
                {
                    foreach (var item in listIssue)
                    {
                        var code = item.FindElement(By.CssSelector(".issuekey"));
                        var status = item.FindElement(By.CssSelector(".status"));
                        var labels = item.FindElement(By.CssSelector(".labels"));

                        var newKey = string.Format("{0} - {1} - {2}", code.Text, status.Text, labels.Text);

                        ListJira.Add(newKey);
                    }
                }
            }
            catch (System.Exception e)
            {
                CountErrors += 1;
            }
        }

        public void CheckJiraStatus()
        {
            Thread.Sleep(2000);

            try
            {
                var listIssue = driver.FindElements(By.CssSelector("#issuetable .issuerow"));

                if (listIssue != null && listIssue.Count > 0)
                {
                    foreach (var item in listIssue)
                    {
                        var code = item.FindElement(By.CssSelector(".issuekey"));
                        var title = item.FindElement(By.CssSelector(".summary"));
                        var status = item.FindElement(By.CssSelector(".status"));
                        var labels = item.FindElement(By.CssSelector(".labels"));

                        var newKey = string.Format("{0} - {1} - {2}", code.Text, status.Text, labels.Text);
                        var endKey = string.Format("{0} - {1}", status.Text, labels.Text);

                        // Phần tử được cập nhật
                        var itemExits = ListJira.FirstOrDefault(s => s.StartsWith(code.Text) && !s.EndsWith(endKey));
                        // Phần tử phát sinh mới
                        var itemNew = ListJira.FirstOrDefault(s => s.StartsWith(code.Text));

                        if (itemExits != null)
                        {
                            SendMessageSlack(string.Format("Cập nhật: {0} - {1} - {2} - {3}", code.Text, title.Text, status.Text, labels.Text));

                            ListJira.Remove(itemExits);
                            ListJira.Add(newKey);
                        }
                        else if (itemNew == null)
                        {
                            ListJira.Add(newKey);

                            SendMessageSlack(string.Format("Có mã mới: {0} - {1} - {2}", code.Text, title.Text, status.Text));
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                CheckJiraStatus();
            }
        }

        private void Button_Click_End(object sender, RoutedEventArgs e)
        {
            Flag = false;
        }
    }
}
