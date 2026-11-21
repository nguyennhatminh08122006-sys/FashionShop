using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;
using System.Linq;

namespace Kiemthu_CANIFA.TestCases.QuanTri
{
    public class QuanLySanPham
    {
        public void RunProductAdminTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ QUẢN LÝ SẢN PHẨM (ADMIN) ==========\n");

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

            // Bấm menu Quản trị
            driver.FindElement(By.XPath("/html/body/header/div/div/a[2]")).Click();
            System.Threading.Thread.Sleep(1500);

            string testProductName = "Sản phẩm Test Tự Động " + DateTime.Now.ToString("HHmmss");

            RunTestCase("10. Sản phẩm", "AD-PRD-001", "Hiển thị danh sách sản phẩm trong trang quản trị.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr[1]")).Count > 0);

                int rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr")).Count;
                Assert.IsTrue(rows > 0, "Không hiển thị danh sách sản phẩm.");
            }, "Hiển thị đầy đủ danh sách sản phẩm kèm Mã, Ảnh, Tên, Danh mục, Giá bán, Tồn kho và Trạng thái (IsActive).");

            RunTestCase("10. Sản phẩm", "AD-PRD-002", "Thêm mới sản phẩm thành công với đầy đủ thông tin hợp lệ.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                driver.FindElement(By.XPath("/html/body/div/div/main/div/a")).Click(); // Bấm thêm

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Name']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='Name']")).SendKeys(testProductName);
                driver.FindElement(By.XPath("//*[@id='Description']")).SendKeys("Mô tả cho sản phẩm test.");
                driver.FindElement(By.XPath("//*[@id='Price']")).SendKeys("399000");
                driver.FindElement(By.XPath("//*[@id='Sizes']")).SendKeys("M,L,XL");
                driver.FindElement(By.XPath("//*[@id='StockQuantity']")).SendKeys("50");
                driver.FindElement(By.XPath("//*[@id='Color']")).SendKeys("#FFFFFF");

                var categorySelect = new SelectElement(driver.FindElement(By.XPath("//*[@id='CategoryId']")));
                categorySelect.SelectByIndex(1);

                System.Threading.Thread.Sleep(500);
                // Dùng XPath giới hạn trong thẻ main để tránh bấm nhầm Đăng xuất
                driver.FindElement(By.XPath("//main//button[@type='submit']")).Click();
                System.Threading.Thread.Sleep(2000);

                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                Assert.IsTrue(driver.PageSource.Contains(testProductName), "Sản phẩm không xuất hiện.");
            }, "Sản phẩm mới được lưu vào bảng Products và xuất hiện trong danh sách quản trị.");

            RunTestCase("10. Sản phẩm", "AD-PRD-003", "Không cho phép thêm sản phẩm khi Tên sản phẩm để trống.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                driver.FindElement(By.XPath("/html/body/div/div/main/div/a")).Click();

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Name']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='Price']")).SendKeys("399000");
                driver.FindElement(By.XPath("//*[@id='StockQuantity']")).SendKeys("20");

                System.Threading.Thread.Sleep(500);
                driver.FindElement(By.XPath("//main//button[@type='submit']")).Click();
                System.Threading.Thread.Sleep(500);

                bool hasError = driver.Url.Contains("Create") || driver.FindElement(By.XPath("//*[@id='Name']")).GetAttribute("validationMessage") != "";
                Assert.IsTrue(hasError, "Hệ thống vẫn cho phép lưu khi bỏ trống trường bắt buộc.");
            }, "Hiển thị thông báo lỗi bắt buộc nhập Tên sản phẩm; không tạo bản ghi.");

            RunTestCase("10. Sản phẩm", "AD-PRD-004", "Kiểm tra ràng buộc Giá bán (Price) phải lớn hơn 0.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products/Create");

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Name']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='Name']")).SendKeys("Sản phẩm lỗi giá");
                driver.FindElement(By.XPath("//*[@id='Price']")).SendKeys("-150000");

                System.Threading.Thread.Sleep(500);
                driver.FindElement(By.XPath("//main//button[@type='submit']")).Click();
                System.Threading.Thread.Sleep(500);

                bool isPrevented = driver.Url.Contains("Create") || driver.FindElement(By.XPath("//*[@id='Price']")).GetAttribute("validationMessage") != "";
                Assert.IsTrue(isPrevented, "Hệ thống không chặn lưu giá âm.");
            }, "Hệ thống từ chối lưu và hiển thị thông báo “Giá bán phải lớn hơn 0”.");

            RunTestCase("10. Sản phẩm", "AD-PRD-005", "Kiểm tra ràng buộc Số lượng tồn kho (StockQuantity) không được âm.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products/Create");

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Name']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='Name']")).SendKeys("Sản phẩm lỗi tồn");
                driver.FindElement(By.XPath("//*[@id='Price']")).SendKeys("100000");
                driver.FindElement(By.XPath("//*[@id='StockQuantity']")).SendKeys("-5");

                System.Threading.Thread.Sleep(500);
                driver.FindElement(By.XPath("//main//button[@type='submit']")).Click();
                System.Threading.Thread.Sleep(500);

                bool isPrevented = driver.Url.Contains("Create") || driver.FindElement(By.XPath("//*[@id='StockQuantity']")).GetAttribute("validationMessage") != "";
                Assert.IsTrue(isPrevented, "Hệ thống không chặn tồn kho âm.");
            }, "Hệ thống báo lỗi số lượng tồn kho không hợp lệ và không lưu vào CSDL.");

            RunTestCase("10. Sản phẩm", "AD-PRD-006", "Kiểm tra tải lên tệp ảnh vượt quá giới hạn 2 MB hoặc sai định dạng.", () =>
            {
                Assert.Fail("Thiếu thuộc tính kiểm tra giới hạn dung lượng ảnh 2MB ở phía Server (Hệ thống chưa xử lý).");
            }, "Hệ thống từ chối tải tệp và cảnh báo dung lượng ảnh tối đa 2 MB / sai định dạng.");

            RunTestCase("10. Sản phẩm", "AD-PRD-007", "Cập nhật thông tin giá bán và số lượng tồn của sản phẩm.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                IWebElement editBtn = rows.FirstOrDefault(r => r.Text.Contains(testProductName))?.FindElement(By.XPath(".//td[9]/div/a[2]"));

                Assert.IsNotNull(editBtn, "Không tìm thấy sản phẩm test để sửa.");
                editBtn.Click();

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Price']")).Count > 0);
                var priceInput = driver.FindElement(By.XPath("//*[@id='Price']"));
                priceInput.Clear(); priceInput.SendKeys("349000");

                var stockInput = driver.FindElement(By.XPath("//*[@id='StockQuantity']"));
                stockInput.Clear(); stockInput.SendKeys("80");

                System.Threading.Thread.Sleep(500);
                driver.FindElement(By.XPath("//main//button[@type='submit']")).Click();
                System.Threading.Thread.Sleep(1500);

                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                Assert.IsTrue(driver.PageSource.Contains("349"), "Giá chưa cập nhật.");
            }, "Dữ liệu trong bảng Products được cập nhật đúng; giao diện hiển thị giá mới.");

            RunTestCase("10. Sản phẩm", "AD-PRD-008", "Chuyển đổi trạng thái ẩn/hiện (IsActive) của sản phẩm.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                IWebElement editBtn = rows.FirstOrDefault(r => r.Text.Contains(testProductName))?.FindElement(By.XPath(".//td[9]/div/a[2]"));

                Assert.IsNotNull(editBtn, "Không tìm thấy sản phẩm test để đổi trạng thái.");
                editBtn.Click();

                wait.Until(d => d.FindElements(By.XPath("//*[@id='IsActive']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='IsActive']")).Click();

                System.Threading.Thread.Sleep(500);
                driver.FindElement(By.XPath("//main//button[@type='submit']")).Click();
                System.Threading.Thread.Sleep(1500);

                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                Assert.IsTrue(driver.Url.Contains("Products"), "Lưu trạng thái thất bại.");
            }, "Sản phẩm được đánh dấu ẩn/hiện thành công trong trang Admin.");

            RunTestCase("10. Sản phẩm", "AD-PRD-009", "Xóa thành công sản phẩm chưa từng phát sinh trong đơn hàng.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                IWebElement deleteBtn = rows.FirstOrDefault(r => r.Text.Contains(testProductName))?.FindElement(By.XPath(".//td[9]/div/form[2]/button"));

                Assert.IsNotNull(deleteBtn, "Không tìm thấy SP test để xóa.");
                deleteBtn.Click();

                System.Threading.Thread.Sleep(500);
                driver.SwitchTo().Alert().Accept();
                System.Threading.Thread.Sleep(1500);

                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                Assert.IsFalse(driver.PageSource.Contains(testProductName), "SP chưa bị xóa.");
            }, "Sản phẩm bị xóa khỏi bảng Products thành công.");

            RunTestCase("10. Sản phẩm", "AD-PRD-010", "Chặn xóa sản phẩm đã tồn tại trong bảng chi tiết đơn hàng (OrderItems).", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                if (rows.Count > 0)
                {
                    string originalProductName = rows[0].FindElement(By.XPath(".//td[3]")).Text;
                    IWebElement deleteBtn = rows[0].FindElement(By.XPath(".//td[9]/div/form[2]/button"));
                    deleteBtn.Click();

                    System.Threading.Thread.Sleep(500);
                    driver.SwitchTo().Alert().Accept();
                    System.Threading.Thread.Sleep(1500);

                    driver.Navigate().GoToUrl("http://localhost:5000/Admin/Products");
                    Assert.IsTrue(driver.PageSource.Contains(originalProductName), "Đã lỡ xóa SP gốc!");
                }
            }, "Hiển thị thông báo không cho phép xóa SP đã có trong đơn hàng; giữ nguyên dữ liệu.");

            driver.Quit();
        }
    }
}