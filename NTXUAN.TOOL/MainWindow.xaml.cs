using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static List<string> Comments = new List<string>();

        public static bool Flag = true;
        public static int CountErrors = 0;
        public static ChromeDriver driver = null;

        public MainWindow()
        {
            InitializeComponent();
            //InitChromeDriver();
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
            catch (System.Exception e)
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
            if(driver == null)
            {
                ChromeOptions options = new ChromeOptions();

                options.AddArgument(@"user-data-dir=D:\Chrome\Profile");

                driver = new ChromeDriver(GetDriverService(), options);
            }
        }

        private void Button_Click_StartAsync(object sender, RoutedEventArgs e)
        {
            if(driver == null)
            {
                OpenChrome();
            }
            else
            {
                InitListJira();

                while (Flag)
                {
                    Thread.Sleep(3000);

                    CheckJiraStatus();
                }
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
                CheckJiraStatus();//delete-comment
            }
        }

        private void Button_Click_PushData(object sender, RoutedEventArgs e)
        {
            OpenChrome();

            DeleteComments();
            LoadFileData();
            //AddComments();
        }

        private void Button_Click_SplitFile(object sender, RoutedEventArgs e)
        {
            LoadFileData();
            SaveFileSplit();
        }

        private void LoadFileData()
        {
            Thread.Sleep(1000);

            string data = "";
            var MaxLength = 29000;
            var inputFileName = "D:/Selenium_Source/InputData.txt";

            List<string> lines = File.ReadLines(inputFileName).ToList();

            foreach (var item in lines)
            {
                data = data + item + "\n";

                if(data.Length > MaxLength)
                {
                    Comments.Add(data);
                    data = "";
                }
            }

            if (!string.IsNullOrEmpty(data))
            {
                Comments.Add(data);
            }
        }

        private void AddComments()
        {
            Thread.Sleep(1000);

            try
            {
                foreach (var item in Comments)
                {
                    var comment = driver.FindElement(By.CssSelector("#addcomment #footer-comment-button"));

                    comment.Click();
                    Thread.Sleep(1000);

                    var commentArea = driver.FindElement(By.CssSelector("#addcomment #comment"));

                    commentArea.SendKeys(item);
                    Thread.Sleep(2000);

                    var commentSubmit = driver.FindElement(By.CssSelector("#addcomment #issue-comment-add-submit"));

                    commentSubmit.Click();
                    Thread.Sleep(1500);
                }
            }
            catch (System.Exception e)
            {
            }
        }

        private void DeleteComments()
        {
            Thread.Sleep(1000);

            try
            {
                var comment = driver.FindElement(By.CssSelector(".activity-comment .delete-comment"));

                while(comment != null)
                {
                    ClickElement(".activity-comment .concise .action-head");
                    Thread.Sleep(500);
                    ClickElement(".activity-comment .delete-comment");
                    Thread.Sleep(500);

                    ClickElement("#comment-delete-submit");
                    Thread.Sleep(1000);

                    comment = driver.FindElement(By.CssSelector(".activity-comment .delete-comment"));
                }
            }
            catch (System.Exception e)
            {
            }
        }

        public static void SaveFileSplit()
        {
            int index = 1;

            try
            {
                // Clear files
                DirectoryInfo di = new DirectoryInfo("D:/Selenium_Source/Output");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                foreach (var item in Comments)
                {
                    using (StreamWriter sw = File.CreateText(string.Format("D:/Selenium_Source/Output/File_Output_{0}.txt", index++)))
                    {
                        sw.Write(item);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public static void ClickElement(string selector)
        {
            try
            {
                var element = driver.FindElement(By.CssSelector(selector));

                element.Click();
            }
            catch (Exception e)
            {
            }
        }

        private void Button_Click_GetData(object sender, RoutedEventArgs e)
        {
            OpenChrome();

            Comments = new List<string>();

            try
            {
                var fileName = string.Format("D:/OutputComments_{0}.txt", DateTime.Now.ToString("HH_mm_ss")); 
                var listComments = driver.FindElements(By.CssSelector(".activity-comment .action-body"));

                if (listComments != null && listComments.Count > 0)
                {
                    foreach (var item in listComments)
                    {
                        Comments.Add(item.Text);
                    }
                }

                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.Write(string.Join("", Comments.ToArray()));
                }
            }
            catch (System.Exception ex)
            {
            }
        }
    }
}
