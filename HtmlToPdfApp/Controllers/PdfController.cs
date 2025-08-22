using HtmlToPdfApp.Models;
using HtmlToPdfApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HtmlToPdfApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly HtmlToPdfConverter _pdfConverter;
        private readonly ILogger<PdfController> _logger;

        public PdfController(HtmlToPdfConverter pdfConverter, ILogger<PdfController> logger)
        {
            _pdfConverter = pdfConverter;
            _logger = logger;
        }

        /// <summary>
        /// Converts HTML content to PDF (JSON method)
        /// </summary>
        /// <param name="request">The HTML content to convert</param>
        /// <returns>PDF file</returns>
        [HttpPost("convert")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Convert([FromBody] HtmlToPdfRequest request)
        {
            try
            {
                _logger.LogInformation("Received HTML to PDF conversion request via JSON");

                if (request == null || string.IsNullOrEmpty(request.HtmlContent))
                {
                    _logger.LogWarning("Request was null or HTML content was empty");
                    return BadRequest("HTML content is required");
                }

                // Generate timestamp for filename
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string filename = $"xyz_{timestamp}.pdf";

                // Convert to PDF asynchronously
                _logger.LogInformation("Starting PDF conversion");
                byte[] pdfBytes = await _pdfConverter.ConvertHtmlToPdfAsync(request.HtmlContent);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    _logger.LogWarning("PDF conversion resulted in empty output");
                    return StatusCode(500, "PDF conversion failed to produce output");
                }

                _logger.LogInformation($"Conversion successful, returning PDF file ({pdfBytes.Length} bytes)");

                // Set content disposition to force download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}\"");

                // Return PDF as file with attachment disposition
                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during PDF conversion");
                return StatusCode(500, $"PDF conversion failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts HTML content to PDF using form data
        /// </summary>
        /// <returns>PDF file</returns>
        [HttpPost("convert-form")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConvertForm([FromForm] string htmlContent)
        {
            try
            {
                _logger.LogInformation("Received HTML to PDF conversion request via form data");

                if (string.IsNullOrEmpty(htmlContent))
                {
                    _logger.LogWarning("HTML content from form was empty");
                    return BadRequest("HTML content is required");
                }

                // Generate timestamp for filename
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string filename = $"xyz_{timestamp}.pdf";

                // Convert to PDF asynchronously
                _logger.LogInformation("Starting PDF conversion from form data");
                byte[] pdfBytes = await _pdfConverter.ConvertHtmlToPdfAsync(htmlContent);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    _logger.LogWarning("PDF conversion resulted in empty output");
                    return StatusCode(500, "PDF conversion failed to produce output");
                }

                _logger.LogInformation($"Conversion successful, returning PDF file ({pdfBytes.Length} bytes)");

                // Set content disposition to force download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}\"");

                // Return PDF as file with attachment disposition
                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during PDF conversion from form data");
                return StatusCode(500, $"PDF conversion failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts HTML content to PDF using raw content
        /// </summary>
        /// <returns>PDF file</returns>
        [HttpPost("convert-raw")]
        [Consumes("text/html", "text/plain")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConvertRaw()
        {
            try
            {
                _logger.LogInformation("Received HTML to PDF conversion request via raw content");

                // Read HTML from request body
                string htmlContent;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    htmlContent = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrEmpty(htmlContent))
                {
                    _logger.LogWarning("Raw HTML content was empty");
                    return BadRequest("HTML content is required");
                }

                // Generate timestamp for filename
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string filename = $"xyz_{timestamp}.pdf";

                // Convert to PDF asynchronously
                _logger.LogInformation("Starting PDF conversion from raw content");
                byte[] pdfBytes = await _pdfConverter.ConvertHtmlToPdfAsync(htmlContent);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    _logger.LogWarning("PDF conversion resulted in empty output");
                    return StatusCode(500, "PDF conversion failed to produce output");
                }

                _logger.LogInformation($"Conversion successful, returning PDF file ({pdfBytes.Length} bytes)");

                // Set content disposition to force download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}\"");

                // Return PDF as file with attachment disposition
                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during PDF conversion from raw content");
                return StatusCode(500, $"PDF conversion failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Status of the service</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                CurrentTime = "2025-08-22 00:25:10",
                User = "bashu06"
            });
        }
    }
}