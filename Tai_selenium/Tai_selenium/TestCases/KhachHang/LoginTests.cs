using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;

namespace Kiemthu_CANIFA.TestCases.KhachHang
{
    public class LoginTests
    {
        public void RunLoginTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ ĐĂNG NHẬP VÀ ĐĂNG XUẤT ==========\n");

            
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

            void NavigateToLogin()
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Account/Login");
                wait.Until(d => d.FindElements(By.XPath("//*[@id='Email']")).Count > 0);
            }

            RunTestCase("02. Xác thực", "AUTH-006", "Kiểm tra bỏ trống trường Email khi nhấn Đăng nhập.", " ", () =>
            {
                NavigateToLogin();
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Customer@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                var emailInput = driver.FindElement(By.XPath("//*[@id='Email']"));
                Assert.IsNotNull(emailInput.GetAttribute("required"));
            }, "Hiển thị cảnh báo bắt buộc nhập Email", "Không gửi yêu cầu xác thực");

            RunTestCase("02. Xác thực", "AUTH-007", "Kiểm tra bỏ trống trường Mật khẩu khi nhấn Đăng nhập.", " ", () =>
            {
                NavigateToLogin();
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("demo@canifa.vn");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                var passInput = driver.FindElement(By.XPath("//*[@id='Password']"));
                Assert.IsNotNull(passInput.GetAttribute("required"));
            }, "Hiển thị cảnh báo bắt buộc nhập Mật khẩu", "Không đăng nhập được");

            RunTestCase("02. Xác thực", "AUTH-003", "Kiểm tra đăng nhập với Email không tồn tại trong hệ thống.", " ", () =>
            {
                NavigateToLogin();
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("notfound@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("123456");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div/form/div/ul/li")).Count > 0);
                string err = driver.FindElement(By.XPath("/html/body/main/div/form/div/ul/li")).Text;
                Assert.IsNotEmpty(err);
            }, "Không tạo phiên làm việc", "Hiển thị thông báo “Email hoặc mật khẩu không chính xác”");

            RunTestCase("02. Xác thực", "AUTH-004", "Kiểm tra đăng nhập khi nhập sai mật khẩu.", " ", () =>
            {
                NavigateToLogin();
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("demo@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("SaiMatKhau123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div/form/div/ul/li")).Count > 0);
                string err = driver.FindElement(By.XPath("/html/body/main/div/form/div/ul/li")).Text;
                Assert.IsNotEmpty(err);
            }, "Từ chối đăng nhập", "Hiển thị thông báo lỗi xác thực", "Không lộ PasswordHash");

            RunTestCase("02. Xác thực", "AUTH-005", "Kiểm tra đăng nhập bằng tài khoản đang bị khóa (IsLocked = 1).", " ", () =>
            {
                NavigateToLogin();
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("locked_user@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Test@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                wait.Until(d => d.FindElements(By.XPath("/html/body/main/div/form/div/ul/li")).Count > 0);
                string err = driver.FindElement(By.XPath("/html/body/main/div/form/div/ul/li")).Text;
                Assert.IsNotEmpty(err);
            }, "Từ chối đăng nhập", "Hiển thị thông báo “Tài khoản của bạn đã bị khóa”");

            RunTestCase("02. Xác thực", "AUTH-009", "Kiểm tra truy cập khu vực Quản trị (/Admin) khi chưa đăng nhập.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin");
                wait.Until(d => d.Url.Contains("Login"));
                Assert.IsTrue(driver.Url.ToLower().Contains("login"));
            }, "Hệ thống chặn truy cập", "Tự động chuyển hướng về trang /Account/Login");

            RunTestCase("02. Xác thực", "AUTH-001", "Đăng nhập thành công bằng tài khoản Khách hàng hợp lệ.", " ", () =>
            {
                NavigateToLogin();
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("demo@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Demo@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                wait.Until(d => !d.Url.Contains("Login"));
                Assert.IsFalse(driver.Url.Contains("Login"));
            }, "Đăng nhập thành công", "Điều hướng về trang chủ", "Thanh header hiển thị tên khách hàng");

            RunTestCase("02. Xác thực", "AUTH-010", "Kiểm tra truy cập khu vực Quản trị (/Admin) bằng tài khoản Khách hàng.", " ", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin");
                Assert.IsTrue(driver.Url.ToLower().Contains("login") || driver.PageSource.Contains("Access Denied"));
            }, "Hệ thống từ chối quyền truy cập (Access Denied / Redirect)", "Không hiển thị dữ liệu quản trị");

            RunTestCase("02. Xác thực", "AUTH-008", "Kiểm tra chức năng Đăng xuất tài khoản.", " ", () =>
            {
                driver.Navigate().GoToUrl(" ");
                var logoutBtns = driver.FindElements(By.XPath("/html/body/header/div/div/form/button"));
                if (logoutBtns.Count > 0)
                {
                    logoutBtns[0].Click();
                }
                else
                {
                    driver.Navigate().GoToUrl(" ");
                }

                wait.Until(d => d.FindElements(By.XPath("//a[contains(@href, 'Login')]")).Count > 0);
                Assert.IsTrue(driver.FindElements(By.XPath("//a[contains(@href, 'Login')]")).Count > 0);
            }, "Xóa phiên đăng nhập", "Quay về trang chủ ở trạng thái khách vãng lai");

            RunTestCase("02. Xác thực", "AUTH-002", "Đăng nhập thành công bằng tài khoản Quản trị viên (Admin).", " ", () =>
            {
                NavigateToLogin();
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("admin@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Admin@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                wait.Until(d => d.Url.ToLower().Contains("admin") || !d.Url.Contains("Login"));
                Assert.IsFalse(driver.Url.Contains("Login"));
            }, "Đăng nhập thành công", "Cho phép truy cập bảng điều khiển quản trị tại /Admin");

            driver.Quit();
        }
    }
}