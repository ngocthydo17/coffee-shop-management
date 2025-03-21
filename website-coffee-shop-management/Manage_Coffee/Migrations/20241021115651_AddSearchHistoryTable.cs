using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manage_Coffee.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "SignUpUserModel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "SignUpUserModel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ChangePasswordModel",
                columns: table => new
                {
                    CurrentPassword = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NewPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmNewPassword = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangePasswordModel", x => x.CurrentPassword);
                });

            migrationBuilder.CreateTable(
                name: "ChiNhanh",
                columns: table => new
                {
                    MaCN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Diachi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChiNhanh__27258E0E20D82064", x => x.MaCN);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucKM",
                columns: table => new
                {
                    MaKM = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GiaTri = table.Column<int>(type: "int", nullable: false),
                    Soluong = table.Column<int>(type: "int", nullable: false),
                    Hanmuc = table.Column<int>(type: "int", nullable: false),
                    Ngayapdung = table.Column<DateTime>(type: "date", nullable: false),
                    Ngayhethan = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhMucK__2725CF155B7D31EE", x => x.MaKM);
                });

            migrationBuilder.CreateTable(
                name: "ForgotPassword",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgotPassword", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKH = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Diachi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Matkhau = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SDT = table.Column<int>(type: "int", nullable: true),
                    Xu = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    GioiTinh = table.Column<bool>(type: "bit", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KhachHan__2725CF1E3B146E27", x => x.MaKH);
                });

            migrationBuilder.CreateTable(
                name: "Loai",
                columns: table => new
                {
                    Maloai = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Loai__3E1DB46D4D7258A9", x => x.Maloai);
                });

            migrationBuilder.CreateTable(
                name: "NguyenVatLieu",
                columns: table => new
                {
                    MaNVL = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Dongia = table.Column<int>(type: "int", nullable: false),
                    DVT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Mota = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Anh = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    SoLuong = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NguyenVa__3A197864454B6A99", x => x.MaNVL);
                });

            migrationBuilder.CreateTable(
                name: "Nhacungcap",
                columns: table => new
                {
                    MaCcap = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Diachi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SDT = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Nhacungc__1A1475A84B56D97C", x => x.MaCcap);
                });

            migrationBuilder.CreateTable(
                name: "ResetPasswordModel",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmNewPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPasswordModel", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SearchHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keyword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SearchCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SignInModel",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RememberMe = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignInModel", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "Size",
                columns: table => new
                {
                    MaSize = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TriGia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Size__A787E7ED45B00A2A", x => x.MaSize);
                });

            migrationBuilder.CreateTable(
                name: "Slider",
                columns: table => new
                {
                    MaSL = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Anh = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Mota = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Trangthai = table.Column<bool>(type: "bit", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Slider__2725CF1E3B146E27", x => x.MaSL);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNV = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sdt = table.Column<int>(type: "int", nullable: false),
                    Mkhau = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Chucvu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Diachi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GioiTinh = table.Column<bool>(type: "bit", unicode: false, nullable: true),
                    Ngaysinh = table.Column<DateTime>(type: "date", nullable: false),
                    MaCN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NhanVien__2725D70A2E229495", x => x.MaNV);
                    table.ForeignKey(
                        name: "FK__NhanVien__MaCN__3D5E1FD2",
                        column: x => x.MaCN,
                        principalTable: "ChiNhanh",
                        principalColumn: "MaCN");
                });

            migrationBuilder.CreateTable(
                name: "Phieudhonl",
                columns: table => new
                {
                    MaPhieuonl = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    Ngaygiodat = table.Column<DateTime>(type: "datetime", nullable: false),
                    TongTien = table.Column<int>(type: "int", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: true),
                    PTTT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TienShip = table.Column<int>(type: "int", nullable: true),
                    PTNH = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaKH = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    MaCN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaKM = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Phieudho__144F3C04508AE3D5", x => x.MaPhieuonl);
                    table.ForeignKey(
                        name: "FK__Phieudhonl__MaCN__49C3F6B7",
                        column: x => x.MaCN,
                        principalTable: "ChiNhanh",
                        principalColumn: "MaCN");
                    table.ForeignKey(
                        name: "FK__Phieudhonl__MaKH__48CFD27E",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH");
                    table.ForeignKey(
                        name: "FK__Phieudhonl__MaKM__4AB81AF0",
                        column: x => x.MaKM,
                        principalTable: "DanhMucKM",
                        principalColumn: "MaKM");
                });

            migrationBuilder.CreateTable(
                name: "SanPham",
                columns: table => new
                {
                    MaSP = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Dongia = table.Column<int>(type: "int", nullable: false),
                    DVT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MOTA = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Anh = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", unicode: false, nullable: false),
                    Maloai = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    MaTopping = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SanPham__2725081CCC8EB8FA", x => x.MaSP);
                    table.ForeignKey(
                        name: "FK__SanPham__MaToppi__5629CD9C",
                        column: x => x.MaTopping,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                    table.ForeignKey(
                        name: "FK__SanPham__Maloai__5535A963",
                        column: x => x.Maloai,
                        principalTable: "Loai",
                        principalColumn: "Maloai");
                });

            migrationBuilder.CreateTable(
                name: "DanhMucCa",
                columns: table => new
                {
                    Ngay = table.Column<DateTime>(type: "date", nullable: false),
                    MaNV = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Calam = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhMucC__D9BEBAC2B9F2395D", x => new { x.Ngay, x.MaNV });
                    table.ForeignKey(
                        name: "FK__DanhMucCa__MaNV__4222D4EF",
                        column: x => x.MaNV,
                        principalTable: "NhanVien",
                        principalColumn: "MaNV");
                });

            migrationBuilder.CreateTable(
                name: "PhieuNhapXuat",
                columns: table => new
                {
                    MaPhieu = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    NgayLap = table.Column<DateTime>(type: "date", nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Diachi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaNV = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaCcap = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhieuNha__2660BFE0404E71E8", x => x.MaPhieu);
                    table.ForeignKey(
                        name: "FK__PhieuNhapX__MaNV__59063A47",
                        column: x => x.MaNV,
                        principalTable: "NhanVien",
                        principalColumn: "MaNV");
                    table.ForeignKey(
                        name: "FK__PhieuNhap__MaCca__59FA5E80",
                        column: x => x.MaCcap,
                        principalTable: "Nhacungcap",
                        principalColumn: "MaCcap");
                });

            migrationBuilder.CreateTable(
                name: "PhieuOrder",
                columns: table => new
                {
                    MaOrder = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    Ngaygiodat = table.Column<DateTime>(type: "datetime", nullable: false),
                    Soban = table.Column<int>(type: "int", nullable: false),
                    Tongtien = table.Column<int>(type: "int", nullable: false),
                    Trangthai = table.Column<bool>(type: "bit", nullable: true),
                    PTTT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sdt = table.Column<int>(type: "int", nullable: true),
                    MaCN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaNV = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    MaKM = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhieuOrd__50559EF73AF128AD", x => x.MaOrder);
                    table.ForeignKey(
                        name: "FK__PhieuOrder__MaCN__5070F446",
                        column: x => x.MaCN,
                        principalTable: "ChiNhanh",
                        principalColumn: "MaCN");
                    table.ForeignKey(
                        name: "FK__PhieuOrder__MaKM__52593CB8",
                        column: x => x.MaKM,
                        principalTable: "DanhMucKM",
                        principalColumn: "MaKM");
                    table.ForeignKey(
                        name: "FK__PhieuOrder__MaNV__5165187F",
                        column: x => x.MaNV,
                        principalTable: "NhanVien",
                        principalColumn: "MaNV");
                });

            migrationBuilder.CreateTable(
                name: "BOM",
                columns: table => new
                {
                    MaSP = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaNVL = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    SoLuong = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BOM__74849F9A3EE903C7", x => new { x.MaSP, x.MaNVL });
                    table.ForeignKey(
                        name: "FK__BOM__MaNVL__628FA481",
                        column: x => x.MaNVL,
                        principalTable: "NguyenVatLieu",
                        principalColumn: "MaNVL");
                    table.ForeignKey(
                        name: "FK__BOM__MaSP__619B8048",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateTable(
                name: "ChitietDanhGia",
                columns: table => new
                {
                    MaSP = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaKH = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NhanXet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChitietD__D55754ED97C2B4EC", x => new { x.MaSP, x.MaKH });
                    table.ForeignKey(
                        name: "FK__ChitietDan__MaKH__66603565",
                        column: x => x.MaKH,
                        principalTable: "KhachHang",
                        principalColumn: "MaKH");
                    table.ForeignKey(
                        name: "FK__ChitietDan__MaSP__656C112C",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateTable(
                name: "CTSPonl",
                columns: table => new
                {
                    MaSP = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaPhieuonl = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    MaKH = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    MaSize = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Soluong = table.Column<int>(type: "int", nullable: false),
                    Gia = table.Column<int>(type: "int", nullable: false),
                    Tongtien = table.Column<int>(type: "int", nullable: false),
                    Ghichu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CTSPonl__6B340E67A2D88F1E", x => new { x.MaSP, x.MaPhieuonl, x.MaSize, x.MaKH });
                    table.ForeignKey(
                        name: "FK__CTSPonl__MaPhieu__6E01572D",
                        column: x => x.MaPhieuonl,
                        principalTable: "Phieudhonl",
                        principalColumn: "MaPhieuonl");
                    table.ForeignKey(
                        name: "FK__CTSPonl__MaSP__6D0D32F4",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                    table.ForeignKey(
                        name: "FK__CTSPonl__MaSize__6EF57B66",
                        column: x => x.MaSize,
                        principalTable: "Size",
                        principalColumn: "MaSize");
                });

            migrationBuilder.CreateTable(
                name: "KIT",
                columns: table => new
                {
                    MaKIT = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaSP = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    GiaKit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KIT__MaSp_MaKit", x => new { x.MaSP, x.MaKIT });
                    table.ForeignKey(
                        name: "FK__KIT__MaKIT__628FA481",
                        column: x => x.MaKIT,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                    table.ForeignKey(
                        name: "FK__KIT__MaSP__619B8048",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateTable(
                name: "CTPhieu",
                columns: table => new
                {
                    MaNVL = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaPhieu = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Soluong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CTPhieu__287F739A30E43D33", x => new { x.MaNVL, x.MaPhieu });
                    table.ForeignKey(
                        name: "FK__CTPhieu__MaNVL__693CA210",
                        column: x => x.MaNVL,
                        principalTable: "NguyenVatLieu",
                        principalColumn: "MaNVL");
                    table.ForeignKey(
                        name: "FK__CTPhieu__MaPhieu__6A30C649",
                        column: x => x.MaPhieu,
                        principalTable: "PhieuNhapXuat",
                        principalColumn: "MaPhieu");
                });

            migrationBuilder.CreateTable(
                name: "CTSanPham",
                columns: table => new
                {
                    MaOrder = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    MaSP = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MaKH = table.Column<string>(type: "char(50)", unicode: false, fixedLength: true, maxLength: 50, nullable: false),
                    MaSize = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    Soluong = table.Column<int>(type: "int", nullable: false),
                    Gia = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<int>(type: "int", nullable: false),
                    Ghichu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CTSanPha__E3CA93C7BB55FCEE", x => new { x.MaOrder, x.MaSP, x.MaKH, x.MaSize });
                    table.ForeignKey(
                        name: "FK__CTSanPham__MaOrd__5CD6CB2B",
                        column: x => x.MaOrder,
                        principalTable: "PhieuOrder",
                        principalColumn: "MaOrder");
                    table.ForeignKey(
                        name: "FK__CTSanPham__MaSP__5DCAEF64",
                        column: x => x.MaSP,
                        principalTable: "SanPham",
                        principalColumn: "MaSP");
                    table.ForeignKey(
                        name: "FK__CTSanPham__MaSiz__5EBF139D",
                        column: x => x.MaSize,
                        principalTable: "Size",
                        principalColumn: "MaSize");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BOM_MaNVL",
                table: "BOM",
                column: "MaNVL");

            migrationBuilder.CreateIndex(
                name: "IX_ChitietDanhGia_MaKH",
                table: "ChitietDanhGia",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_CTPhieu_MaPhieu",
                table: "CTPhieu",
                column: "MaPhieu");

            migrationBuilder.CreateIndex(
                name: "IX_CTSanPham_MaSize",
                table: "CTSanPham",
                column: "MaSize");

            migrationBuilder.CreateIndex(
                name: "IX_CTSanPham_MaSP",
                table: "CTSanPham",
                column: "MaSP");

            migrationBuilder.CreateIndex(
                name: "IX_CTSPonl_MaPhieuonl",
                table: "CTSPonl",
                column: "MaPhieuonl");

            migrationBuilder.CreateIndex(
                name: "IX_CTSPonl_MaSize",
                table: "CTSPonl",
                column: "MaSize");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucCa_MaNV",
                table: "DanhMucCa",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_KIT_MaKIT",
                table: "KIT",
                column: "MaKIT");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaCN",
                table: "NhanVien",
                column: "MaCN");

            migrationBuilder.CreateIndex(
                name: "IX_Phieudhonl_MaCN",
                table: "Phieudhonl",
                column: "MaCN");

            migrationBuilder.CreateIndex(
                name: "IX_Phieudhonl_MaKH",
                table: "Phieudhonl",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_Phieudhonl_MaKM",
                table: "Phieudhonl",
                column: "MaKM");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhapXuat_MaCcap",
                table: "PhieuNhapXuat",
                column: "MaCcap");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhapXuat_MaNV",
                table: "PhieuNhapXuat",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuOrder_MaCN",
                table: "PhieuOrder",
                column: "MaCN");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuOrder_MaKM",
                table: "PhieuOrder",
                column: "MaKM");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuOrder_MaNV",
                table: "PhieuOrder",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_Maloai",
                table: "SanPham",
                column: "Maloai");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MaTopping",
                table: "SanPham",
                column: "MaTopping");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BOM");

            migrationBuilder.DropTable(
                name: "ChangePasswordModel");

            migrationBuilder.DropTable(
                name: "ChitietDanhGia");

            migrationBuilder.DropTable(
                name: "CTPhieu");

            migrationBuilder.DropTable(
                name: "CTSanPham");

            migrationBuilder.DropTable(
                name: "CTSPonl");

            migrationBuilder.DropTable(
                name: "DanhMucCa");

            migrationBuilder.DropTable(
                name: "ForgotPassword");

            migrationBuilder.DropTable(
                name: "KIT");

            migrationBuilder.DropTable(
                name: "ResetPasswordModel");

            migrationBuilder.DropTable(
                name: "SearchHistories");

            migrationBuilder.DropTable(
                name: "SignInModel");

            migrationBuilder.DropTable(
                name: "Slider");

            migrationBuilder.DropTable(
                name: "NguyenVatLieu");

            migrationBuilder.DropTable(
                name: "PhieuNhapXuat");

            migrationBuilder.DropTable(
                name: "PhieuOrder");

            migrationBuilder.DropTable(
                name: "Phieudhonl");

            migrationBuilder.DropTable(
                name: "Size");

            migrationBuilder.DropTable(
                name: "SanPham");

            migrationBuilder.DropTable(
                name: "Nhacungcap");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "DanhMucKM");

            migrationBuilder.DropTable(
                name: "Loai");

            migrationBuilder.DropTable(
                name: "ChiNhanh");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "SignUpUserModel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "SignUpUserModel",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
