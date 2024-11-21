using CoCo.Models;

namespace CoCo.ViewModel.NewsViewModel
{
    public class NewsViewModel
    {
        public NewsModel news { get; set; }
        public List<CommentModel> Comments { get; set; }
        public string? cbody { get; set; }
    }
}
