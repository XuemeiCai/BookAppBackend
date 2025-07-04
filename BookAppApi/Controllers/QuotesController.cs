using Microsoft.AspNetCore.Mvc;
using BookAppApi.Models;
using BookAppApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace BookAppApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QuotesController : ControllerBase
    {
        private readonly QuoteService _quoteService;

        public QuotesController(QuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        // Get all quotes
        [HttpGet]
        public async Task<ActionResult<List<Quote>>> GetQuotes()
        {
            var quotes = await _quoteService.GetAsync();
            return Ok(quotes);
        }

        //Add a new quote
        [HttpPost]
        public async Task<ActionResult<Quote>> AddQuote([FromBody] Quote newQuote)
        {
            var quote = await _quoteService.CreateAsync(newQuote);
            return CreatedAtAction(nameof(GetQuotes), new { id = quote.Id }, quote);
        }

        // Edit a quote
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuote(string id, [FromBody] Quote updatedQuote)
        {
            updatedQuote.Id = id;
            await _quoteService.UpdateAsync(id, updatedQuote);
            return Ok(updatedQuote);
        }

        // Delete a quote
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuote(string id)
        {
            await _quoteService.DeleteAsync(id);
            return NoContent();
        }

    }
}
