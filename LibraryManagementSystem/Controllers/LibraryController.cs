//using LibraryManagementSystem.Entities;
//using LibraryManagementSystem.Model;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Cosmos;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LibraryManagementSystem.Controllers
//{
//    [Route("api/[Controller]/[Action]")]
//    [ApiController]
//    public class LibraryController : ControllerBase
//    {
//        private readonly Container _container;

//        public LibraryController()
//        {
//            _container = GetContainer();
//        }

//        private string URI = "https://localhost:8081";
//        private string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
//        private string DatabaseName = "LibraryDB";
//        private string ContainerName = "LibraryContainer";

//        private Container GetContainer()
//        {
//            CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
//            Database database = cosmosClient.GetDatabase(DatabaseName);
//            return database.GetContainer(ContainerName);
//        }

//        #region Book Operations

//        [HttpPost]
//        public async Task<IActionResult> AddBook([FromBody] Book bookModel)
//        {
//            var book = new BookEntity
//            {
//                Id = Guid.NewGuid().ToString(),
//                UId = Guid.NewGuid().ToString(),
//                DocumentType = "book",
//                Title = bookModel.Title,
//                Author = bookModel.Author,
//                PublishedDate = bookModel.PublishedDate,
//                ISBN = bookModel.ISBN,
//                IsIssued = false,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now,
//                UpdatedBy = "Admin",
//                UpdatedOn = DateTime.Now,
//                Version = 1,
//                Active = true,
//                Archived = false
//            };

//            await _container.CreateItemAsync(book);

//            bookModel.UId = book.UId;

//            return Ok(bookModel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetBookByUId(string uid)
//        {
//            var book = _container.GetItemLinqQueryable<BookEntity>(true)
//                                 .Where(b => b.UId == uid && b.Active && !b.Archived)
//                                 .AsEnumerable()
//                                 .FirstOrDefault();

//            if (book == null)
//            {
//                return NotFound();
//            }

//            var bookModel = new Book
//            {
//                UId = book.UId,
//                Title = book.Title,
//                Author = book.Author,
//                PublishedDate = book.PublishedDate,
//                ISBN = book.ISBN,
//                IsIssued = book.IsIssued
//            };

//            return Ok(bookModel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetBookByName(string name)
//        {
//            var book = _container.GetItemLinqQueryable<BookEntity>(true)
//                                 .Where(b => b.Title == name && b.Active && !b.Archived)
//                                 .AsEnumerable()
//                                 .FirstOrDefault();

//            if (book == null)
//            {
//                return NotFound();
//            }

//            var bookModel = new Book
//            {
//                UId = book.UId,
//                Title = book.Title,
//                Author = book.Author,
//                PublishedDate = book.PublishedDate,
//                ISBN = book.ISBN,
//                IsIssued = book.IsIssued
//            };

//            return Ok(bookModel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAllBooks()
//        {
//            var books = _container.GetItemLinqQueryable<BookEntity>(true)
//                                  .Where(b => b.Active && !b.Archived && b.DocumentType == "book")
//                                  .ToList();

//            var bookModels = books.Select(book => new Book
//            {
//                UId = book.UId,
//                Title = book.Title,
//                Author = book.Author,
//                PublishedDate = book.PublishedDate,
//                ISBN = book.ISBN,
//                IsIssued = book.IsIssued
//            }).ToList();

//            return Ok(bookModels);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAvailableBooks()
//        {
//            var books = _container.GetItemLinqQueryable<BookEntity>(true)
//                                  .Where(b => b.Active && !b.Archived && !b.IsIssued && b.DocumentType == "book")
//                                  .ToList();

//            var bookModels = books.Select(book => new Book
//            {
//                UId = book.UId,
//                Title = book.Title,
//                Author = book.Author,
//                PublishedDate = book.PublishedDate,
//                ISBN = book.ISBN,
//                IsIssued = book.IsIssued
//            }).ToList();

