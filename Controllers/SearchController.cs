using Microsoft.AspNetCore.Mvc;
using SchoolManager.Services;

namespace SchoolManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    private readonly string[] _allowedTypes = {"persons", "students", "teachers" };
    
    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] string type)
    {
        if (string.IsNullOrEmpty(term) || term.Length < 2)
            return Ok(new List<SearchResult>());
        
        if (string.IsNullOrEmpty(type))
            return BadRequest();
        
        if (!_allowedTypes.Contains(type.ToLower()))
            return BadRequest();
        
        var results = await _searchService.SearchAsync(term, type);
        return Ok(results);
    }
    
}