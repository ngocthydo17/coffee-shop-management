namespace Manage_Coffee.Models
{
    public class Da
    {
        public string MaDa { get; set; } = null!;

        public string Ten { get; set; } = null!;
        public virtual ICollection<Ctsponl> Ctsponls { get; set; } = new List<Ctsponl>();
    }
}
