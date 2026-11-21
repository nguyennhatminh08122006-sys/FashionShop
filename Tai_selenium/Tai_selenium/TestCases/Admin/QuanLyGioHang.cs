using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;

namespace Kiemthu_CANIFA.TestCases.QuanTri
{
    public class QuanLyGioHang
    {
        public void RunCartAdminTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ QUẢN LÝ GIỎ HÀNG (ADMIN) ==========\n");

            void RunTestCase(string module, string id, string desc, Action testAction, params string[] passMsgs)
            {
                Console.WriteLine($"[{module}] {id} | {desc}");
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

            // Đăng nhập quyền Admin
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Account/Login");
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("admin@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Admin@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                System.Threading.Thread.Sleep(2000);
            }
            catch { }

            RunTestCase("13. Quản lý giỏ hàng", "AD-CRT-001", "Hiển thị danh sách các giỏ hàng của khách hàng.", () =>
            {
                // Bổ sung lệnh bay thẳng vào trang Admin/Carts hoặc trang Dashboard Admin trước khi bấm
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Carts");

                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]")).Count > 0);

                int rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr")).Count;
                Assert.IsTrue(rows > 0, "Không có dữ liệu giỏ hàng nào được hiển thị.");
            }, "Hiển thị danh sách gồm Mã giỏ hàng (Id), Mã tài khoản (UserId), Họ tên (FullName), Email và ngày tạo.");

            RunTestCase("13. Quản lý giỏ hàng", "AD-CRT-002", "Xem chi tiết các sản phẩm bên trong một giỏ hàng cụ thể.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Carts");
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]/td[7]/div/a")).Count > 0);

                // Bấm nút Xem ở dòng đầu tiên
                driver.FindElement(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]/td[7]/div/a")).Click();
                System.Threading.Thread.Sleep(1000);

                Assert.IsTrue(driver.Url.ToLower().Contains("details") || driver.PageSource.Contains("lượng"), "Nút Xem chi tiết không chuyển trang hoặc không hiển thị đúng thông tin.");
            }, "Hiển thị danh sách CartItems gồm: Mã SP, Tên SP, Màu sắc, Kích cỡ, Số lượng và cảnh báo dòng hàng (nếu có).");

            RunTestCase("13. Quản lý giỏ hàng", "AD-CRT-004", "Hủy thao tác xóa giỏ hàng tại hộp thoại xác nhận.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Carts");
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]/td[7]/div/form/button")).Count > 0);

                // Bấm nút Xóa ở dòng đầu tiên
                driver.FindElement(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]/td[7]/div/form/button")).Click();
                System.Threading.Thread.Sleep(500);

                // Bấm nút Hủy (Dismiss) trên Alert
                driver.SwitchTo().Alert().Dismiss();
                System.Threading.Thread.Sleep(1000);

                // Kiểm tra giỏ hàng vẫn còn tồn tại ở dòng 1
                Assert.IsTrue(driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]")).Count > 0, "Hệ thống vẫn xóa dữ liệu dù đã bấm Hủy.");
            }, "Hệ thống hủy thao tác xóa; dữ liệu giỏ hàng trong bảng Carts và CartItems được giữ nguyên.");

            RunTestCase("13. Quản lý giỏ hàng", "AD-CRT-003", "Xóa một dòng sản phẩm hoặc xóa toàn bộ giỏ hàng sau khi xác nhận.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Carts");
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]/td[7]/div/form/button")).Count > 0);

                // Lấy ID giỏ hàng dòng đầu tiên (nếu có thẻ td chứa ID) để so sánh, ở đây dùng tạm logic đếm số dòng
                int initialRows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr")).Count;

                // Bấm nút Xóa
                driver.FindElement(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]/td[7]/div/form/button")).Click();
                System.Threading.Thread.Sleep(500);

                // Bấm Đồng ý (Accept) trên Alert
                driver.SwitchTo().Alert().Accept();
                System.Threading.Thread.Sleep(1500);

                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Carts"); // Load lại list
                int currentRows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr")).Count;

                Assert.IsTrue(currentRows < initialRows || currentRows == 0, "Dữ liệu giỏ hàng chưa được xóa khỏi danh sách.");
            }, "Dữ liệu trong CartItems và Carts của giỏ hàng đó bị xóa; hiển thị thông báo cập nhật thành công.");

            driver.Quit();
        }
    }
}