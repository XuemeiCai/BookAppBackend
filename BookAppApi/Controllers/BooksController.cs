using Microsoft.AspNetCore.Mvc;
using BookAppApi.Models;
using BookAppApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace BookAppApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookListService _bookList;

        public BooksController(BookListService bookList)
        {
            _bookList = bookList;
        }

        // Get book list
        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetBooks()
        {
            var books = await _bookList.GetAsync();
            return Ok(books);
        }

        // Add a new book
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book newBook)
        {
            var book = await _bookList.CreateAsync(newBook);
            return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
        }

        //Update book info
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(string id, [FromBody] Book updatedBook)
        {
            updatedBook.Id = id;
            await _bookList.UpdateAsync(id, updatedBook);
            return Ok(updatedBook);
        }

        // Delete a book
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            await _bookList.DeleteAsync(id);
            return NoContent();
        }

        //Search books by title or author
        [HttpGet("search")]
        public async Task<ActionResult<List<Book>>> SearchBooks([FromQuery] string term)
        {
            var books = await _bookList.SearchAsync(term);
            return Ok(books);
        }

        
    }

    
}
