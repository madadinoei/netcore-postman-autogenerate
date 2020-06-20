using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AutoGeneratePostman.Controllers
{
    /// <summary>
    /// Books Controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = false, GroupName = "Books")]

    public class BooksController : ControllerBase
    {
        private static List<string> BookNames = new List<string>()
        {"In Search of Lost Time", "Ulysses", "Don Quixote", "The Great Gatsby",
            "One Hundred Years of Solitude",
            " Moby Dick",
            "War and Peace",
            "Lolita",
            "Hamlet",
            "The Catcher in the Rye"
        };

        private readonly ILogger<BooksController> _logger;

        public BooksController(ILogger<BooksController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// get all books title
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var rng = new Random();
            return BookNames.ToList();
        }

        /// <summary>
        /// add new book command summary
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<IActionResult> Post(Book command)
        {
            BookNames.Add(command.Title);
            return new Task<IActionResult>(() => Ok(command));
        }
    }
}
