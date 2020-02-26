using System;
using System.Threading.Tasks;
using System.Web;

using core;
using core.core;

using Microsoft.AspNetCore.Mvc;

using Serilog;

namespace webapp.Controllers
{
    /// <summary>
    /// Triggers new image scans.
    /// </summary>
    [ApiController]
    [Route("scan")]
    public class ScanController : ControllerBase
    {
        private static readonly ILogger Logger = Log.ForContext<ScanController>();

        private readonly ImageScanner scanner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanController"/> class.
        /// </summary>
        public ScanController(ImageScanner scanner)
        {
            this.scanner = scanner;
        }

        /// <summary>
        /// Scan the image specified in a request body.
        /// </summary>
        /// <param name="imageTag">The image tag to scan.</param>
        /// <returns>Acknowledgement.</returns>
        [HttpPost]
        [Route("image/{imageTag}")]
        public async Task<ObjectResult> ScanImages([FromRoute] string imageTag)
        {
            try
            {
                var unescapedTag = HttpUtility.UrlDecode(imageTag);
                var image = ContainerImage.FromFullName(unescapedTag);
                var result = await this.scanner.Scan(new ScanRequest
                {
                    Image = image,
                    ScanId = Guid.NewGuid().ToString(),
                });
                return this.StatusCode(201, result);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Failed to scan the image {Image}", imageTag);
                return this.StatusCode(500, $"Failed to scan an image because of exception: {ex.Message}");
            }
        }
    }
}
