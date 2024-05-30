using Microsoft.AspNetCore.Mvc;

    using LibraryManagementSystem.Entities;
    using LibraryManagementSystem.Model;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
    
{
        [Route("api/[Controller]/[Action]")]
        [ApiController]
    public class BooksController : Controller
    {
            private readonly Container _container;

            public BooksController()
            {
                _container = GetContainer();
            }

            private string URI = "https://localhost:8081";
            private string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            private string DatabaseName = "LibraryDB";
            private string ContainerName = "LibraryContainer";

            private Container GetContainer()
            {
                CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
                Database database = cosmosClient.GetDatabase(DatabaseName);
                return database.GetContainer(ContainerName);
            }

            [HttpPost]
            public async Task<IActionResult> AddBook([FromBody] Book bookModel)
            {
                var book = new BookEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    UId = Guid.NewGuid().ToString(),
                    DocumentType = "book",
                    Title = bookModel.Title,
                    Author = bookModel.Author,
                    PublishedDate = bookModel.PublishedDate,
                    ISBN = bookModel.ISBN,
                    IsIssued = false,
                    CreatedBy = "Admin",
                    CreatedOn = DateTime.Now,
                    UpdatedBy = "Admin",
                    UpdatedOn = DateTime.Now,
                    Version = 1,
                    Active = true,
                    Archived = false
                };

                await _container.CreateItemAsync(book);

                bookModel.UId = book.UId;

                return Ok(bookModel);
            }

            [HttpGet]
            public async Task<IActionResult> GetBookByUId(string uid)
            {
                var book = _container.GetItemLinqQueryable<BookEntity>(true)
                                     .Where(b => b.UId == uid && b.Active && !b.Archived)
                                     .AsEnumerable()
                                     .FirstOrDefault();

                if (book == null)
                {
                    return NotFound();
                }

                var bookModel = new Book
                {
                    UId = book.UId,
                    Title = book.Title,
                    Author = book.Author,
                    PublishedDate = book.PublishedDate,
                    ISBN = book.ISBN,
                    IsIssued = book.IsIssued
                };

                return Ok(bookModel);
            }

            [HttpGet]
            public async Task<IActionResult> GetBookByName(string name)
            {
                var book = _container.GetItemLinqQueryable<BookEntity>(true)
                                     .Where(b => b.Title == name && b.Active && !b.Archived)
                                     .AsEnumerable()
                                     .FirstOrDefault();

                if (book == null)
                {
                    return NotFound();
                }

                var bookModel = new Book
                {
                    UId = book.UId,
                    Title = book.Title,
                    Author = book.Author,
                    PublishedDate = book.PublishedDate,
                    ISBN = book.ISBN,
                    IsIssued = book.IsIssued
                };

                return Ok(bookModel);
            }

            [HttpGet]
            public async Task<IActionResult> GetAllBooks()
            {
                var books = _container.GetItemLinqQueryable<BookEntity>(true)
                                      .Where(b => b.Active && !b.Archived && b.DocumentType == "book")
                                      .ToList();

                var bookModels = books.Select(book => new Book
                {
                    UId = book.UId,
                    Title = book.Title,
                    Author = book.Author,
                    PublishedDate = book.PublishedDate,
                    ISBN = book.ISBN,
                    IsIssued = book.IsIssued
                }).ToList();

                return Ok(bookModels);
            }

            [HttpGet]
            public async Task<IActionResult> GetAvailableBooks()
            {
                var books = _container.GetItemLinqQueryable<BookEntity>(true)
                                      .Where(b => b.Active && !b.Archived && !b.IsIssued && b.DocumentType == "book")
                                      .ToList();

                var bookModels = books.Select(book => new Book
                {
                    UId = book.UId,
                    Title = book.Title,
                    Author = book.Author,
                    PublishedDate = book.PublishedDate,
                    ISBN = book.ISBN,
                    IsIssued = book.IsIssued
                }).ToList();

                return Ok(bookModels);
            }

            [HttpGet]
            public async Task<IActionResult> GetIssuedBooks()
            {
                var books = _container.GetItemLinqQueryable<BookEntity>(true)
                                      .Where(b => b.Active && !b.Archived && b.IsIssued && b.DocumentType == "book")
                                      .ToList();

                var bookModels = books.Select(book => new Book
                {
                    UId = book.UId,
                    Title = book.Title,
                    Author = book.Author,
                    PublishedDate = book.PublishedDate,
                    ISBN = book.ISBN,
                    IsIssued = book.IsIssued
                }).ToList();

                return Ok(bookModels);
            }

            [HttpPost]
            public async Task<IActionResult> UpdateBook([FromBody] Book bookModel)
            {
                var existingBook = _container.GetItemLinqQueryable<BookEntity>(true)
                                             .Where(b => b.UId == bookModel.UId && b.Active && !b.Archived)
                                             .AsEnumerable()
                                             .FirstOrDefault();

                if (existingBook == null)
                {
                    return NotFound();
                }

                existingBook.Archived = true;
                existingBook.Active = false;
                await _container.ReplaceItemAsync(existingBook, existingBook.Id);

                var updatedBook = new BookEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    UId = bookModel.UId,
                    DocumentType = "book",
                    Title = bookModel.Title,
                    Author = bookModel.Author,
                    PublishedDate = bookModel.PublishedDate,
                    ISBN = bookModel.ISBN,
                    IsIssued = bookModel.IsIssued,
                    CreatedBy = existingBook.CreatedBy,
                    CreatedOn = existingBook.CreatedOn,
                    UpdatedBy = "Admin",
                    UpdatedOn = DateTime.Now,
                    Version = existingBook.Version + 1,
                    Active = true,
                    Archived = false
                };

                await _container.CreateItemAsync(updatedBook);

                return Ok(bookModel);
            }
        }
}
