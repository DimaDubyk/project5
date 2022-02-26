using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;

namespace project5
{
    [TestFixture]
    public class Class1
    {
        public IWebDriver Driver { get; set; }
        public Process TorProcess { get; set; }
        public WebDriverWait Wait { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            //todo presetup:
            //add firefox to PATH env variable
            //based on: https://www.automatetheplanet.com/webdriver-selenium-tor-integration/
            
            //path to tor browser
            String torBinaryPath = @"[Path to Tor browser folder]\Tor Browser\Browser\firefox.exe";
            this.TorProcess = new Process();
            this.TorProcess.StartInfo.FileName = torBinaryPath;
            this.TorProcess.StartInfo.Arguments = "-n";
            this.TorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            this.TorProcess.Start();

            //setup Firefox profile via tor VPN
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("network.proxy.type", 1);
            profile.SetPreference("network.proxy.socks", "127.0.0.1");
            profile.SetPreference("network.proxy.socks_port", 9150);

           
            FirefoxOptions options = new FirefoxOptions();
            options.Profile = profile;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(path, "Resources");
            
            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(filePath, "geckodriver.exe");
            service.FirefoxBinaryPath = @"C:\Program Files\Mozilla Firefox\firefox.exe";

            //ugly hack to wait for esteblishing TOR VPN connection
            Thread.Sleep(10000);
            this.Driver = new FirefoxDriver(service, options);
            Driver.Manage().Window.Maximize();
            this.Wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(60));
        }

        [Test]
        public void Test1()
        {
            foreach (var user in ReadDataFile())
            {
                Driver.Navigate().GoToUrl("https://vk.com/login");
                Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("email")));
                Driver.FindElement(By.Id("email")).SendKeys(user.login);
                Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("pass")));
                Driver.FindElement(By.Id("pass")).SendKeys(user.pass);
                Driver.FindElement(By.Id("pass")).Click();
                Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("login_button")));
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.Driver.Quit();
            this.TorProcess.Kill();
        }
        
        public static List<(string login, string pass)> ReadDataFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(path, "Resources");
            string fileName = Path.Combine(filePath, "data.txt");
            string[] messageText = File.ReadAllLines(fileName);
            var users = messageText.Select(l => l.Split(':'))
                .Select(l2 => (l2?[0], (l2?.Length < 2)
                    ? throw new InvalidDataException("data.txt is corrupted, not all users has password")
                    : l2[1])).ToList();
            return users;
        }
    }
}