using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace Kiemthu_CANIFA.Common
{
    public class BrowserSetup
    {
        public IWebDriver driver;

        // Hàm khởi tạo và mở trình duyệt
        public IWebDriver OpenBrowser()
        {
            ChromeOptions options = new ChromeOptions();
            // Ẩn dòng thông báo "Chrome is being controlled by automated test software"
            options.AddExcludedArgument("enable-automation");

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize(); // Phóng to toàn màn hình

            // Chờ tối đa 10s để các phần tử web tải xong
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            return driver;
        }

        // Hàm đóng trình duyệt sau khi test xong
        public void CloseBrowser()
        {
            if (driver != null)
            {
                driver.Quit();
            }
        }
    }
}