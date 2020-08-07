using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DocumentUploadCore;
using DocumentUploadDemo.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace DocumentUploadDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly string targetFilePath = "c:\\files";

        // GET: api/<FilesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] {"value1", "value2"};
        }

        // GET api/<FilesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<FilesController>
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            IFileUploadRequest uploadRequest = new StreamingFileUploadRequest(Request);
            ManagedDocument[] uploadedFiles;

            try
            {
                uploadedFiles = await uploadRequest.ReadUploadedFiles();
            }
            catch (InvalidFileUploadException ex)
            {
                return BadRequest(ex.Message);
            }

            foreach (var currentFile in uploadedFiles)
            {
                await using var targetStream = System.IO.File.Create(Path.Combine(targetFilePath, $"{currentFile.Metadata.Name}{currentFile.Metadata.FileType}"));
                await targetStream.WriteAsync(currentFile.Contents);
            }

            return Created(nameof(FilesController), null);
        }

        // PUT api/<FilesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FilesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}