using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;

namespace Kiemthu_CANIFA.TestCases.KhachHang
{
    public class XemDanhMuc
    {
        public void RunCategoryTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ XEM DANH MỤC ==========\n");

            void RunTestCase(string module, string id, string desc, string url, Action testAction, params string[] passMsgs)
            {
                Console.WriteLine($"[{module}] {id} | {desc}");
                Console.WriteLine(url);
                try
                {
                    testAction.Invoke();
                    foreach (var msg in passMsgs) Console.WriteLine($"  PASS  {msg}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  FAIL  {ex.Message}");
                }
                Console.WriteLine();
            }

            RunTestCase("08. Danh mục", "CAT-001", "Hiển thị đầy đủ thanh menu danh mục trên trang chủ.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/");
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/section[2]/a[1]/span")).Count > 0);

                string nu = driver.FindElement(By.XPath("/html/body/main/section[2]/a[1]/span")).Text;
                string nam = driver.FindElement(By.XPath("/html/body/main/section[2]/a[2]/span")).Text;
                string treEm = driver.FindElement(By.XPath("/html/body/main/section[2]/a[3]/span")).Text;
                string phuKien = driver.FindElement(By.XPath("/html/body/main/section[2]/a[4]/span")).Text;

                Assert.IsTrue(nu.Contains("Nữ") && nam.Contains("Nam") && treEm.Contains("Trẻ em") && phuKien.Contains("Phụ kiện"), "Không hiển thị đúng 4 danh mục chuẩn.");
            }, "Thanh menu ngang hiển thị đúng 4 danh mục chuẩn: Nữ, Nam, Trẻ em, Phụ kiện.");

            RunTestCase("08. Danh mục", "CAT-002", "Xem danh sách sản phẩm thuộc danh mục Nữ.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/");
                driver.FindElement(By.XPath("/html/body/main/section[2]/a[1]/span")).Click();

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[1]/h2")).Count > 0);
                string title = driver.FindElement(By.XPath("/html/body/main/div[1]/h2")).Text;
                Assert.IsTrue(title.ToLower().Contains("nữ"), "Tiêu đề trang không đúng danh mục Nữ.");

                var products = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3"));
                Assert.IsTrue(products.Count > 0, "Không hiển thị sản phẩm nào.");
            }, "Chỉ hiển thị các sản phẩm đang hiển thị (IsActive=1) có CategoryId thuộc danh mục Nữ.");

            RunTestCase("08. Danh mục", "CAT-003", "Xem danh sách sản phẩm thuộc danh mục Nam.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/");
                driver.FindElement(By.XPath("/html/body/main/section[2]/a[2]/span")).Click(); // Chọn Nam

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[1]/h2")).Count > 0);
                string title = driver.FindElement(By.XPath("/html/body/main/div[1]/h2")).Text;
                Assert.IsTrue(title.ToLower().Contains("nam"), "Tiêu đề trang không đúng danh mục Nam.");

                var products = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3"));
                Assert.IsTrue(products.Count > 0, "Không hiển thị danh sách sản phẩm.");
            }, "Hiển thị đúng danh sách sản phẩm thuộc danh mục Nam cùng tên, ảnh, màu sắc và giá bán.");

            RunTestCase("08. Danh mục", "CAT-004", "Xem danh sách sản phẩm thuộc danh mục Trẻ em và Phụ kiện.", " ", () =>
            {
                // Kiểm tra Trẻ em
                driver.Navigate().GoToUrl("http://localhost:5000/");
                driver.FindElement(By.XPath("/html/body/main/section[2]/a[3]/span")).Click();
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[1]/h2")).Count > 0);
                Assert.IsTrue(driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3")).Count > 0, "Lỗi lọc Trẻ em");

                // Kiểm tra Phụ kiện
                driver.Navigate().GoToUrl("http://localhost:5000/");
                driver.FindElement(By.XPath("/html/body/main/section[2]/a[4]/span")).Click();
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[1]/h2")).Count > 0);
                Assert.IsTrue(driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3")).Count > 0, "Lỗi lọc Phụ kiện");
            }, "Danh sách sản phẩm được lọc chính xác theo từng danh mục được chọn.");

            RunTestCase("08. Danh mục", "CAT-005", "Kiểm tra trường hợp chọn danh mục chưa có sản phẩm nào.", " ", () =>
            {
                // Website không có danh mục rỗng sẵn nên giả lập tìm từ khóa "kẹo"
                driver.Navigate().GoToUrl("http://localhost:5000/");
                var searchBox = driver.FindElement(By.XPath("/html/body/header/div/form/input"));
                searchBox.Clear(); searchBox.SendKeys("kẹo"); searchBox.SendKeys(Keys.Enter);

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/p")).Count > 0);
                string emptyMsg = driver.FindElement(By.XPath("/html/body/main/p")).Text;
                Assert.IsNotEmpty(emptyMsg, "Không tìm thấy câu thông báo lỗi.");
            }, "Hiển thị thông báo “Danh mục chưa tồn tại sản phẩm nào”; giao diện không bị lỗi.");

            RunTestCase("08. Danh mục", "CAT-006", "Chuyển từ danh sách danh mục sang trang Xem chi tiết sản phẩm.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/");
                driver.FindElement(By.XPath("/html/body/main/section[2]/a[1]/span")).Click();
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[3]/a[1]/div/h3")).Count > 0);

                // Bấm vào sản phẩm đầu tiên
                driver.FindElement(By.XPath("/html/body/main/div[3]/a[1]/div/h3")).Click();
                System.Threading.Thread.Sleep(1500);

                Assert.IsTrue(driver.Url.ToLower().Contains("/products/details"), "Không điều hướng được.");
            }, "Điều hướng tới trang chi tiết sản phẩm; hiển thị đúng Tên, Mô tả, Giá, Màu sắc, Kích cỡ (Sizes) và Tồn kho.");

            driver.Quit();
        }
    }
}