//            return Ok(bookModels);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetIssuedBooks()
//        {
//            var books = _container.GetItemLinqQueryable<BookEntity>(true)
//                                  .Where(b => b.Active && !b.Archived && b.IsIssued && b.DocumentType == "book")
//                                  .ToList();

//            var bookModels = books.Select(book => new Book
//            {
//                UId = book.UId,
//                Title = book.Title,
//                Author = book.Author,
//                PublishedDate = book.PublishedDate,
//                ISBN = book.ISBN,
//                IsIssued = book.IsIssued
//            }).ToList();

//            return Ok(bookModels);
//        }

//        [HttpPost]
//        public async Task<IActionResult> UpdateBook([FromBody] Book bookModel)
//        {
//            var existingBook = _container.GetItemLinqQueryable<BookEntity>(true)
//                                         .Where(b => b.UId == bookModel.UId && b.Active && !b.Archived)
//                                         .AsEnumerable()
//                                         .FirstOrDefault();

//            if (existingBook == null)
//            {
//                return NotFound();
//            }

//            existingBook.Archived = true;
//            existingBook.Active = false;
//            await _container.ReplaceItemAsync(existingBook, existingBook.Id);

//            var updatedBook = new BookEntity
//            {
//                Id = Guid.NewGuid().ToString(),
//                UId = bookModel.UId,
//                DocumentType = "book",
//                Title = bookModel.Title,
//                Author = bookModel.Author,
//                PublishedDate = bookModel.PublishedDate,
//                ISBN = bookModel.ISBN,
//                IsIssued = bookModel.IsIssued,
//                CreatedBy = existingBook.CreatedBy,
//                CreatedOn = existingBook.CreatedOn,
//                UpdatedBy = "Admin",
//                UpdatedOn = DateTime.Now,
//                Version = existingBook.Version + 1,
//                Active = true,
//                Archived = false
//            };

//            await _container.CreateItemAsync(updatedBook);

//            return Ok(bookModel);
//        }

//        #endregion

//        #region Member Operations

//        [HttpPost]
//        public async Task<IActionResult> AddMember([FromBody] Member memberModel)
//        {
//            var member = new MemberEntity
//            {
//                Id = Guid.NewGuid().ToString(),
//                UId = Guid.NewGuid().ToString(),
//                DocumentType = "member",
//                Name = memberModel.Name,
//                DateOfBirth = memberModel.DateOfBirth,
//                Email = memberModel.Email,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now,
//                UpdatedBy = "Admin",
//                UpdatedOn = DateTime.Now,
//                Version = 1,
//                Active = true,
//                Archived = false
//            };

//            await _container.CreateItemAsync(member);

//            memberModel.UId = member.UId;

//            return Ok(memberModel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetMemberByUId(string uid)
//        {
//            var member = _container.GetItemLinqQueryable<MemberEntity>(true)
//                                   .Where(m => m.UId == uid && m.Active && !m.Archived)
//                                   .AsEnumerable()
//                                   .FirstOrDefault();

//            if (member == null)
//            {
//                return NotFound();
//            }

//            var memberModel = new Member
//            {
//                UId = member.UId,
//                Name = member.Name,
//                DateOfBirth = member.DateOfBirth,
//                Email = member.Email
//            };

//            return Ok(memberModel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAllMembers()
//        {
//            var members = _container.GetItemLinqQueryable<MemberEntity>(true)
//                                    .Where(m => m.Active && !m.Archived && m.DocumentType == "member")
//                                    .ToList();

//            var memberModels = members.Select(member => new Member
//            {
//                UId = member.UId,
//                Name = member.Name,
//                DateOfBirth = member.DateOfBirth,
//                Email = member.Email
//            }).ToList();

//            return Ok(memberModels);
//        }

//        [HttpPost]
//        public async Task<IActionResult> UpdateMember([FromBody] Member memberModel)
//        {
//            var existingMember = _container.GetItemLinqQueryable<MemberEntity>(true)
//                                           .Where(m => m.UId == memberModel.UId && m.Active && !m.Archived)
//                                           .AsEnumerable()
//                                           .FirstOrDefault();

