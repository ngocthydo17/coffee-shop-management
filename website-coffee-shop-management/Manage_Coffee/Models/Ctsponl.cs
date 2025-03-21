﻿using System;
using System.Collections.Generic;

namespace Manage_Coffee.Models;

public partial class Ctsponl
{
    public int Soluong { get; set; }

    public int Gia { get; set; }

    public int Tongtien { get; set; }

    public string? Ghichu { get; set; }

    public string MaSp { get; set; }

    public string MaPhieuonl { get; set; } = null!;

    public string MaKh { get; set; } = null!;

    public string MaSize { get; set; } = null!;
    public string MaDa { get; set; } = null!;

	public string MaDuong { get; set; } = null!;

    public virtual Phieudhonl MaPhieuonlNavigation { get; set; } = null!;

    public virtual Size MaSizeNavigation { get; set; } = null!;
    public virtual Da MaDaNavigation { get; set; } = null!;
    public virtual Duong MaDuongNavigation { get; set; } = null!;

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
