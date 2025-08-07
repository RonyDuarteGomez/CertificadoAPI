using Microsoft.AspNetCore.Mvc;
using CertificadoAPI.Models;
using System;
using System.IO;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using Microsoft.AspNetCore.Hosting;
using iText.Kernel.Pdf.Canvas;

namespace CertificadoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificadoController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public CertificadoController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("Generar")]
        public IActionResult GenerarPDF([FromBody] CertificadoRequest request)
        {
            if (string.IsNullOrEmpty(request.Nombre) ||
                string.IsNullOrEmpty(request.Actividad) ||
                string.IsNullOrEmpty(request.Fecha))
            {
                return BadRequest("Faltan datos requeridos.");
            }

            try
            {
                using var ms = new MemoryStream();
                using var writer = new PdfWriter(ms);
                using var pdf = new PdfDocument(writer);

                var pageSize = PageSize.A4.Rotate(); 
                pdf.SetDefaultPageSize(pageSize);
                var document = new Document(pdf, pageSize);
                document.SetMargins(60, 60, 60, 60);

                pdf.AddNewPage();

                string imgPath = System.IO.Path.Combine(_env.WebRootPath ?? "wwwroot", "img", "certificado.jpg");
                if (!System.IO.File.Exists(imgPath))
                    return StatusCode(500, "No se encontró la imagen de fondo.");

                var imageData = ImageDataFactory.Create(imgPath);

                var canvas = new PdfCanvas(pdf.GetFirstPage());
                canvas.AddImageFittedIntoRectangle(imageData, pageSize, false);

                

                document.Add(new Paragraph($"{request.Nombre}")
                    .SetFontSize(30)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(200));

                document.Add(new Paragraph($"{request.Actividad}")
                    .SetFontSize(25)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(100));

                document.Add(new Paragraph($"{request.Fecha}")
                    .SetFontSize(15)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginTop(61));

                

                document.Close();

                var pdfBytes = ms.ToArray();
                var base64Pdf = Convert.ToBase64String(pdfBytes);
                return Ok(base64Pdf);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar PDF: {ex.Message}");
            }
        }
    }
}
