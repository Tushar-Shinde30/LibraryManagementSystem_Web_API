using LibraryManagementSystem.Entities;
using LibraryManagementSystem.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class MemberController : Controller
    {
        private readonly Container _container;

        public MemberController()
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
        public async Task<IActionResult> AddMember([FromBody] Member memberModel)
        {
            var member = new MemberEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = Guid.NewGuid().ToString(),
                DocumentType = "member",
                Name = memberModel.Name,
                DateOfBirth = memberModel.DateOfBirth,
                Email = memberModel.Email,
                CreatedBy = "Tushar",
                CreatedOn = DateTime.Now,
                UpdatedBy = "Tushar",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            await _container.CreateItemAsync(member);

            memberModel.UId = member.UId;

            return Ok(memberModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetMemberByUId(string uid)
        {
            var member = _container.GetItemLinqQueryable<MemberEntity>(true)
                                   .Where(m => m.UId == uid && m.Active && !m.Archived)
                                   .AsEnumerable()
                                   .FirstOrDefault();

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
        public async Task<IActionResult> GetAllMembers()
        {
            var members = _container.GetItemLinqQueryable<MemberEntity>(true)
                                    .Where(m => m.Active && !m.Archived && m.DocumentType == "member")
                                    .ToList();

            var memberModels = members.Select(member => new Member
            {
                UId = member.UId,
                Name = member.Name,
                DateOfBirth = member.DateOfBirth,
                Email = member.Email
            }).ToList();

            return Ok(memberModels);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMember([FromBody] Member memberModel)
        {
            var existingMember = _container.GetItemLinqQueryable<MemberEntity>(true)
                                           .Where(m => m.UId == memberModel.UId && m.Active && !m.Archived)
                                           .AsEnumerable()
                                           .FirstOrDefault();

            if (existingMember == null)
            {
                return NotFound();
            }

            existingMember.Archived = true;
            existingMember.Active = false;
            await _container.ReplaceItemAsync(existingMember, existingMember.Id);

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
                UpdatedBy = "Tushar",
                UpdatedOn = DateTime.Now,
                Version = existingMember.Version + 1,
                Active = true,
                Archived = false
            };

            await _container.CreateItemAsync(updatedMember);

            return Ok(memberModel);
        }

    }
}
