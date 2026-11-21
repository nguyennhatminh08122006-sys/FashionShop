

// 26/10
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;

namespace Kiemthu_CANIFA.TestCases.KhachHang
{
    public class AuthTests
    {
        public void RunRegisterTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ ĐĂNG KÝ ==========\n");

            // Hàm hỗ trợ in định dạng 
            void RunTestCase(string method, string module, string id, string desc, string url, Action testAction, params string[] passMsgs)
            {
                Console.WriteLine($"{method} {module} > {id} | {desc}");
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

            void NavigateToRegister()
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Account/Register");
                wait.Until(d => d.FindElement(By.XPath("//*[@id='FullName']")));
            }

            RunTestCase(" ", "01. Đăng ký", "REG-001", "Kiểm tra hiển thị đầy đủ biểu mẫu đăng ký.", " ", () =>
            {
                NavigateToRegister();
                Assert.IsTrue(driver.FindElement(By.XPath("//*[@id='FullName']")).Displayed);
            }, "Không trả lỗi 5xx", "Trang đăng ký tải thành công", "Có đủ trường nhập liệu");
            RunTestCase(" ", "01. Đăng ký", "REG-002", "Đăng ký tài khoản khách hàng thành công với dữ liệu hợp lệ.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='FullName']")).SendKeys("Nguyễn Văn A");
                // Sử dụng DateTime.Now.Ticks để tạo email động, tránh lỗi trùng lặp khi chạy test nhiều lần
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("user_new_" + DateTime.Now.Ticks + "@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Phone']")).SendKeys("0907654321");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Test@123");
                driver.FindElement(By.XPath("//*[@id='ConfirmPassword']")).SendKeys("Test@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();

                // Đợi hệ thống chuyển hướng (thường là về trang Login hoặc trang chủ sau khi đăng ký thành công)
                wait.Until(d => d.Url.Contains("Login") || d.Url == "http://localhost:5000/");
                Assert.IsTrue(driver.Url.Contains("Login") || driver.Url == "http://localhost:5000/");
            }, "Không trả lỗi 5xx", "Tạo tài khoản thành công", "Chuyển hướng chính xác");

            RunTestCase(" ", "01. Đăng ký", "REG-003", "Không cho phép đăng ký khi để trống trường Họ tên.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("test@canifa.vn");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                string err = wait.Until(d => d.FindElement(By.XPath("/html/body/main/div/form/span[1]"))).Text;
                Assert.IsNotEmpty(err);
            }, "Không trả lỗi 5xx", "Từ chối khi họ tên trống", "Hiển thị cảnh báo hợp lệ");

            RunTestCase(" ", "01. Đăng ký", "REG-004", "Không cho phép đăng ký khi Email sai định dạng.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='FullName']")).SendKeys("Nguyễn Văn A");
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("email_sai_dinh_dang");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                string err = wait.Until(d => d.FindElement(By.XPath("/html/body/main/div/form/span[2]"))).Text;
                Assert.IsNotEmpty(err);
            }, "Không trả lỗi 5xx", "Từ chối email sai định dạng", "Báo lỗi đúng thẻ span");

            RunTestCase(" ", "01. Đăng ký", "REG-005", "Không cho phép đăng ký khi Email đã tồn tại.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='FullName']")).SendKeys("Khách Demo");
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("demo@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Test@123");
                driver.FindElement(By.XPath("//*[@id='ConfirmPassword']")).SendKeys("Test@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                string err = wait.Until(d => d.FindElement(By.XPath("/html/body/main/div/form/span[2]"))).Text;
                Assert.IsNotEmpty(err);
            }, "Không trả lỗi 5xx", "Chặn email trùng lặp");

            RunTestCase(" ", "01. Đăng ký", "REG-006", "Kiểm tra ràng buộc Số điện thoại.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='FullName']")).SendKeys("Nguyễn Văn B");
                driver.FindElement(By.XPath("//*[@id='Phone']")).SendKeys("0987");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                string err = wait.Until(d => d.FindElement(By.XPath("/html/body/main/div/form/span[3]"))).Text;
                Assert.IsNotEmpty(err);
            }, "Từ chối SĐT không đủ số", "Hiển thị thông báo lỗi SĐT");

            RunTestCase(" ", "01. Đăng ký", "REG-007", "Kiểm tra giá trị biên Mật khẩu dưới 6 ký tự.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='FullName']")).SendKeys("Trần C");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("12345");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                string err = wait.Until(d => d.FindElement(By.XPath("/html/body/main/div/form/span[4]"))).Text;
                Assert.IsNotEmpty(err);
            }, "Từ chối mật khẩu < 6 ký tự");

            RunTestCase(" ", "01. Đăng ký", "REG-009", "Nhập lại mật khẩu không khớp.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Test@123");
                driver.FindElement(By.XPath("//*[@id='ConfirmPassword']")).SendKeys("Test@456");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                string err = wait.Until(d => d.FindElement(By.XPath("/html/body/main/div/form/span[5]"))).Text;
                Assert.IsNotEmpty(err);
            }, "Xác nhận hai mật khẩu không khớp");

            RunTestCase(" ", "01. Đăng ký", "REG-008", "Kiểm tra giá trị biên Mật khẩu đúng bằng 6 ký tự.", " ", () =>
            {
                NavigateToRegister();
                driver.FindElement(By.XPath("//*[@id='FullName']")).SendKeys("Trịnh Tiến Sơn");
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("khach" + DateTime.Now.Ticks + "@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Phone']")).SendKeys("0912345678");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("123456");
                driver.FindElement(By.XPath("//*[@id='ConfirmPassword']")).SendKeys("123456");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                wait.Until(d => d.Url.Contains("Login"));
                Assert.IsTrue(driver.Url.Contains("Login"));
            }, "Không trả lỗi 5xx", "Tạo tài khoản thành công", "Chuyển hướng về trang Đăng nhập");

            driver.Quit();
        }
    }
}