//            if (existingMember == null)
//            {
//                return NotFound();
//            }

//            existingMember.Archived = true;
//            existingMember.Active = false;
//            await _container.ReplaceItemAsync(existingMember, existingMember.Id);

//            var updatedMember = new MemberEntity
//            {
//                Id = Guid.NewGuid().ToString(),
//                UId = memberModel.UId,
//                DocumentType = "member",
//                Name = memberModel.Name,
//                DateOfBirth = memberModel.DateOfBirth,
//                Email = memberModel.Email,
//                CreatedBy = existingMember.CreatedBy,
//                CreatedOn = existingMember.CreatedOn,
//                UpdatedBy = "Admin",
//                UpdatedOn = DateTime.Now,
//                Version = existingMember.Version + 1,
//                Active = true,
//                Archived = false
//            };

//            await _container.CreateItemAsync(updatedMember);

//            return Ok(memberModel);
//        }

//        #endregion

//        #region Issue Operations

//        [HttpPost]
//        public async Task<IActionResult> IssueBook([FromBody] Issue issueModel)
//        {
//            var book = _container.GetItemLinqQueryable<BookEntity>(true)
//                                 .Where(b => b.UId == issueModel.BookId && b.Active && !b.Archived && !b.IsIssued)
//                                 .AsEnumerable()
//                                 .FirstOrDefault();

//            if (book == null)
//            {
//                return NotFound("Book not found or already issued.");
//            }

//            var member = _container.GetItemLinqQueryable<MemberEntity>(true)
//                                   .Where(m => m.UId == issueModel.MemberId && m.Active && !m.Archived)
//                                   .AsEnumerable()
//                                   .FirstOrDefault();

//            if (member == null)
//            {
//                return NotFound("Member not found.");
//            }

//            var issue = new IssueEntity
//            {
//                Id = Guid.NewGuid().ToString(),
//                UId = Guid.NewGuid().ToString(),
//                DocumentType = "issue",
//                BookId = issueModel.BookId,
//                MemberId = issueModel.MemberId,
//                IssueDate = DateTime.Now,
//                IsReturned = false,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now,
//                UpdatedBy = "Admin",
//                UpdatedOn = DateTime.Now,
//                Version = 1,
//                Active = true,
//                Archived = false
//            };

//            book.IsIssued = true;
//            book.UpdatedOn = DateTime.Now;
//            book.UpdatedBy = "Admin";

//            await _container.ReplaceItemAsync(book, book.Id);
//            await _container.CreateItemAsync(issue);

//            return Ok(issueModel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> ReturnBook([FromBody] Issue issueModel)
//        {
//            var issue = _container.GetItemLinqQueryable<IssueEntity>(true)
//                                  .Where(i => i.BookId == issueModel.BookId && i.MemberId == issueModel.MemberId && i.Active && !i.Archived && !i.IsReturned)
//                                  .AsEnumerable()
//                                  .FirstOrDefault();

//            if (issue == null)
//            {
//                return NotFound("Issue record not found or book already returned.");
//            }

//            issue.IsReturned = true;
//            issue.UpdatedOn = DateTime.Now;
//            issue.UpdatedBy = "Admin";
//            await _container.ReplaceItemAsync(issue, issue.Id);

//            var book = _container.GetItemLinqQueryable<BookEntity>(true)
//                                 .Where(b => b.UId == issueModel.BookId && b.Active && !b.Archived && b.IsIssued)
//                                 .AsEnumerable()
//                                 .FirstOrDefault();

//            if (book == null)
//            {
//                return NotFound("Book not found or already returned.");
//            }

//            book.IsIssued = false;
//            book.UpdatedOn = DateTime.Now;
//            book.UpdatedBy = "Admin";
//            await _container.ReplaceItemAsync(book, book.Id);

//            return Ok(issueModel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetIssuedBooksByMember(string memberId)
//        {
//            var issues = _container.GetItemLinqQueryable<IssueEntity>(true)
//                                   .Where(i => i.MemberId == memberId && i.Active && !i.Archived && !i.IsReturned)
//                                   .ToList();

