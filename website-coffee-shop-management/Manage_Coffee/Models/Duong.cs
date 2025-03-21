namespace Manage_Coffee.Models
{
    public class Duong
    {
        public string MaDuong { get; set; } = null!;

        public string Ten { get; set; } = null!;
        public virtual ICollection<Ctsponl> Ctsponls { get; set; } = new List<Ctsponl>();
    }
}
