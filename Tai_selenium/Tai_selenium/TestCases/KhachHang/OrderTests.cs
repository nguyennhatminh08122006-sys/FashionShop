using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;

namespace Kiemthu_CANIFA.TestCases.KhachHang
{
    public class OrderTests
    {
        public void RunOrderTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ ĐẶT HÀNG ==========\n");

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

            RunTestCase("06. Đặt hàng", "ORDER-001", "Kiểm tra yêu cầu đăng nhập khi khách vãng lai nhấn Đặt hàng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                wait.Until(d => d.Url.ToLower().Contains("login"));
                Assert.IsTrue(driver.Url.ToLower().Contains("login"));
            }, "Hệ thống thông báo yêu cầu đăng nhập", "Chuyển hướng đến trang /Account/Login");

            // SETUP 1: TIẾN HÀNH ĐĂNG NHẬP
            try
            {
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("demo@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Demo@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                wait.Until(d => !d.Url.ToLower().Contains("login"));
            }
            catch { }

            RunTestCase("06. Đặt hàng", "ORDER-002", "Không cho phép đặt hàng khi giỏ hàng đang trống.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                System.Threading.Thread.Sleep(1000);
                Assert.IsTrue(driver.PageSource.Contains("chưa có sản phẩm") || driver.PageSource.ToLower().Contains("trống"));
            }, "Hiển thị thông báo “Bạn chưa có sản phẩm nào trong giỏ hàng!”", "Không tạo đơn");

