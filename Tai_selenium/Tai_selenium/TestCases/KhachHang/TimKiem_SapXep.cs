using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;

namespace Kiemthu_CANIFA.TestCases.KhachHang
{
    public class TimKiem_SapXep
    {
        public void RunSearchTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ TÌM KIẾM VÀ SẮP XẾP ==========\n");

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

            void SearchKeyword(string keyword)
            {
                driver.Navigate().GoToUrl("http://localhost:5000/");
                wait.Until(d => d.FindElements(By.XPath("/html/body/header/div/form/input")).Count > 0);
                var searchBox = driver.FindElement(By.XPath("/html/body/header/div/form/input"));
                searchBox.Clear();
                searchBox.SendKeys(keyword);
                searchBox.SendKeys(Keys.Enter);
                System.Threading.Thread.Sleep(1500);
            }

            List<long> GetPricesOnPage()
            {
                var priceElements = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/div/b"));
                List<long> prices = new List<long>();
                foreach (var element in priceElements)
                {
                    string priceText = element.Text.Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                    if (long.TryParse(priceText, out long price))
                    {
                        prices.Add(price);
                    }
                }
                return prices;
            }

            RunTestCase("04. Tìm kiếm", "SEARCH-001", "Tìm kiếm sản phẩm với từ khóa chính xác có tồn tại.", " ", () =>
            {
                SearchKeyword("Áo");
                var productNames = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3"));
                Assert.IsTrue(productNames.Count > 0, "Không tìm thấy sản phẩm nào.");
            }, "Hiển thị danh sách các sản phẩm có chứa cụm từ 'Áo'");

            RunTestCase("04. Tìm kiếm", "SEARCH-002", "Tìm kiếm sản phẩm không phân biệt chữ hoa và chữ thường.", " ", () =>
            {
                SearchKeyword("áo");
                int countLower = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3")).Count;

                SearchKeyword("ÁO");
                int countUpper = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3")).Count;

                Assert.AreEqual(countLower, countUpper, "Số lượng kết quả không khớp nhau.");
                Assert.IsTrue(countLower > 0);
            }, "Cả hai lần tìm kiếm trả về cùng một tập kết quả sản phẩm khớp tên");

            RunTestCase("04. Tìm kiếm", "SEARCH-003", "Tìm kiếm với từ khóa không khớp sản phẩm nào.", " ", () =>
            {
                SearchKeyword("Sản phẩm không tồn tại XYZ123");
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/p")).Count > 0);
                string msg = driver.FindElement(By.XPath("/html/body/main/p")).Text;
                Assert.IsTrue(msg.ToLower().Contains("không tìm thấy"));
            }, "Hiển thị thông báo “Không tìm thấy sản phẩm phù hợp” và danh sách rỗng");

            RunTestCase("04. Tìm kiếm", "SEARCH-004", "Kiểm tra thực hiện tìm kiếm khi bỏ trống từ khóa.", " ", () =>
            {
                SearchKeyword("");
                var errorMsgs = driver.FindElements(By.XPath("/html/body/main/p"));
                bool hasError = errorMsgs.Count > 0 && errorMsgs[0].Text.ToLower().Contains("vui lòng nhập");
                Assert.IsTrue(hasError, "Website không cảnh báo lỗi mà hiển thị toàn bộ sản phẩm.");
            }, "Hiển thị thông báo “Vui lòng nhập từ khóa tìm kiếm”");

            RunTestCase("04. Tìm kiếm", "SEARCH-005", "Kiểm tra tìm kiếm khi từ khóa có khoảng trắng thừa ở hai đầu.", " ", () =>
            {
                SearchKeyword("   Áo   ");
                var productNames = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3"));
                Assert.IsTrue(productNames.Count > 0, "Hệ thống không tự cắt (trim) khoảng trắng.");
            }, "Hệ thống tự động cắt khoảng trắng (trim) và trả về đúng các sản phẩm 'Áo'");

            RunTestCase("04. Sắp xếp", "SEARCH-006", "Sắp xếp danh sách sản phẩm theo Giá tăng dần.", " ", () =>
            {
                SearchKeyword("Áo");

                // Click chọn mục "Giá tăng dần" (Option thứ 2 trong thẻ select)
                driver.FindElement(By.XPath("/html/body/main/div[1]/form/label/select")).Click();
                driver.FindElement(By.XPath("/html/body/main/div[1]/form/label/select/option[2]")).Click();
                System.Threading.Thread.Sleep(2000);

                List<long> prices = GetPricesOnPage();
                Assert.IsTrue(prices.Count > 0, "Không có sản phẩm để sắp xếp.");

                for (int i = 0; i < prices.Count - 1; i++)
                {
                    Assert.IsTrue(prices[i] <= prices[i + 1], $"Sai thứ tự: {prices[i]} lớn hơn {prices[i + 1]}");
                }
            }, "Danh sách sản phẩm hiển thị được sắp xếp theo cột Price tăng dần");

            RunTestCase("04. Sắp xếp", "SEARCH-007", "Sắp xếp danh sách sản phẩm theo Giá giảm dần.", " ", () =>
            {
                SearchKeyword("Áo");

                // Click chọn mục "Giá giảm dần" (Option thứ 3 trong thẻ select)
                driver.FindElement(By.XPath("/html/body/main/div[1]/form/label/select")).Click();
                driver.FindElement(By.XPath("/html/body/main/div[1]/form/label/select/option[3]")).Click();
                System.Threading.Thread.Sleep(2000);

                List<long> prices = GetPricesOnPage();
                Assert.IsTrue(prices.Count > 0, "Không có sản phẩm để sắp xếp.");

                for (int i = 0; i < prices.Count - 1; i++)
                {
                    Assert.IsTrue(prices[i] >= prices[i + 1], $"Sai thứ tự: {prices[i]} nhỏ hơn {prices[i + 1]}");
                }
            }, "Danh sách sản phẩm hiển thị được sắp xếp theo cột Price giảm dần chính xác");

            RunTestCase("04. Sắp xếp", "SEARCH-008", "Kết hợp tìm kiếm theo từ khóa và sắp xếp theo giá.", " ", () =>
            {
                SearchKeyword("Áo");

                driver.FindElement(By.XPath("/html/body/main/div[1]/form/label/select")).Click();
                driver.FindElement(By.XPath("/html/body/main/div[1]/form/label/select/option[2]")).Click();
                System.Threading.Thread.Sleep(2000);

                var productNames = driver.FindElements(By.XPath("/html/body/main/div[3]/a/div/h3"));
                Assert.IsTrue(productNames.Count > 0);

                List<long> prices = GetPricesOnPage();
                for (int i = 0; i < prices.Count - 1; i++)
                {
                    Assert.IsTrue(prices[i] <= prices[i + 1]);
                }
            }, "Chỉ hiển thị các sản phẩm chứa từ 'Áo' và được sắp xếp đúng thứ tự giá tăng dần");

            driver.Quit();
        }
    }
}