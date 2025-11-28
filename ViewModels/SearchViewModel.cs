// ViewModels/SearchViewModel.cs
namespace Home_furnishings.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; } = "name";
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 12;

        // User context
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public int CartItemCount { get; set; }

        // Pagination helpers
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

        // Filter helpers
        public bool HasFilters => !string.IsNullOrEmpty(Query) || CategoryId.HasValue || MinPrice.HasValue || MaxPrice.HasValue;
        public string SearchSummary
        {
            get
            {
                if (!HasFilters) return "All Products";

                var parts = new List<string>();
                if (!string.IsNullOrEmpty(Query)) parts.Add($"'{Query}'");
                if (CategoryId.HasValue)
                {
                    var category = Categories.FirstOrDefault(c => c.CategoryId == CategoryId.Value);
                    if (category != null) parts.Add($"in {category.Name}");
                }
                if (MinPrice.HasValue || MaxPrice.HasValue)
                {
                    if (MinPrice.HasValue && MaxPrice.HasValue)
                        parts.Add($"${MinPrice:F2} - ${MaxPrice:F2}");
                    else if (MinPrice.HasValue)
                        parts.Add($"over ${MinPrice:F2}");
                    else if (MaxPrice.HasValue)
                        parts.Add($"under ${MaxPrice:F2}");
                }

                return string.Join(" ", parts);
            }
        }

        // Sort options
        public List<SortOption> SortOptions => new List<SortOption>
        {
            new SortOption { Value = "name", Text = "Name (A-Z)" },
            new SortOption { Value = "name_desc", Text = "Name (Z-A)" },
            new SortOption { Value = "price", Text = "Price (Low to High)" },
            new SortOption { Value = "price_desc", Text = "Price (High to Low)" },
            new SortOption { Value = "newest", Text = "Newest First" }
        };
    }

    public class SortOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}