using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HtmlToPdfApp.Services
{
    public class HtmlToPdfConverter
    {
        private readonly IConverter _converter;
        private readonly ILogger<HtmlToPdfConverter> _logger;

        public HtmlToPdfConverter(IConverter converter, ILogger<HtmlToPdfConverter> logger = null)
        {
            _converter = converter;
            _logger = logger;
        }

        public byte[] ConvertHtmlToPdf(string htmlContent)
        {
            try
            {
                _logger?.LogInformation("Starting HTML to PDF conversion");

                // Ensure proper UTF-8 encoding for special characters
                var encodedHtml = EnsureUtf8Encoding(htmlContent);

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                        DocumentTitle = "Generated PDF",
                        DPI = 300, // Higher DPI for better quality
                        ImageDPI = 300,
                        ImageQuality = 100,
                    },
                    Objects = {
                        new ObjectSettings
                        {
                            PagesCount = true,
                            HtmlContent = encodedHtml,
                            WebSettings = {
                                DefaultEncoding = "utf-8",
                                EnableJavascript = true,
                                EnableIntelligentShrinking = true,
                                PrintMediaType = true,
                                LoadImages = true,
                                MinimumFontSize = 10,
                                UserStyleSheet = null
                            },
                            HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = false },
                            FooterSettings = { FontSize = 9, Line = false, Center = "Generated on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") }
                        }
                    }
                };

                _logger?.LogInformation("Configuration created, starting conversion");
                var result = _converter.Convert(doc);
                _logger?.LogInformation($"Conversion completed, PDF size: {result.Length} bytes");

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error converting HTML to PDF");
                throw new Exception("PDF conversion failed. See inner exception for details.", ex);
            }
        }

        public async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
            return await Task.Run(() => ConvertHtmlToPdf(htmlContent));
        }

        private string EnsureUtf8Encoding(string html)
        {
            try
            {
                // Check if HTML already has UTF-8 meta tag
                if (!html.Contains("<meta charset=\"utf-8\"") &&
                    !html.Contains("<meta charset='utf-8'") &&
                    !html.Contains("charset=utf-8"))
                {
                    // Add UTF-8 meta tag if not present
                    int headIndex = html.IndexOf("<head>", StringComparison.OrdinalIgnoreCase);
                    if (headIndex >= 0)
                    {
                        // Insert after <head> tag
                        html = html.Insert(headIndex + 6, "\n<meta charset=\"utf-8\">\n");
                    }
                    else
                    {
                        // If no head tag, try to add before the first tag
                        int htmlIndex = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
                        if (htmlIndex >= 0)
                        {
                            // Find where the html tag ends
                            int htmlEndIndex = html.IndexOf(">", htmlIndex);
                            if (htmlEndIndex >= 0)
                            {
                                html = html.Insert(htmlEndIndex + 1, "\n<head><meta charset=\"utf-8\"></head>\n");
                            }
                        }
                    }
                }

                // Ensure HTML is properly UTF-8 encoded
                byte[] bytes = Encoding.UTF8.GetBytes(html);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error ensuring UTF-8 encoding, using original HTML");
                return html; // Return original if processing fails
            }
        }
    }
}