using NUnit.Framework;

using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace AutomatedTests
{
    public class ZendenTests
    {
        WebDriver driver;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://zenden.ru/");
            //driver.FindElement(By.XPath("//button[contains(.,'Хорошо')]")).Click();
        }

        [Test]
        public void TestPriceFilter()
        {
            new Actions(driver).MoveToElement(driver.FindElement(By.XPath("//a[text() = 'Мужчинам']"))).Build().Perform();
            driver.FindElement(By.XPath("//a[text() = 'Мужчинам']")).Click();

            var newElemMinPrice = driver.FindElement(By.XPath("//*[@name='PRICES[FROM]']"));
            var newElemMaxPrice = driver.FindElement(By.XPath("//*[@name='PRICES[TO]']"));

            newElemMinPrice.SendKeys(Keys.Control + "A");
            newElemMinPrice.SendKeys("1000");

            newElemMaxPrice.SendKeys(Keys.Control + "A");
            newElemMaxPrice.SendKeys("9000");
            driver.FindElement(By.XPath("//div[@class = 'slider__inputs']")).Click();


            new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(x => driver.FindElements(By.XPath(".//*[contains(@class,'.is-loading')]")).Count > 0);

            new WebDriverWait(driver, TimeSpan.FromSeconds(30))
               .Until(x => driver.FindElements(By.XPath(".//*[contains(@class,'.is-loading')]")).Count == 0);

            Actions actions = new Actions(driver);
            actions.SendKeys(Keys.End).Build().Perform();

            var webPrices = driver.FindElements(By.CssSelector(".product-price__current"));

            int[] actualPrices = webPrices.Select(webPrice => Int32.Parse(Regex.Replace(webPrice.Text, @"\D+", ""))).ToArray();
            actualPrices.ToList().ForEach(price => Assert.IsTrue(price >= 1000 && price <= 9000, "Some element did not pass into the filter"));
        }

        [Test]
        public void TestAddToCartButtonTooltipText()
        {
            var firstButtonAddToCart = driver.FindElement(By.CssSelector(".fp-promo-card.fp-promo-card_alt.swiper-slide.swiper-slide-active"));
            new Actions(driver).MoveToElement(firstButtonAddToCart.FindElement(By.CssSelector(".fp-promo-card__preview.link"))).Build().Perform();
            Assert.IsTrue(firstButtonAddToCart.FindElements(By.XPath(".//*[contains(@class,'fp-promo-card__button')]")).Any(),
                "Tooltip on 'To learn more' has not appeared");
            Assert.AreEqual(firstButtonAddToCart.FindElement(By.XPath(".//*[contains(@class,'button__text')]")).Text.Trim(), "Узнать больше",
                "Incorrect tooltip text");
        }

        [Test]
        public void NegativeTestPhoneNumberConfirmationWithEmptyPhoneNumber()
        {
            driver.FindElement(By.XPath("//span[text() = 'Войти']")).Click();
            driver.FindElement(By.XPath("//span[text() = 'Зарегистрироваться']")).Click();
            Assert.IsTrue(!driver.FindElements(By.CssSelector(".form__row form__row_submit.js-request.is-hidden")).Any(),
                "The register button is available if the phone number are empty");
        }

        [TearDown]
        public void CleanUp()
        {
            driver.Quit();
        }
    }
}