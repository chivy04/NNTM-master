namespace StartupNNTM.ViewModels
{
    public class PostVm
    {
        public string Title { get; set; }
        public string AddressId { get; set; }
        public string Price { get; set; }
        public string Content { get; set; }
        public string TypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserId { get; set; }
        public ICollection<IFormFile> Images { get; set; }
    }
}
