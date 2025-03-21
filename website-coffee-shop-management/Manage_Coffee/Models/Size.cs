using System;
using System.Collections.Generic;

namespace Manage_Coffee.Models;

public partial class Size
{
    public string MaSize { get; set; } = null!;

    public string Ten { get; set; } = null!;

    public int TriGia { get; set; }
    public string Trongluong { get; set; } 

    public virtual ICollection<CtsanPham> CtsanPhams { get; set; } = new List<CtsanPham>();

    public virtual ICollection<Ctsponl> Ctsponls { get; set; } = new List<Ctsponl>();
    public virtual ICollection<CTKIT> CtKits { get; set; } = new List<CTKIT>();
}
