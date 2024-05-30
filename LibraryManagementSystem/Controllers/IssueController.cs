using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;


namespace LibraryManagementSystem.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class IssueController : Controller
    {
        private readonly Container _container;

        public IssueController()
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
        public async Task<IActionResult> IssueBook([FromBody] Issue issueModel)
        {
            var book = _container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(b => b.UId == issueModel.BookId && b.Active && !b.Archived && !b.IsIssued)
                                 .AsEnumerable()
                                 .FirstOrDefault();

            if (book == null)
            {
                return NotFound("Book not found or already issued.");
            }

            var member = _container.GetItemLinqQueryable<MemberEntity>(true)
                                   .Where(m => m.UId == issueModel.MemberId && m.Active && !m.Archived)
                                   .AsEnumerable()
                                   .FirstOrDefault();

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

            await _container.ReplaceItemAsync(book, book.Id);
            await _container.CreateItemAsync(issue);

            return Ok(issueModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook([FromBody] Issue issueModel)
        {
            var issue = _container.GetItemLinqQueryable<IssueEntity>(true)
                                  .Where(i => i.BookId == issueModel.BookId && i.MemberId == issueModel.MemberId && i.Active && !i.Archived && !i.IsReturned)
                                  .AsEnumerable()
                                  .FirstOrDefault();

            if (issue == null)
            {
                return NotFound("Issue record not found or book already returned.");
            }

            issue.IsReturned = true;
            issue.UpdatedOn = DateTime.Now;
            issue.UpdatedBy = "Admin";
            await _container.ReplaceItemAsync(issue, issue.Id);

            var book = _container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(b => b.UId == issueModel.BookId && b.Active && !b.Archived && b.IsIssued)
                                 .AsEnumerable()
                                 .FirstOrDefault();

            if (book == null)
            {
                return NotFound("Book not found or already returned.");
            }

            book.IsIssued = false;
            book.UpdatedOn = DateTime.Now;
            book.UpdatedBy = "Admin";
            await _container.ReplaceItemAsync(book, book.Id);

            return Ok(issueModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetIssuedBooksByMember(string memberId)
        {
            var issues = _container.GetItemLinqQueryable<IssueEntity>(true)
                                   .Where(i => i.MemberId == memberId && i.Active && !i.Archived && !i.IsReturned)
                                   .ToList();

            var issueModels = issues.Select(issue => new Issue
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
        public async Task<IActionResult> GetAllIssues()
        {
            var issues = _container.GetItemLinqQueryable<IssueEntity>(true)
                                   .Where(i => i.Active && !i.Archived)
                                   .ToList();

            var issueModels = issues.Select(issue => new Issue
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

    }
}
