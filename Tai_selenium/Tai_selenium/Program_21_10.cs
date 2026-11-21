//using Kiemthu_CANIFA.TestCases.Admin;
using Kiemthu_CANIFA.TestCases.KhachHang; // Gọi đến thư mục chứa file Test
using Kiemthu_CANIFA.TestCases.QuanTri;
using System;
using System.Text;

namespace Kiemthu_CANIFA
{
    class Program_21_10
    {
        static void Main(string[] args)
        {
            // Hỗ trợ in tiếng Việt có dấu ra màn hình Console
            Console.OutputEncoding = Encoding.UTF8;

            // dki tai khoan 
            //AuthTests authTest = new AuthTests();
            //authTest.RunRegisterTests();

            //// dang nhap dang xuat.
            //LoginTests loginTest = new LoginTests();
            //loginTest.RunLoginTests();

            //// 3. Chạy phần Xem Sản Phẩm Theo Danh Mục
            //XemDanhMuc categoryTest = new XemDanhMuc();
            //categoryTest.RunCategoryTests();

            ////4. Chạy phần Tìm kiếm và Sắp xếp
            //TimKiem_SapXep searchTest = new TimKiem_SapXep();
            //searchTest.RunSearchTests();

            //// Chạy phần Giỏ hàng
            //GioHang cartTest = new GioHang();
            //cartTest.RunCartTests();

            //// Chạy phần Đặt hàng
            //OrderTests orderTest = new OrderTests();
            //orderTest.RunOrderTests();

            // Chạy phần Quản lý Danh mục (Admin)
            //QuanLyDanhMuc adminCatTest = new QuanLyDanhMuc();
            //adminCatTest.RunCategoryAdminTests();

            // Quan ly san pham 
            //QuanLySanPham adminPrdTest = new QuanLySanPham();
            //adminPrdTest.RunProductAdminTests();

            //Quan ly tai khoan 
            //QuanLyTaiKhoan adminUsrTest = new QuanLyTaiKhoan();
            //adminUsrTest.RunUserAdminTests();

            // quan ly gio  hang 
            QuanLyGioHang adminCartTest = new QuanLyGioHang();
            adminCartTest.RunCartAdminTests();

            // quan ly don hang 
            //QuanLyDonHang adminOrderTest = new QuanLyDonHang();
            //adminOrderTest.RunOrderAdminTests();

            Console.WriteLine("Nhấn phím Enter để thoát...");
            Console.ReadLine();
        }
    }
}