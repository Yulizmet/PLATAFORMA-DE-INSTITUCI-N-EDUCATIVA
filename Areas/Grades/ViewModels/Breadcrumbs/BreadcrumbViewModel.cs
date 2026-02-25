namespace SchoolManager.Areas.Grades.ViewModels.Breadcrumbs
{
    public class BreadcrumbItem
    {
        public string Title { get; set; } = null!;
        public string? Url { get; set; }
        public bool IsActive => string.IsNullOrEmpty(Url);
    }

    public class BreadcrumbViewModel
    {
        public List<BreadcrumbItem> Items { get; set; } = new();

        public void AddItem(string title, string? url = null)
        {
            Items.Add(new BreadcrumbItem { Title = title, Url = url });
        }
    }
}
