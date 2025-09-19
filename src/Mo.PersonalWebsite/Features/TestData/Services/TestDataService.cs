using Mo.PersonalWebsite.Infrastructure.Data;
using Mo.PersonalWebsite.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mo.PersonalWebsite.Features.TestData.Services;

public class TestDataService : ITestDataService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TestDataService> _logger;

    private readonly string[] _categories = {
        "Technology", "Programming", "Web Development", "Mobile Development", "DevOps",
        "Artificial Intelligence", "Machine Learning", "Data Science", "Cybersecurity", "Cloud Computing",
        "Software Engineering", "Frontend", "Backend", "Full Stack", "Database",
        "UI/UX Design", "Project Management", "Career", "Tutorials", "News"
    };

    private readonly string[] _sampleParagraphs = {
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
        "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.",
        "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga.",
        "Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestae non recusandae.",
        "Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat. The quick brown fox jumps over the lazy dog. This sentence contains every letter of the alphabet and is commonly used for testing purposes.",
        "In a rapidly evolving technological landscape, developers must continuously adapt to new frameworks, languages, and methodologies. The importance of staying current with industry trends cannot be overstated, as it directly impacts career growth and project success.",
        "Modern web development encompasses a vast array of technologies and best practices. From responsive design principles to progressive web applications, developers must balance user experience with performance optimization.",
        "Database design and optimization play a crucial role in application performance. Understanding indexing strategies, query optimization, and normalization principles can significantly impact system scalability.",
        "Cloud computing has revolutionized how we deploy and manage applications. Services like AWS, Azure, and Google Cloud Platform provide scalable infrastructure solutions that enable rapid development and deployment.",
        "Cybersecurity considerations should be integrated into every phase of the development lifecycle. From secure coding practices to regular security audits, protecting user data and system integrity is paramount."
    };

    private readonly string[] _titleWords = {
        "Advanced", "Complete", "Essential", "Modern", "Ultimate", "Comprehensive", "Professional", "Practical",
        "Introduction", "Guide", "Tutorial", "Mastering", "Understanding", "Building", "Creating", "Developing",
        "Optimizing", "Implementing", "Designing", "Testing", "Deploying", "Scaling", "Managing", "Learning"
    };

    public TestDataService(AppDbContext context, ILogger<TestDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Category>> EnsureTestCategoriesAsync()
    {
        var existingCategories = await _context.Categories.ToListAsync();
        var categoriesToCreate = new List<Category>();

        foreach (var categoryName in _categories)
        {
            if (!existingCategories.Any(c => c.Name == categoryName))
            {
                categoriesToCreate.Add(new Category
                {
                    Name = categoryName,
                    Slug = GenerateSlug(categoryName),
                    Description = $"Articles about {categoryName.ToLower()}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (categoriesToCreate.Any())
        {
            _context.Categories.AddRange(categoriesToCreate);
            await _context.SaveChangesAsync();
        }

        return await _context.Categories.ToListAsync();
    }

    public async Task<int> GenerateTestArticlesAsync(int count = 100000)
    {
        _logger.LogInformation("Starting to generate {Count} test articles", count);

        // Ensure categories exist
        var categories = await EnsureTestCategoriesAsync();
        var random = new Random();

        var batchSize = 1000;
        var totalCreated = 0;

        for (int batch = 0; batch < Math.Ceiling((double)count / batchSize); batch++)
        {
            var currentBatchSize = Math.Min(batchSize, count - totalCreated);
            var articles = new List<Article>();

            for (int i = 0; i < currentBatchSize; i++)
            {
                var category = categories[random.Next(categories.Count)];
                var title = GenerateRandomTitle(random);
                var content = GenerateRandomContent(random);
                var createdDate = DateTime.UtcNow.AddDays(-random.Next(0, 365)).AddHours(-random.Next(0, 24));

                articles.Add(new Article
                {
                    Title = $"{title} #{totalCreated + i + 1}",
                    Slug = GenerateSlug($"{title}-{totalCreated + i + 1}"),
                    Content = content,
                    MetaDescription = GenerateMetaDescription(content),
                    MetaKeywords = GenerateKeywords(category.Name, random),
                    CategoryId = category.Id,
                    IsPublished = random.NextDouble() > 0.1, // 90% published
                    CreatedAt = createdDate,
                    UpdatedAt = createdDate,
                    PublishedAt = random.NextDouble() > 0.1 ? createdDate : null
                });
            }

            _context.Articles.AddRange(articles);
            await _context.SaveChangesAsync();

            totalCreated += currentBatchSize;
            _logger.LogInformation("Created batch {Batch}: {Created}/{Total} articles", batch + 1, totalCreated, count);
        }

        _logger.LogInformation("Successfully generated {Count} test articles", totalCreated);
        return totalCreated;
    }

    public async Task<int> ClearAllTestDataAsync()
    {
        _logger.LogInformation("Starting to clear all test data");

        // Clear articles first (due to foreign key constraints)
        var articleCount = await _context.Articles.CountAsync();
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Articles");

        // Clear categories (keep General if it exists)
        var categoryCount = await _context.Categories.Where(c => c.Name != "General").CountAsync();
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Categories WHERE Name != 'General'");

        _logger.LogInformation("Cleared {ArticleCount} articles and {CategoryCount} categories", articleCount, categoryCount);
        return articleCount + categoryCount;
    }

    private string GenerateRandomTitle(Random random)
    {
        var word1 = _titleWords[random.Next(_titleWords.Length)];
        var word2 = _titleWords[random.Next(_titleWords.Length)];
        var category = _categories[random.Next(_categories.Length)];
        
        var patterns = new[]
        {
            $"{word1} {category} for Developers",
            $"{word1} Guide to {category}",
            $"How to {word2} {category} Applications",
            $"{word1} {word2} with {category}",
            $"Best Practices for {category} Development",
            $"{word1} {category}: Tips and Tricks",
            $"Getting Started with {category}",
            $"{word2} Modern {category} Solutions"
        };

        return patterns[random.Next(patterns.Length)];
    }

    private string GenerateRandomContent(Random random)
    {
        var content = new StringBuilder();
        var paragraphCount = random.Next(100, 150); // 100-150 lines/paragraphs

        content.AppendLine("<h2>Introduction</h2>");
        content.AppendLine($"<p>{_sampleParagraphs[random.Next(_sampleParagraphs.Length)]}</p>");

        for (int i = 0; i < paragraphCount; i++)
        {
            if (i % 20 == 0 && i > 0) // Add headers every 20 paragraphs
            {
                content.AppendLine($"<h3>Section {(i / 20) + 1}</h3>");
            }

            if (i % 15 == 10) // Add code blocks occasionally
            {
                content.AppendLine("<pre><code>");
                content.AppendLine("function example() {");
                content.AppendLine("    console.log('This is a sample code block');");
                content.AppendLine("    return true;");
                content.AppendLine("}");
                content.AppendLine("</code></pre>");
            }
            else if (i % 25 == 15) // Add lists occasionally
            {
                content.AppendLine("<ul>");
                for (int j = 0; j < random.Next(3, 6); j++)
                {
                    content.AppendLine($"<li>List item {j + 1}: {_sampleParagraphs[random.Next(_sampleParagraphs.Length)].Substring(0, 50)}...</li>");
                }
                content.AppendLine("</ul>");
            }
            else
            {
                var paragraph = _sampleParagraphs[random.Next(_sampleParagraphs.Length)];
                if (random.NextDouble() < 0.3) // 30% chance to combine paragraphs
                {
                    paragraph += " " + _sampleParagraphs[random.Next(_sampleParagraphs.Length)];
                }
                content.AppendLine($"<p>{paragraph}</p>");
            }
        }

        content.AppendLine("<h2>Conclusion</h2>");
        content.AppendLine($"<p>{_sampleParagraphs[random.Next(_sampleParagraphs.Length)]}</p>");

        return content.ToString();
    }

    private string GenerateMetaDescription(string content)
    {
        // Extract first paragraph and limit to 160 characters
        var firstParagraph = content.Split(new[] { "</p>" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        var plainText = System.Text.RegularExpressions.Regex.Replace(firstParagraph, "<.*?>", "");
        return plainText.Length > 160 ? plainText.Substring(0, 157) + "..." : plainText;
    }

    private string GenerateKeywords(string categoryName, Random random)
    {
        var keywords = new List<string> { categoryName.ToLower() };
        var commonKeywords = new[] { "development", "programming", "software", "technology", "guide", "tutorial", "tips", "best practices" };
        
        for (int i = 0; i < random.Next(3, 6); i++)
        {
            keywords.Add(commonKeywords[random.Next(commonKeywords.Length)]);
        }

        return string.Join(", ", keywords.Distinct());
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(":", "")
            .Replace("#", "sharp")
            .Replace("+", "plus")
            .Replace(".", "-")
            .Replace(",", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace("|", "-")
            .Replace("?", "")
            .Replace("!", "")
            .Replace("@", "at")
            .Replace("%", "percent")
            .Replace("*", "")
            .Replace("=", "equals")
            .Replace("<", "")
            .Replace(">", "");
    }
}
