﻿using System;
using System.Collections.Generic;

namespace Manage_Coffee.Models;

public partial class DanhMucKm
{
	public string MaKm { get; set; } = null!;

	public string Ten { get; set; } = null!;

	public int GiaTri { get; set; }

	public int Soluong { get; set; }

	public int Hanmuc { get; set; }
	public DateTime Ngayapdung { get; set; }

	public DateTime Ngayhethan { get; set; }

	public virtual ICollection<PhieuOrder> PhieuOrders { get; set; } = new List<PhieuOrder>();

	public virtual ICollection<Phieudhonl> Phieudhonls { get; set; } = new List<Phieudhonl>();
}
