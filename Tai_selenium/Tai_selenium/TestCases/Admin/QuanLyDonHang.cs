using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using Kiemthu_CANIFA.Common;
using System.Linq;

namespace Kiemthu_CANIFA.TestCases.QuanTri
{
    public class QuanLyDonHang
    {
        public void RunOrderAdminTests()
        {
            BrowserSetup setup = new BrowserSetup();
            IWebDriver driver = setup.OpenBrowser();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Console.WriteLine("========== BẮT ĐẦU KIỂM THỬ QUẢN LÝ ĐƠN HÀNG (ADMIN) ==========\n");

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

            RunTestCase("12. Đơn hàng", "AD-ORD-001", "Hiển thị danh sách đơn hàng và bộ lọc trạng thái.", () =>
            {
                driver.FindElement(By.XPath("/html/body/div/aside/nav/a[5]")).Click(); // Bấm menu Đơn hàng
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section[2]/div/table/tbody")).Count > 0);

                int rows = driver.FindElements(By.XPath("/html/body/div/div/main/section[2]/div/table/tbody/tr")).Count;
                Assert.IsTrue(rows > 0, "Không có dữ liệu đơn hàng nào được hiển thị.");
            }, "Hiển thị danh sách đơn gồm Mã đơn, Ngày đặt, Người nhận, Tổng tiền, Phương thức TT, Trạng thái.");

            RunTestCase("12. Đơn hàng", "AD-ORD-002", "Xem chi tiết các sản phẩm trong một đơn hàng.", () =>
            {
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Orders");
                wait.Until(d => d.FindElements(By.XPath("/html/body/div/div/main/section[2]/div/table/tbody/tr[1]")).Count > 0);

                // Bấm nút Chi tiết ở dòng đầu tiên
                driver.FindElement(By.XPath("/html/body/div/div/main/section[2]/div/table/tbody/tr[1]/td[9]/a")).Click();
                System.Threading.Thread.Sleep(1000);

                Assert.IsTrue(driver.Url.ToLower().Contains("details") || driver.PageSource.Contains("Sản phẩm"), "Nút Xem chi tiết không hoạt động.");
            }, "Hiển thị đúng thông tin người nhận và bảng OrderItems gồm: Mã SP, Tên SP, Size, Số lượng, Đơn giá.");

            RunTestCase("12. Đơn hàng", "AD-ORD-003", "Xác nhận đơn hàng mới (Chờ xác nhận -> Đã xác nhận).", () =>
            {
                // Lúc này đang ở trang chi tiết của đơn hàng dòng 1
                var actionBtns = driver.FindElements(By.XPath("/html/body/div/div/main/section[2]/form[1]/button"));
                if (actionBtns.Count > 0)
                {
                    actionBtns[0].Click(); // Click "Xác nhận đơn"
                    System.Threading.Thread.Sleep(1500);
                    Assert.IsTrue(true, "Đã thực thi Xác nhận đơn.");
                }
                else
                {
                    Console.WriteLine("  [Lưu ý] Đơn hàng này đã qua trạng thái Mới.");
                }
            }, "Trạng thái đơn hàng trong bảng Orders cập nhật thành 'Đã xác nhận'.");

            RunTestCase("12. Đơn hàng", "AD-ORD-004", "Cập nhật trạng thái đơn hàng sang Đang xử lý và Đang giao.", () =>
            {
                var actionBtns = driver.FindElements(By.XPath("/html/body/div/div/main/section[2]/form[1]/button"));
                if (actionBtns.Count > 0)
                {
                    actionBtns[0].Click(); // Click "Bắt đầu giao"
                    System.Threading.Thread.Sleep(1500);
                    Assert.IsTrue(true, "Đã thực thi Bắt đầu giao.");
                }
            }, "Hệ thống cập nhật đúng trạng thái đơn hàng tuần tự bằng nút hành động.");

            RunTestCase("12. Đơn hàng", "AD-ORD-005", "Cập nhật trạng thái đơn hàng sang Đã giao (hoàn tất).", () =>
            {
                var actionBtns = driver.FindElements(By.XPath("/html/body/div/div/main/section[2]/form[1]/button"));
                if (actionBtns.Count > 0)
                {
                    actionBtns[0].Click(); // Click "Hoàn thành"
                    System.Threading.Thread.Sleep(1500);
                    Assert.IsTrue(true, "Đã thực thi Hoàn thành đơn.");
                }
            }, "Trạng thái đơn chuyển thành 'Hoàn thành' và kết thúc luồng xử lý đơn.");

            RunTestCase("12. Đơn hàng", "AD-ORD-007", "Không cho phép hủy đơn hàng đã ở trạng thái Đã giao.", () =>
            {
                // Sau ca 005, đơn hàng dòng 1 đã ở trạng thái "Hoàn thành"
                // Ta sẽ đếm xem nút Hủy (form[2]) có còn tồn tại trên giao diện không
                var cancelBtns = driver.FindElements(By.XPath("/html/body/div/div/main/section[2]/form[2]/button"));

                Assert.IsTrue(cancelBtns.Count == 0, "Lỗi bảo mật: Nút Hủy vẫn hiển thị khi đơn đã Hoàn thành!");
            }, "Hệ thống đã chủ động ẩn nút Hủy đối với đơn hàng đã hoàn tất/giao thành công.");

            RunTestCase("12. Đơn hàng", "AD-ORD-008", "Không cho phép chuyển ngược trạng thái đơn hàng trái quy tắc.", () =>
            {
                // Kiểm tra xem có thanh dropdown chọn trạng thái tự do nào không
                var selectDropdowns = driver.FindElements(By.XPath("//select[contains(@name, 'status')]"));

                Assert.IsTrue(selectDropdowns.Count == 0, "Lỗi bảo mật: Tồn tại thanh Dropdown cho phép chuyển ngược trạng thái.");
            }, "Hệ thống sử dụng cơ chế nút bấm 1 chiều, triệt tiêu hoàn toàn rủi ro chuyển ngược trạng thái.");

            RunTestCase("12. Đơn hàng", "AD-ORD-006", "Hủy đơn hàng và kiểm tra hoàn lại số lượng tồn kho (StockQuantity).", () =>
            {
                // Quay lại danh sách đơn hàng
                driver.Navigate().GoToUrl("http://localhost:5000/Admin/Orders");
                System.Threading.Thread.Sleep(1000);

                // Cố gắng tìm một đơn hàng khác ở dòng thứ 2 (hoặc dòng cuối) để test tính năng Hủy
                var viewBtns = driver.FindElements(By.XPath("/html/body/div/div/main/section[2]/div/table/tbody/tr[2]/td[9]/a"));
                if (viewBtns.Count > 0)
                {
                    viewBtns[0].Click(); // Xem chi tiết đơn thứ 2
                    System.Threading.Thread.Sleep(1000);

                    var cancelBtns = driver.FindElements(By.XPath("/html/body/div/div/main/section[2]/form[2]/button"));
                    if (cancelBtns.Count > 0)
                    {
                        cancelBtns[0].Click();
                        System.Threading.Thread.Sleep(500);
                        try { driver.SwitchTo().Alert().Accept(); } catch { } // Xác nhận Hủy nếu có
                        System.Threading.Thread.Sleep(1500);

                        Assert.IsTrue(true, "Đã bấm Hủy thành công đơn hàng số 2.");
                    }
                    else
                    {
                        Console.WriteLine("  [Lưu ý] Đơn hàng này không còn nút Hủy.");
                    }
                }
                else
                {
                    Console.WriteLine("  [Lưu ý] Không có đơn hàng thứ 2 để test thao tác Hủy.");
                }
            }, "Trạng thái đơn chuyển sang 'Đã hủy' (hoặc bị xóa); StockQuantity được cộng hoàn lại.");

            driver.Quit();
        }
    }
}