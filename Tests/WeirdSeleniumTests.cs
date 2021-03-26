
using Magenic.Maqs.BaseSeleniumTest;
using Magenic.Maqs.BaseSeleniumTest.Extensions;
using Magenic.Maqs.Utilities.Data;
using Magenic.Maqs.Utilities.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace Tests
{

    /// <summary>
    /// Weird Selenium test class
    /// </summary>
    [TestClass]
    public class WeirdSeleniumTests : BaseSeleniumTest
    {
        /// <summary>
        /// Find element throws if no element is found
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NoSuchElementException))]
        public void FindElementThrows()
        {
            WebDriver.FindElement(By.CssSelector("#missing"));
            Assert.Fail("Error should have been thrown");
        }

        /// <summary>
        /// Find elements does not throw if no elements are found
        /// </summary>
        [TestMethod]
        public void FindElementsZero()
        {
            var elements = WebDriver.FindElements(By.CssSelector("#missing"));
            Assert.AreEqual(0, elements.Count);
        }

        /// <summary>
        /// Problem with mixing waits
        /// </summary>
        [TestMethod]
        public void TooMuchWaiting()
        {
            LoginPageModel page = new LoginPageModel(this.TestObject);
            page.OpenLoginPage();
            Stopwatch stopwatch = new Stopwatch();

            WebDriverWait wait = new WebDriverWait(new SystemClock(), WebDriver, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(.5));
            WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            stopwatch.Start();

            try
            {
                // Will timeout in 18.5+ seconds
                wait.Until(x => x.FindElement(By.CssSelector("#missing")));
            }
            catch
            {
                // We expect this to throw an exception
            }

            stopwatch.Stop();

            double seconds = stopwatch.Elapsed.TotalSeconds;

            Assert.IsTrue(seconds > 6 && seconds < 7, $"Took {seconds}, but expected 6-7 seconds");
            Console.WriteLine(stopwatch.Elapsed);
        }

        /// <summary>
        /// Open page test
        /// </summary>
        [TestMethod]
        public void WaitingWithElements()
        {
            LoginPageModel page = new LoginPageModel(this.TestObject);
            page.OpenLoginPage();
            Stopwatch stopwatch = new Stopwatch();

            WebDriverWait wait = new WebDriverWait(new SystemClock(), WebDriver, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(.5));
            WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            stopwatch.Start();

            // Will find 0 elements in 9 seconds
            wait.Until(x => x.FindElements(By.CssSelector("#missing")));

            stopwatch.Stop();

            double seconds = stopwatch.Elapsed.TotalSeconds;

            Assert.IsTrue(seconds > 3 && seconds < 4, $"Took {seconds}, but expected about 3 seconds");
            Console.WriteLine(stopwatch.Elapsed);
        }

        /// <summary>
        /// Open page test
        /// </summary>
        [TestMethod]
        public void FindParent()
        {
            LoginPageModel page = new LoginPageModel(this.TestObject);
            page.OpenLoginPage();

            // Find child node
            var element = this.WebDriver.FindElement(By.CssSelector("#Login"));
            Assert.AreEqual("Login", element.Text);

            // Get the child nodes parent
            var parentElement = element.FindElement(By.XPath(".."));
            Assert.AreEqual("usernamePassword:Login", parentElement.Text.Replace(Environment.NewLine, "").Trim());
        }

        /// <summary>
        /// Enter credentials test
        /// </summary>
        [TestMethod]
        public void DoAllTheWeirdThings()
        {
            // Emulate iPhone
            this.TestObject.OverrideWebDriver(() => EmulateiPhone());

            // Make sure we are using an event firing web driver
            Assert.IsInstanceOfType(WebDriver, typeof(EventFiringWebDriver));
            ((EventFiringWebDriver)WebDriver).Navigating += WebDriverNavigating;
            ((EventFiringWebDriver)WebDriver).Navigated += WebDriverNavigated;

            LoginPageModel page = new LoginPageModel(this.TestObject);
            page.OpenLoginPage();

            WebDriver.FindElement(By.CssSelector("body")).SendKeys(Keys.Control + "t");

            // Add red border around login button
            var element = ((EventFiringWebDriver)WebDriver).GetLowLevelDriver().FindElement(By.Id("Login"));
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].setAttribute('style', 'border: 2px solid red;');", element);

            // Save login button image
            Screenshot elementshot = ((ITakesScreenshot)element).GetScreenshot();
            var elementshotPath = Path.Combine(LoggingConfig.GetLogDirectory(), "elementshot.png");
            elementshot.SaveAsFile(elementshotPath);
            IncludeScreeshot(TestObject, elementshotPath);

            // Rename login button
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].innerHTML='Messing With Dev'", element);

            // Save renamed login button image
            Screenshot elementshot2 = ((ITakesScreenshot)element).GetScreenshot();
            var elementshotPath2 = Path.Combine(LoggingConfig.GetLogDirectory(), "elementshot2.png");
            elementshot2.SaveAsFile(elementshotPath2);
            IncludeScreeshot(TestObject, elementshotPath2);

            // Save screenshot
            Screenshot screenshot = ((ITakesScreenshot)WebDriver).GetScreenshot();
            var screenshotPath = Path.Combine(LoggingConfig.GetLogDirectory(), "screenshot.png");
            screenshot.SaveAsFile(screenshotPath);
            IncludeScreeshot(TestObject, screenshotPath);

            // Launch a new tab
            string newTab = "window.open('https://magenicautomation.azurewebsites.net/Automation/iFramePage');";
            ((IJavaScriptExecutor)WebDriver).ExecuteScript(newTab);

            // Switch to new/last tab
            WebDriver.SwitchTo().Window(WebDriver.WindowHandles.Last());

            // Save screenshot
            Screenshot screenshotiFrame = ((ITakesScreenshot)WebDriver).GetScreenshot();
            var screenshotPathiFrame = Path.Combine(LoggingConfig.GetLogDirectory(), "screenshotiFrame.png");
            screenshotiFrame.SaveAsFile(screenshotPathiFrame);
            IncludeScreeshot(TestObject, screenshotPathiFrame);

            // Switch back to origianl tab and interact with the iFrame content
            WebDriver.SwitchTo().Window(WebDriver.WindowHandles.First());

            // Save page source
            Log.LogMessage(MessageType.INFORMATION, WebDriver.PageSource);
        }






        [TestMethod]
        public void FunWithTabsAndiFrames()
        {
            // Open a page with an iFrame
            WebDriver.Navigate().GoToUrl("https://magenicautomation.azurewebsites.net/Automation/iFramePage");

            // Launch a new tab
            string newTab = "window.open('https://magenicautomation.azurewebsites.net');";
            ((IJavaScriptExecutor)WebDriver).ExecuteScript(newTab);

            // Switch to new/last tab
            WebDriver.SwitchTo().Window(WebDriver.WindowHandles.Last());

            // Interact with page
            LoginPageModel page = new LoginPageModel(this.TestObject);
            page.OpenLoginPage();
            page.EnterCredentials("Bad", "StillBad");

            // Switch back to origianl tab and interact with the iFrame content
            WebDriver.SwitchTo().Window(WebDriver.WindowHandles.First());
            WebDriver.SwitchTo().Frame("mageniciFrame");

            WebDriver.FindElement(By.CssSelector("[href='https://magenic.com/contact']")).Click();
            WebDriver.FindElement(By.CssSelector("[placeholder='First Name']")).SendKeys("Ted");
            WebDriver.FindElement(By.CssSelector("[placeholder='Last Name']")).SendKeys("Johnson");

            // Do to top level page and interact withit
            WebDriver.SwitchTo().DefaultContent();
            WebDriver.FindElement(By.CssSelector("#ContactButton A")).Click();

            // Switch to last tab
            WebDriver.SwitchTo().Window(WebDriver.WindowHandles.Last());
            page.LoginWithInvalidCredentials("Worse", "FarWorse");
            Assert.IsTrue(page.ErrorMessage.Displayed, "Error message should be displayed");
        }

        /// <summary>
        /// Get a Chrome driver setup with emulation 
        /// </summary>
        /// <returns>A Chrome driver</returns>
        private IWebDriver EmulateiPhone()
        {
            ChromeOptions options = new ChromeOptions();
            options.EnableMobileEmulation("iPhone 8");

            return WebDriverFactory.GetChromeDriver(SeleniumConfig.GetCommandTimeout(), options, "DEFAULT");
        }

        /// <summary>
        /// Add an image to our html logger
        /// </summary>
        /// <param name="testObject">The test object</param>
        /// <param name="path">Path to the image</param>
        private void IncludeScreeshot(SeleniumTestObject testObject, string path)
        {
            var writer = new StreamWriter(((FileLogger)testObject.Log).FilePath, true);

            writer.WriteLine(StringProcessor.SafeFormatter(
                        "<div class='collapse col-12 show' data-logtype='IMAGE'><div class='card'><div class='card-body'><h6 class='card-subtitle mb-1'></h6></div><a class='pop'><img class='card-img-top rounded' src='{0}'style='width: 200px;'></a></div></div>", path));

            writer.Flush();
            writer.Close();

            // Add the file as an attachment
            this.TestObject.AddAssociatedFile(path);
        }

        /// <summary>
        /// Event for webdriver that is navigating
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event object</param>
        private void WebDriverNavigating(object sender, WebDriverNavigationEventArgs e)
        {
            TestContext.WriteLine($"CUSTOM - Navigating to: {e.Url}");
        }

        /// <summary>
        /// Event for webdriver that has navigated
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event object</param>
        private void WebDriverNavigated(object sender, WebDriverNavigationEventArgs e)
        {
            TestContext.WriteLine($"CUSTOM - Navigated to: {e.Url}");
        }
    }
}