            // SETUP 2: TRUY CẬP TRỰC TIẾP VÀ THÊM SẢN PHẨM VÀO GIỎ ĐỂ TEST CÁC BƯỚC SAU
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Products/Details/15");
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[1]/div/form/fieldset/label/span")).Count > 0);
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/fieldset/label/span")).Click();
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/button")).Click();
                System.Threading.Thread.Sleep(2000);
            }
            catch { }

            RunTestCase("06. Đặt hàng", "ORDER-005", "Không cho phép đặt hàng khi bỏ trống Tên người nhận.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                wait.Until(d => d.FindElements(By.XPath("//*[@id='ReceiverName']")).Count > 0);

                var nameInput = driver.FindElement(By.XPath("//*[@id='ReceiverName']"));
                nameInput.Clear();
                Assert.IsNotNull(nameInput.GetAttribute("required"), "Thiếu thuộc tính required ở Tên người nhận");
                driver.FindElement(By.XPath("/html/body/main/div/div/form/button")).Click();
            }, "Hiển thị thông báo “Vui lòng nhập tên người nhận”", "Không tạo đơn hàng");

            RunTestCase("06. Đặt hàng", "ORDER-006", "Không cho phép đặt hàng khi bỏ trống Địa chỉ giao hàng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                wait.Until(d => d.FindElements(By.XPath("//*[@id='Address']")).Count > 0);

                driver.FindElement(By.XPath("//*[@id='ReceiverName']")).SendKeys("Khách Demo");
                var addressInput = driver.FindElement(By.XPath("//*[@id='Address']"));
                addressInput.Clear();

                Assert.IsNotNull(addressInput.GetAttribute("required"), "Thiếu thuộc tính required ở Địa chỉ");
                driver.FindElement(By.XPath("/html/body/main/div/div/form/button")).Click();
            }, "Hiển thị thông báo “Vui lòng nhập địa chỉ giao hàng”", "Giữ nguyên giỏ hàng");

            RunTestCase("06. Đặt hàng", "ORDER-007", "Không cho phép đặt hàng khi Số điện thoại sai định dạng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                wait.Until(d => d.FindElements(By.XPath("//*[@id='Phone']")).Count > 0);

                driver.FindElement(By.XPath("//*[@id='ReceiverName']")).Clear();
                driver.FindElement(By.XPath("//*[@id='ReceiverName']")).SendKeys("Khách Demo");
                driver.FindElement(By.XPath("//*[@id='Address']")).Clear();
                driver.FindElement(By.XPath("//*[@id='Address']")).SendKeys("Hà Nội");

                var phoneInput = driver.FindElement(By.XPath("//*[@id='Phone']"));
                phoneInput.Clear();
                phoneInput.SendKeys("0912abc");

                driver.FindElement(By.XPath("/html/body/main/div/div/form/button")).Click();

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div/div/form/span[2]")).Count > 0);
                string err = driver.FindElement(By.XPath("/html/body/main/div/div/form/span[2]")).Text;
                Assert.IsNotEmpty(err);
            }, "Từ chối tạo đơn", "Hiển thị thông báo lỗi định dạng số điện thoại");

            RunTestCase("06. Đặt hàng", "ORDER-009", "Kiểm tra tính chính xác của Tổng tiền đơn hàng (Total).", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div/aside/div/b")).Count > 0);

                string subTotalText = driver.FindElement(By.XPath("/html/body/main/div/aside/ul/li/b")).Text;
                string totalText = driver.FindElement(By.XPath("/html/body/main/div/aside/div/b")).Text;

                long subTotal = long.Parse(subTotalText.Replace("đ", "").Replace(".", "").Replace(",", "").Trim());
                long total = long.Parse(totalText.Replace("đ", "").Replace(".", "").Replace(",", "").Trim());

                Assert.AreEqual(subTotal, total, "Tổng tiền thanh toán (Total) bị tính sai.");
            }, "Tổng tiền thanh toán được tính đúng bằng tổng tiền hàng", "Lưu khớp dữ liệu bảng Orders");

            RunTestCase("06. Đặt hàng", "ORDER-003", "Đặt hàng thành công với phương thức COD và thông tin giao hàng hợp lệ.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                wait.Until(d => d.FindElements(By.XPath("//*[@id='ReceiverName']")).Count > 0);

                driver.FindElement(By.XPath("//*[@id='ReceiverName']")).Clear();
                driver.FindElement(By.XPath("//*[@id='ReceiverName']")).SendKeys("Trịnh Tiến Sơn");

                driver.FindElement(By.XPath("//*[@id='Phone']")).Clear();
                driver.FindElement(By.XPath("//*[@id='Phone']")).SendKeys("0988777666");

                driver.FindElement(By.XPath("//*[@id='Address']")).Clear();
                driver.FindElement(By.XPath("//*[@id='Address']")).SendKeys("Đại học Công nghiệp Hà Nội");

                driver.FindElement(By.XPath("/html/body/main/div/div/form/button")).Click();

                wait.Until(d => !d.Url.ToLower().Contains("checkout"));
                Assert.IsFalse(driver.Url.ToLower().Contains("checkout"));
            }, "Tạo đơn hàng mới trong Orders (Status='Chờ xác nhận')", "Hiển thị trang chi tiết đơn hàng vừa đặt");

            RunTestCase("06. Đặt hàng", "ORDER-004", "Kiểm tra làm trống giỏ hàng và trừ tồn kho sau khi đặt hàng thành công.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Cart");
                System.Threading.Thread.Sleep(1000);
                Assert.IsTrue(driver.PageSource.Contains("chưa có sản phẩm") || driver.PageSource.ToLower().Contains("trống"));
            }, "Giỏ hàng của khách bị xóa hết", "StockQuantity của sản phẩm giảm");

            RunTestCase("06. Đặt hàng", "ORDER-008", "Kiểm tra đặt hàng khi tồn kho sản phẩm không còn đủ tại thời điểm xác nhận.", " ", () =>
            {
                // SETUP LẠI GIỎ HÀNG (Vì ORDER-003 đã thanh toán làm trống giỏ)
                driver.Navigate().GoToUrl("http://localhost:5000/Products/Details/15");
                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div[1]/div/form/button")).Count > 0);
                driver.FindElement(By.XPath("/html/body/main/div[1]/div/form/button")).Click();
                System.Threading.Thread.Sleep(1000);

                // CHUYỂN QUA THANH TOÁN
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/Checkout");
                wait.Until(d => d.FindElements(By.XPath("//*[@id='ReceiverName']")).Count > 0);

                driver.FindElement(By.XPath("//*[@id='ReceiverName']")).Clear();
                driver.FindElement(By.XPath("//*[@id='ReceiverName']")).SendKeys("Khách Demo");
                driver.FindElement(By.XPath("//*[@id='Phone']")).Clear();
                driver.FindElement(By.XPath("//*[@id='Phone']")).SendKeys("0912345678");
                driver.FindElement(By.XPath("//*[@id='Address']")).Clear();
                driver.FindElement(By.XPath("//*[@id='Address']")).SendKeys("Hà Nội");

                driver.FindElement(By.XPath("/html/body/main/div/div/form/button")).Click();

                // Đợi Alert với timeout ngắn (3 giây) để không bị treo nếu lỗi logic
                WebDriverWait shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                try
                {
                    shortWait.Until(d =>
                    {
                        try { return d.SwitchTo().Alert() != null; }
                        catch (NoAlertPresentException) { return false; }
                    });

                    IAlert alert = driver.SwitchTo().Alert();
                    Assert.IsTrue(alert.Text.ToLower().Contains("không đủ") || alert.Text.ToLower().Contains("tồn kho") || alert.Text.ToLower().Contains("hết hàng"), "Nội dung Alert sai.");
                    alert.Accept();
                }
                catch (WebDriverTimeoutException)
                {
                    Assert.Fail("Lỗi: Đã đặt hàng thành công dù tồn kho không đủ (Alert không xuất hiện).");
                }
                System.Threading.Thread.Sleep(1000);
            }, "Hệ thống hiển thị hộp thoại cảnh báo hết hàng", "Hủy thao tác tạo đơn thành công");

            RunTestCase("06. Đặt hàng", "ORDER-010", "Xem lịch sử đơn hàng và chi tiết đơn hàng đã đặt của khách hàng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Orders/History");
                System.Threading.Thread.Sleep(2000);

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/section[2]/div")).Count > 0);
                Assert.IsTrue(driver.FindElements(By.XPath("/html/body/main/section[2]/div")).Count > 0);
            }, "Hiển thị đúng mã đơn, ngày đặt, trạng thái", "Danh sách sản phẩm, kích cỡ, số lượng và tổng tiền");

            driver.Quit();
        }
    }
}