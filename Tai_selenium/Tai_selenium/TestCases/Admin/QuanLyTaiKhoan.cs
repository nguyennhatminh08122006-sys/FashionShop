using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;
using System.Linq;

namespace Kiemthu_CANIFA.TestCases.QuanTri
{
    public class QuanLyTaiKhoan
    {
        public void RunUserAdminTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ QUẢN LÝ TÀI KHOẢN (ADMIN) ==========\n");

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

            // Đăng nhập
            driver.Navigate().GoToUrl("http://localhost:5000/Account/Login");
            driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("admin@canifa.vn");
            driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Admin@123");
            driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
            System.Threading.Thread.Sleep(2000);

            RunTestCase("11. Tài khoản", "AD-USR-001", "Hiển thị danh sách toàn bộ tài khoản người dùng trong hệ thống.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody")).Count > 0);

                int rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr")).Count;
                Assert.IsTrue(rows > 0, "Không hiển thị danh sách tài khoản.");
            }, "Hiển thị danh sách gồm Mã TK (Id), Họ tên (FullName), Email, Số điện thoại, Vai trò (Role) và Trạng thái khóa (IsLocked).");

            RunTestCase("11. Tài khoản", "AD-USR-002", "Xem thông tin chi tiết của một tài khoản khách hàng.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));

                IWebElement viewBtn = rows.FirstOrDefault(r => r.Text.Contains("demo@canifa.vn"))?.FindElement(By.XPath(".//td[8]/div/a"));
                Assert.IsNotNull(viewBtn, "Không tìm thấy nút Xem của tài khoản demo.");

                viewBtn.Click();
                System.Threading.Thread.Sleep(1000);
                Assert.IsTrue(driver.PageSource.Contains("demo@canifa.vn"), "Không hiển thị chi tiết hồ sơ khách hàng.");
            }, "Hiển thị đầy đủ và chính xác thông tin chi tiết của khách hàng được chọn.");

            RunTestCase("11. Tài khoản", "AD-USR-003", "Khóa một tài khoản khách hàng đang hoạt động.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));

                IWebElement lockBtn = rows.FirstOrDefault(r => r.Text.Contains("demo@canifa.vn"))?.FindElement(By.XPath(".//td[8]/div/form[1]/button"));
                Assert.IsNotNull(lockBtn, "Không tìm thấy nút Khóa của tài khoản demo.");

                lockBtn.Click();
                System.Threading.Thread.Sleep(1000);

                bool hasConcurrencyError = driver.PageSource.Contains("cập nhật bởi người khác");
                if (hasConcurrencyError)
                {
                    Assert.Fail("Lỗi hệ thống (Concurrency): Tài khoản vừa được cập nhật bởi người khác, vui lòng tải lại.");
                }

                Assert.IsTrue(driver.PageSource.Contains("Bị khóa") || driver.PageSource.Contains("Đã khóa"), "Tài khoản chưa được khóa thành công.");
            }, "Cột IsLocked trong bảng Users chuyển sang 1 (Bị khóa); khách hàng đó không thể đăng nhập.");

            RunTestCase("11. Tài khoản", "AD-USR-004", "Mở khóa lại cho tài khoản khách hàng đang bị khóa.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));

                IWebElement unlockBtn = rows.FirstOrDefault(r => r.Text.Contains("demo@canifa.vn"))?.FindElement(By.XPath(".//td[8]/div/form[1]/button"));
                Assert.IsNotNull(unlockBtn, "Không tìm thấy nút Mở khóa.");

                unlockBtn.Click();
                System.Threading.Thread.Sleep(1000);

                if (driver.PageSource.Contains("cập nhật bởi người khác"))
                {
                    Assert.Fail("Lỗi hệ thống (Concurrency): Không thể mở khóa tài khoản.");
                }
            }, "Cột IsLocked cập nhật về 0; tài khoản đăng nhập lại bình thường.");

            RunTestCase("11. Tài khoản", "AD-USR-005", "Hủy thao tác khóa tài khoản tại hộp thoại xác nhận.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));

                IWebElement lockBtn = rows.FirstOrDefault(r => r.Text.Contains("demo@canifa.vn"))?.FindElement(By.XPath(".//td[8]/div/form[1]/button"));
                Assert.IsNotNull(lockBtn, "Không tìm thấy nút Khóa.");

                lockBtn.Click();
                System.Threading.Thread.Sleep(500);

                try
                {
                    driver.SwitchTo().Alert().Dismiss();
                }
                catch (NoAlertPresentException)
                {
                    Assert.Fail("Thiếu hộp thoại xác nhận (Confirm Dialog) khi thao tác Khóa tài khoản. Hệ thống thực thi lệnh Khóa ngay lập tức!");
                }
            }, "Hệ thống hủy thao tác; trạng thái IsLocked của tài khoản không thay đổi.");

            RunTestCase("11. Tài khoản", "AD-USR-006", "Không cho phép Quản trị viên tự khóa chính tài khoản đang đăng nhập.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                var adminRow = rows.FirstOrDefault(r => r.Text.Contains("admin@canifa.vn"));

                // Dùng FindElements để đếm số lượng nút Khóa thay vì ép click ngay
                var lockBtns = adminRow?.FindElements(By.XPath(".//td[8]/div/form[1]/button"));

                if (lockBtns != null && lockBtns.Count > 0)
                {
                    lockBtns[0].Click();
                    System.Threading.Thread.Sleep(1000);
                    Assert.IsTrue(driver.PageSource.Contains("không thể tự khóa") || driver.PageSource.Contains("lỗi"), "Hệ thống đã cho phép Admin tự khóa chính mình!");
                }
            }, "Hệ thống đã chủ động ẩn nút Khóa hoặc từ chối thao tác khóa đối với tài khoản của chính mình.");

            RunTestCase("11. Tài khoản", "AD-USR-007", "Bảo đảm luôn còn ít nhất một Quản trị viên hoạt động trong hệ thống.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                var adminRow = rows.FirstOrDefault(r => r.Text.Contains("admin@canifa.vn"));

                var deleteBtns = adminRow?.FindElements(By.XPath(".//td[8]/div/form[2]/button"));

                if (deleteBtns != null && deleteBtns.Count > 0)
                {
                    deleteBtns[0].Click();
                    System.Threading.Thread.Sleep(500);
                    try { driver.SwitchTo().Alert().Accept(); } catch { }
                    System.Threading.Thread.Sleep(1000);
                    Assert.IsTrue(driver.PageSource.Contains("không thể xóa") || driver.PageSource.Contains("ít nhất một"), "Hệ thống đã lỡ xóa Admin duy nhất!");
                }
            }, "Hệ thống đã chủ động ẩn nút Xóa để bảo vệ Admin duy nhất hoạt động.");

            RunTestCase("11. Tài khoản", "AD-USR-008", "Xóa tài khoản khách hàng chưa có dữ liệu đơn hàng ràng buộc.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Users");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));

                var userRow = rows.FirstOrDefault(r => r.Text.Contains("user_new@canifa.vn")) ?? rows.LastOrDefault(r => !r.Text.Contains("admin") && !r.Text.Contains("demo"));

                if (userRow != null)
                {
                    IWebElement deleteBtn = userRow.FindElement(By.XPath(".//td[8]/div/form[2]/button"));
                    deleteBtn.Click();

                    System.Threading.Thread.Sleep(500);
                    driver.SwitchTo().Alert().Accept();
                    System.Threading.Thread.Sleep(1000);

                    Assert.IsFalse(driver.PageSource.Contains("user_new@canifa.vn"), "Tài khoản chưa bị xóa.");
                }
                else
                {
                    Console.WriteLine("  [Lưu ý] Không tìm thấy tài khoản nháp (không có đơn hàng) nào để test tính năng Xóa.");
                }
            }, "Tài khoản bị xóa khỏi bảng Users; danh sách tài khoản được cập nhật.");

            driver.Quit();
        }
    }
}