//            var issueModels = issues.Select(issue => new Issue
//            {
//                UId = issue.UId,
//                BookId = issue.BookId,
//                MemberId = issue.MemberId,
//                IssueDate = issue.IssueDate,
//                ReturnDate = issue.ReturnDate,
//                IsReturned = issue.IsReturned
//            }).ToList();

//            return Ok(issueModels);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAllIssues()
//        {
//            var issues = _container.GetItemLinqQueryable<IssueEntity>(true)
//                                   .Where(i => i.Active && !i.Archived)
//                                   .ToList();

//            var issueModels = issues.Select(issue => new Issue
//            {
//                UId = issue.UId,
//                BookId = issue.BookId,
//                MemberId = issue.MemberId,
//                IssueDate = issue.IssueDate,
//                ReturnDate = issue.ReturnDate,
//                IsReturned = issue.IsReturned
//            }).ToList();

//            return Ok(issueModels);
//        }

//        #endregion
//    }
//}


using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private static List<BookEntity> books = new List<BookEntity>();
        private static List<MemberEntity> members = new List<MemberEntity>();
        private static List<IssueEntity> issues = new List<IssueEntity>();

        #region Book Operations

        [HttpPost]
        public IActionResult AddBook([FromBody] Book bookModel)
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

            books.Add(book);

            bookModel.UId = book.UId;

            return Ok(bookModel);
        }

        [HttpGet]
        public IActionResult GetBookByUId(string uid)
        {
            var book = books.FirstOrDefault(b => b.UId == uid && b.Active && !b.Archived);

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
        public IActionResult GetBookByName(string name)
        {
            var book = books.FirstOrDefault(b => b.Title == name && b.Active && !b.Archived);

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
        public IActionResult GetAllBooks()
        {
            var bookModels = books.Where(b => b.Active && !b.Archived && b.DocumentType == "book")
                                  .Select(book => new Book
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
        public IActionResult GetAvailableBooks()
        {
            var bookModels = books.Where(b => b.Active && !b.Archived && !b.IsIssued && b.DocumentType == "book")
                                  .Select(book => new Book
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
        public IActionResult GetIssuedBooks()
        {
            var bookModels = books.Where(b => b.Active && !b.Archived && b.IsIssued && b.DocumentType == "book")
                                  .Select(book => new Book
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
        public IActionResult UpdateBook([FromBody] Book bookModel)
        {
            var existingBook = books.FirstOrDefault(b => b.UId == bookModel.UId && b.Active && !b.Archived);

            if (existingBook == null)
            {
                return NotFound();
            }

            existingBook.Archived = true;
            existingBook.Active = false;

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

            books.Add(updatedBook);

            return Ok(bookModel);
        }

        #endregion

        #region Member Operations

        [HttpPost]
        public IActionResult AddMember([FromBody] Member memberModel)
        {
            var member = new MemberEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = Guid.NewGuid().ToString(),
                DocumentType = "member",
                Name = memberModel.Name,
                DateOfBirth = memberModel.DateOfBirth,
                Email = memberModel.Email,
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "Admin",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            members.Add(member);

            memberModel.UId = member.UId;

            return Ok(memberModel);
        }

        [HttpGet]
        public IActionResult GetMemberByUId(string uid)
        {
            var member = members.FirstOrDefault(m => m.UId == uid && m.Active && !m.Archived);

            if (member == null)
            {
                return NotFound();
            }

            var memberModel = new Member
            {
                UId = member.UId,
                Name = member.Name,
                DateOfBirth = member.DateOfBirth,
                Email = member.Email
            };

            return Ok(memberModel);
        }

        [HttpGet]
        public IActionResult GetAllMembers()
        {
            var memberModels = members.Where(m => m.Active && !m.Archived && m.DocumentType == "member")
                                      .Select(member => new Member
                                      {
                                          UId = member.UId,
                                          Name = member.Name,
                                          DateOfBirth = member.DateOfBirth,
                                          Email = member.Email
                                      }).ToList();

            return Ok(memberModels);
        }

        [HttpPost]
        public IActionResult UpdateMember([FromBody] Member memberModel)
        {
            var existingMember = members.FirstOrDefault(m => m.UId == memberModel.UId && m.Active && !m.Archived);

            if (existingMember == null)
            {
                return NotFound();
            }

            existingMember.Archived = true;
            existingMember.Active = false;

            var updatedMember = new MemberEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = memberModel.UId,
                DocumentType = "member",
                Name = memberModel.Name,
                DateOfBirth = memberModel.DateOfBirth,
                Email = memberModel.Email,
                CreatedBy = existingMember.CreatedBy,
                CreatedOn = existingMember.CreatedOn,
                UpdatedBy = "Admin",
                UpdatedOn = DateTime.Now,
                Version = existingMember.Version + 1,
                Active = true,
                Archived = false
            };

            members.Add(updatedMember);

            return Ok(memberModel);
        }

        #endregion

        #region Issue Operations

        [HttpPost]
        public IActionResult IssueBook([FromBody] Issue issueModel)
        {
            var book = books.FirstOrDefault(b => b.UId == issueModel.BookId && b.Active && !b.Archived && !b.IsIssued);

            if (book == null)
            {
                return NotFound("Book not found or already issued.");
            }

            var member = members.FirstOrDefault(m => m.UId == issueModel.MemberId && m.Active && !m.Archived);

            if (member == null)
            {
                return NotFound("Member not found.");
            }

            var issue = new IssueEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = Guid.NewGuid().ToString(),
                DocumentType = "issue",
                BookId = issueModel.BookId,
                MemberId = issueModel.MemberId,
                IssueDate = DateTime.Now,
                IsReturned = false,
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "Admin",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            book.IsIssued = true;
            book.UpdatedOn = DateTime.Now;
            book.UpdatedBy = "Admin";

            issues.Add(issue);

            return Ok(issueModel);
        }

        [HttpPost]
        public IActionResult ReturnBook([FromBody] Issue issueModel)
        {
            var issue = issues.FirstOrDefault(i => i.BookId == issueModel.BookId && i.MemberId == issueModel.MemberId && i.Active && !i.Archived && !i.IsReturned);

            if (issue == null)
            {
                return NotFound("Issue record not found or book already returned.");
            }

            issue.IsReturned = true;
            issue.UpdatedOn = DateTime.Now;
            issue.UpdatedBy = "Admin";

            var book = books.FirstOrDefault(b => b.UId == issueModel.BookId && b.Active && !b.Archived && b.IsIssued);

            if (book == null)
            {
                return NotFound("Book not found or already returned.");
            }

            book.IsIssued = false;
            book.UpdatedOn = DateTime.Now;
            book.UpdatedBy = "Admin";

            return Ok(issueModel);
        }

        [HttpGet]
        public IActionResult GetIssuedBooksByMember(string memberId)
        {
            var issueModels = issues.Where(i => i.MemberId == memberId && i.Active && !i.Archived && !i.IsReturned)
                                   .Select(issue => new Issue
                                   {
                                       UId = issue.UId,
                                       BookId = issue.BookId,
                                       MemberId = issue.MemberId,
                                       IssueDate = issue.IssueDate,
                                       ReturnDate = issue.ReturnDate,
                                       IsReturned = issue.IsReturned
                                   }).ToList();

            return Ok(issueModels);
        }

        [HttpGet]
        public IActionResult GetAllIssues()
        {
            var issueModels = issues.Where(i => i.Active && !i.Archived)
                                   .Select(issue => new Issue
                                   {
                                       UId = issue.UId,
                                       BookId = issue.BookId,
                                       MemberId = issue.MemberId,
                                       IssueDate = issue.IssueDate,
                                       ReturnDate = issue.ReturnDate,
                                       IsReturned = issue.IsReturned
                                   }).ToList();

            return Ok(issueModels);
        }

        #endregion
    }
}

