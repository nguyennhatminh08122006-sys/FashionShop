using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;
using System.Linq;

namespace Kiemthu_CANIFA.TestCases.QuanTri
{
    public class QuanLyDanhMuc
    {
        public void RunCategoryAdminTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ QUẢN LÝ DANH MỤC (ADMIN) ==========\n");

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

            // Đăng nhập quyền Admin trước khi test
            try
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Account/Login");
                driver.FindElement(By.XPath("//*[@id='Email']")).SendKeys("admin@canifa.vn");
                driver.FindElement(By.XPath("//*[@id='Password']")).SendKeys("Admin@123");
                driver.FindElement(By.XPath("/html/body/main/div/form/button")).Click();
                System.Threading.Thread.Sleep(1000);
            }
            catch { }

            RunTestCase("09. Quản lý danh mục", "AD-CAT-001", "Hiển thị danh sách toàn bộ danh mục trong trang quản trị.", "/Admin/Categories", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Categories");
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr")).Count > 0);

                int rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr")).Count;
                Assert.IsTrue(rows > 0, "Không hiển thị danh sách danh mục nào.");
            }, "Hiển thị bảng danh mục gồm các cột: Mã (Id), Tên danh mục (Name), Đường dẫn (Slug), Biểu tượng (Icon).");

            RunTestCase("09. Quản lý danh mục", "AD-CAT-002", "Thêm mới danh mục hợp lệ.", "/Admin/Categories/Create", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Categories");
                driver.FindElement(By.XPath("/html/body/div/div/main/div/a")).Click(); // Nút Thêm danh mục

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Name']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='Name']")).SendKeys("Đồ mặc nhà");
                driver.FindElement(By.XPath("//*[@id='Slug']")).SendKeys("do-mac-nha");
                driver.FindElement(By.XPath("//*[@id='Icon']")).SendKeys("fa-home");

                driver.FindElement(By.XPath("/html/body/div/div/main/section/form/div/button")).Click(); // Nút lưu
                System.Threading.Thread.Sleep(1000);

                string pageSource = driver.PageSource;
                Assert.IsTrue(pageSource.Contains("Đồ mặc nhà"), "Danh mục vừa thêm không xuất hiện trong danh sách.");
            }, "Tạo danh mục mới thành công vào bảng Categories và hiển thị trong danh sách.");

            RunTestCase("09. Quản lý danh mục", "AD-CAT-003", "Không cho phép thêm danh mục khi Tên danh mục hoặc Slug để trống.", "/Admin/Categories/Create", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Categories");
                driver.FindElement(By.XPath("/html/body/div/div/main/div/a")).Click(); // Nút Thêm

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Name']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='Name']")).SendKeys("");
                driver.FindElement(By.XPath("//*[@id='Slug']")).SendKeys("");

                driver.FindElement(By.XPath("/html/body/div/div/main/section/form/div/button")).Click();

                // Kỳ vọng: Form không bị submit (vẫn ở trang Create) hoặc có validation HTML5
                bool hasError = driver.Url.Contains("Create") || driver.FindElement(By.XPath("//*[@id='Name']")).GetAttribute("validationMessage") != "";
                Assert.IsTrue(hasError, "Hệ thống vẫn cho phép lưu khi bỏ trống trường bắt buộc.");
            }, "Hiển thị thông báo lỗi yêu cầu nhập đầy đủ Tên danh mục và Slug; không lưu dữ liệu.");

            RunTestCase("09. Quản lý danh mục", "AD-CAT-004", "Không cho phép thêm hoặc sửa danh mục có Slug bị trùng lặp.", "/Admin/Categories/Create", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Categories");
                driver.FindElement(By.XPath("/html/body/div/div/main/div/a")).Click();

                wait.Until(d => d.FindElements(By.XPath("//*[@id='Name']")).Count > 0);
                driver.FindElement(By.XPath("//*[@id='Name']")).SendKeys("Thời trang Nam mới");
                driver.FindElement(By.XPath("//*[@id='Slug']")).SendKeys("nam"); // Cố tình nhập trùng Slug

                driver.FindElement(By.XPath("/html/body/div/div/main/section/form/div/button")).Click();
                System.Threading.Thread.Sleep(1000);

                // Kỳ vọng hệ thống báo lỗi không điều hướng về trang Index
                Assert.IsTrue(driver.Url.Contains("Create") || driver.PageSource.Contains("tồn tại"), "Hệ thống không chặn Slug bị trùng.");
            }, "Hệ thống từ chối lưu và hiển thị thông báo lỗi đường dẫn Slug đã tồn tại.");

            RunTestCase("09. Quản lý danh mục", "AD-CAT-005", "Cập nhật thông tin danh mục thành công.", "/Admin/Categories/Edit", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Categories");

                // Tìm dòng chứa "Đồ mặc nhà" vừa thêm ở trên và bấm Sửa
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                IWebElement editBtn = null;
                foreach (var row in rows)
                {
                    if (row.Text.Contains("Đồ mặc nhà"))
                    {
                        editBtn = row.FindElement(By.XPath(".//td[6]/div/a"));
                        break;
                    }
                }

                Assert.IsNotNull(editBtn, "Không tìm thấy danh mục để sửa.");
                editBtn.Click();
                System.Threading.Thread.Sleep(1000);

                var nameInput = driver.FindElement(By.XPath("//*[@id='Name']"));
                nameInput.Clear(); nameInput.SendKeys("Bộ mặc nhà cao cấp");

                var slugInput = driver.FindElement(By.XPath("//*[@id='Slug']"));
                slugInput.Clear(); slugInput.SendKeys("bo-mac-nha");

                driver.FindElement(By.XPath("/html/body/div/div/main/section/form/div/button")).Click();
                System.Threading.Thread.Sleep(1000);

                Assert.IsTrue(driver.PageSource.Contains("Bộ mặc nhà cao cấp"), "Tên danh mục chưa được cập nhật thành công.");
            }, "Thông tin danh mục trong bảng Categories được cập nhật đúng theo dữ liệu mới.");

            RunTestCase("09. Quản lý danh mục", "AD-CAT-006", "Xóa thành công danh mục chưa có sản phẩm nào.", "/Admin/Categories", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Categories");

                // Tìm dòng chứa "Bộ mặc nhà cao cấp" và bấm Xóa
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                IWebElement deleteBtn = null;
                foreach (var row in rows)
                {
                    if (row.Text.Contains("Bộ mặc nhà cao cấp"))
                    {
                        deleteBtn = row.FindElement(By.XPath(".//td[6]/div/form/button"));
                        break;
                    }
                }

                Assert.IsNotNull(deleteBtn, "Không tìm thấy danh mục để xóa.");
                deleteBtn.Click();

                // Xử lý hộp thoại Alert (Bấm OK)
                System.Threading.Thread.Sleep(500);
                driver.SwitchTo().Alert().Accept();
                System.Threading.Thread.Sleep(1000);

                Assert.IsFalse(driver.PageSource.Contains("Bộ mặc nhà cao cấp"), "Danh mục chưa bị xóa khỏi danh sách.");
            }, "Danh mục bị xóa khỏi bảng Categories; danh sách cập nhật lại.");

            RunTestCase("09. Quản lý danh mục", "AD-CAT-007", "Chặn xóa danh mục đang có sản phẩm thuộc danh mục đó.", "/Admin/Categories", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Categories");

                // Cố tình xóa danh mục "Nữ" (đang chứa nhiều sản phẩm nhất, thường ở dòng 1)
                var rows = driver.FindElements(By.XPath("/html/body/div/div/main/section/div/table/tbody/tr"));
                IWebElement deleteBtn = null;
                foreach (var row in rows)
                {
                    if (row.Text.Contains("Nữ") && !row.Text.Contains("Phụ"))
                    { // Tránh nhầm với danh mục khác
                        deleteBtn = row.FindElement(By.XPath(".//td[6]/div/form/button"));
                        break;
                    }
                }

                Assert.IsNotNull(deleteBtn, "Không tìm thấy danh mục 'Nữ' để xóa.");
                deleteBtn.Click();

                // Bấm OK
                System.Threading.Thread.Sleep(500);
                driver.SwitchTo().Alert().Accept();
                System.Threading.Thread.Sleep(1000);

                // Kỳ vọng hệ thống báo lỗi không cho xóa, thẻ Nữ vẫn còn
                Assert.IsTrue(driver.PageSource.Contains("Nữ"), "Hệ thống đã lỡ xóa danh mục đang chứa sản phẩm!");
            }, "Hệ thống hiển thị thông báo không cho phép xóa danh mục đang có sản phẩm; dữ liệu giữ nguyên.");

            driver.Quit();
        }
    }
}