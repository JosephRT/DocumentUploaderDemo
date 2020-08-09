using System.Threading.Tasks;
using DocumentUploadCore.Library;
using DocumentUploadDemo.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace DocumentUploadDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IDocumentManagementService documentManagementService;
        private readonly IFileUploadRequestFactory fileUploadRequestFactory;
        private readonly FileUploadSettings fileUploadSettings;

        public FilesController(IDocumentManagementService documentManagementService, IFileUploadRequestFactory fileUploadRequestFactory, FileUploadSettings fileUploadSettings)
        {
            this.documentManagementService = documentManagementService;
            this.fileUploadRequestFactory = fileUploadRequestFactory;
            this.fileUploadSettings = fileUploadSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var documentsMetadata = await documentManagementService.ListDocumentsAsync();
            return Ok(documentsMetadata);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var retrievedDocument = await documentManagementService.GetDocumentAsync(id);

            if (retrievedDocument == null)
            {
                return NotFound();
            }

            const string contentType = "APPLICATION/octet-stream";
            var fullFileName = retrievedDocument.Metadata.FullFileName;
            return File(retrievedDocument.Contents, contentType, fullFileName);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var uploadRequest = fileUploadRequestFactory.GetFileUploadRequest(Request, fileUploadSettings);

            try
            {
                var uploadedFiles = await uploadRequest.ReadUploadedFiles();

                foreach (var currentFile in uploadedFiles)
                {
                    await documentManagementService.SaveDocumentAsync(currentFile);
                }
            }
            catch (InvalidFileUploadException ex)
            {
                return BadRequest(ex.Message);
            }

            return Created(nameof(FilesController), null);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await documentManagementService.DeleteDocumentAsync(id);
            return Ok();
        }
    }
}