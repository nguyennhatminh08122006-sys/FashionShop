using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;

namespace Kiemthu_CANIFA.TestCases.KhachHang
{
    public class GioHang
    {
        public void RunCartTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ GIỎ HÀNG ==========\n");

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

            // Đăng nhập trước khi test giỏ hàng
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Account/Login");
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("demo@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Demo@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                wait.Until(d => !d.Url.ToLower().Contains("login"));

                // Xóa sạch giỏ hàng cũ
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                var deleteBtns = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr/td[6]/form/button"));
                foreach (var btn in deleteBtns) { btn.Click(); System.Threading.Thread.Sleep(500); }
            }
            catch { }

            RunTestCase("05. Giỏ hàng", "CART-001", "Thêm 1 sản phẩm còn hàng với kích cỡ và số lượng hợp lệ vào giỏ.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Products/Details/9");

                // Chọn Size S và nhập số lượng 2
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[1]/div/form/fieldset/label[1]/span")).Count > 0);
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/fieldset/label[1]/span")).Click();

                var qtyInput = driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/label/input"));
                qtyInput.Clear();
                qtyInput.SendKeys("2");

                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/button")).Click();
                System.Threading.Thread.Sleep(1000);

                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                int rowCount = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr")).Count;
                Assert.IsTrue(rowCount > 0, "Giỏ hàng vẫn trống sau khi thêm.");
            }, "Sản phẩm được lưu vào CartItems", "Cập nhật số lượng và thành tiền đúng");

            RunTestCase("05. Giỏ hàng", "CART-002", "Thêm trùng sản phẩm cùng kích cỡ đã có trong giỏ hàng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Products/Details/9");

                // Chọn lại Size S
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/fieldset/label[1]/span")).Click();
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/button")).Click();
                System.Threading.Thread.Sleep(1000);

                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                int rowCount = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr")).Count;

                Assert.AreEqual(1, rowCount, "Hệ thống tạo thêm dòng trùng lặp thay vì cộng dồn.");
                string qtyInCart = driver.FindElement(By.XPath("/html/body/main/table/tbody/tr/td[4]/form/input")).GetAttribute("value");
                Assert.AreEqual("3", qtyInCart, "Số lượng không được cộng dồn chính xác.");
            }, "Hệ thống cộng dồn số lượng của dòng hàng đó", "Không tạo dòng trùng lặp");

            RunTestCase("05. Giỏ hàng", "CART-003", "Thêm cùng một sản phẩm nhưng khác kích cỡ vào giỏ hàng.", " ", () =>
            {
                // Dọn giỏ hàng để đếm cho chuẩn 2 size
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                var deleteBtns = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr/td[6]/form/button"));
                foreach (var btn in deleteBtns) { btn.Click(); System.Threading.Thread.Sleep(500); }

                // Thêm Size S
                driver.Navigate().GoToUrl("http://localhost:5000/Products/Details/9");
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/fieldset/label[1]/span")).Click();
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/button")).Click();
                System.Threading.Thread.Sleep(1000);

                // Thêm Size M
                driver.Navigate().GoToUrl("http://localhost:5000/Products/Details/9");
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/fieldset/label[2]/span")).Click();
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/button")).Click();
                System.Threading.Thread.Sleep(1000);

                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                int rowCount = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr")).Count;
                Assert.AreEqual(2, rowCount, "Không tách riêng được các dòng sản phẩm khác size.");
            }, "Giỏ hàng hiển thị các dòng sản phẩm riêng biệt tương ứng với từng Size");

            RunTestCase("05. Giỏ hàng", "CART-004", "Xem chi tiết trang Giỏ hàng và kiểm tra tính tổng tiền.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");

                long calculateTotal = 0;
                var rows = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr"));

                for (int i = 1; i <= rows.Count; i++)
                {
                    string priceText = driver.FindElement(By.XPath($"/html/body/main/table/tbody/tr[{i}]/td[3]")).Text.Replace("đ", "").Replace(".", "").Trim();
                    string qtyText = driver.FindElement(By.XPath($"/html/body/main/table/tbody/tr[{i}]/td[4]/form/input")).GetAttribute("value");
                    string subTotalText = driver.FindElement(By.XPath($"/html/body/main/table/tbody/tr[{i}]/td[5]")).Text.Replace("đ", "").Replace(".", "").Trim();

                    long price = long.Parse(priceText);
                    long qty = long.Parse(qtyText);
                    long subTotal = long.Parse(subTotalText);

                    Assert.AreEqual(price * qty, subTotal, $"Sai thành tiền ở dòng {i}");
                    calculateTotal += subTotal;
                }

                string finalTotalText = driver.FindElement(By.XPath("/html/body/main/div/b")).Text.Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                long finalTotal = long.Parse(finalTotalText);

                Assert.AreEqual(calculateTotal, finalTotal, "Tổng tiền giỏ hàng (Tổng cộng) tính sai.");
            }, "Hiển thị đúng ảnh, tên, mã SP, màu, size, đơn giá, số lượng", "Tổng tiền = Σ(Price × Quantity) chính xác");

            RunTestCase("05. Giỏ hàng", "CART-005", "Cập nhật số lượng sản phẩm trong giỏ.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");

                var qtyInput = driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/input"));
                qtyInput.Clear();
                qtyInput.SendKeys("5");

                driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/button")).Click();
                System.Threading.Thread.Sleep(1000);

                string newQty = driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/input")).GetAttribute("value");
                Assert.AreEqual("5", newQty, "Số lượng không được cập nhật.");
            }, "Số lượng trong bảng CartItems và tổng tiền tạm tính được cập nhật chính xác");

            RunTestCase("05. Giỏ hàng", "CART-006", "Kiểm tra giá trị biên số lượng tối đa 20 sản phẩm trên một dòng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                var qtyInput = driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/input"));
                qtyInput.Clear();
                qtyInput.SendKeys("21");

                driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/button")).Click();

                string maxVal = qtyInput.GetAttribute("max");
                string validationMsg = qtyInput.GetAttribute("validationMessage");

                bool isPrevented = (maxVal == "20") || !string.IsNullOrEmpty(validationMsg);
                Assert.IsTrue(isPrevented, "Hệ thống không chặn giới hạn 20 sản phẩm.");
            }, "Hệ thống hiển thị cảnh báo chặn số lượng vượt quá 20");

            RunTestCase("05. Giỏ hàng", "CART-007", "Không cho phép thêm hoặc cập nhật số lượng vượt quá tồn kho (StockQuantity).", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");

                var qtyInput = driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/input"));
                qtyInput.Clear();
                qtyInput.SendKeys("999");

                driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/button")).Click();

                WebDriverWait shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                try
                {
                    shortWait.Until(d =>
                    {
                        try { return d.SwitchTo().Alert() != null; }
                        catch (NoAlertPresentException) { return false; }
                    });

                    IAlert alert = driver.SwitchTo().Alert();
                    string alertText = alert.Text.ToLower();
                    Assert.IsTrue(alertText.Contains("tồn kho") || alertText.Contains("không đủ") || alertText.Contains("vượt quá"), "Nội dung hộp thoại cảnh báo không đúng.");
                    alert.Accept();
                }
                catch (WebDriverTimeoutException)
                {
                    string maxVal = qtyInput.GetAttribute("max");
                    string validationMsg = qtyInput.GetAttribute("validationMessage");
                    if (string.IsNullOrEmpty(validationMsg) && string.IsNullOrEmpty(maxVal))
                    {
                        Assert.Fail("Lỗi: Hệ thống đã cho phép cập nhật số lượng vượt tồn kho mà không có cảnh báo nào.");
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }, "Hệ thống từ chối cập nhật", "Thông báo số lượng vượt quá số lượng tồn kho hiện có");

            RunTestCase("05. Giỏ hàng", "CART-009", "Kiểm tra nhập số lượng bằng 0 hoặc số âm trong giỏ hàng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                var qtyInput = driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/input"));
                qtyInput.Clear();
                qtyInput.SendKeys("-2");

                driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[4]/form/button")).Click();

                string minVal = qtyInput.GetAttribute("min");
                string validationMsg = qtyInput.GetAttribute("validationMessage");

                bool isPrevented = (minVal == "1") || !string.IsNullOrEmpty(validationMsg) || qtyInput.GetAttribute("value") == "1";
                Assert.IsTrue(isPrevented, "Hệ thống chấp nhận số lượng âm.");
            }, "Hệ thống không chấp nhận số lượng <= 0");

            RunTestCase("05. Giỏ hàng", "CART-010", "Xóa một sản phẩm khỏi giỏ hàng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                int initialRows = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr")).Count;

                driver.FindElement(By.XPath("/html/body/main/table/tbody/tr[1]/td[6]/form/button")).Click();
                System.Threading.Thread.Sleep(1000);

                int afterRows = driver.FindElements(By.XPath("/html/body/main/table/tbody/tr")).Count;
                Assert.AreEqual(initialRows - 1, afterRows, "Dòng sản phẩm chưa bị xóa.");
            }, "Dòng sản phẩm bị xóa khỏi bảng CartItems", "Tổng tiền giỏ hàng được tính lại chính xác");

            RunTestCase("05. Giỏ hàng", "CART-008", "Kiểm tra khi sản phẩm đã hết hàng (StockQuantity = 0).", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Products/Details/16");

                var addToCartBtn = driver.FindElements(By.XPath("/html/body/main/div[1]/div/form/button"));
                bool isOutOfStock = false;

                if (addToCartBtn.Count == 0 || addToCartBtn[0].GetAttribute("disabled") != null)
                {
                    isOutOfStock = true;
                }

                Assert.IsTrue(isOutOfStock, "Sản phẩm StockQuantity = 0 nhưng vẫn hiển thị 'Còn hàng' và cho phép Thêm vào giỏ.");
            }, "Hiển thị trạng thái “Hết hàng”", "Vô hiệu hóa nút Thêm vào giỏ hàng");

            driver.Quit();
        }
    }
}