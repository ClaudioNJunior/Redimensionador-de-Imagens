using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Redimensionador_de_Imagens.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Redimensionador_de_Imagens.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Redimensionar([FromForm] ArquivosProporcaoModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Sem objeto" });

                if (model.Arquivos.Count == 0 || (model.Width <= 0 && model.Height <= 0))
                    return BadRequest(new { message = "Arquivo e pelo menos uma dimensão são essenciais" });

                foreach (var arquivo in model.Arquivos)
                {
                    if (arquivo.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            arquivo.CopyTo(ms);

                            float width = model.Width;
                            float height = model.Height;
                            var image = new Bitmap(System.Drawing.Image.FromStream(ms));
                            float scaleWidth = width > 0 ? width / image.Width : 0;
                            float scaleHeight = height > 0 ? height / image.Height : 0;
                            float scale = Math.Min(scaleWidth > 0 ? scaleWidth : scaleHeight, scaleHeight > 0 ? scaleHeight : scaleWidth);

                            var bmp = new Bitmap((int)(image.Width * scale), (int)(image.Height * scale));
                            using (var graph = Graphics.FromImage(bmp))
                            {
                                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graph.CompositingQuality = CompositingQuality.HighQuality;
                                graph.SmoothingMode = SmoothingMode.AntiAlias;
                                graph.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graph.DrawImage(image, 0, 0, bmp.Width, bmp.Height);
                            }

                            // Gera o nome do arquivo para download
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(arquivo.FileName);
                            string fileExtension = ".png";
                            string fileName = $"{fileNameWithoutExtension}-{bmp.Width}x{bmp.Height}{fileExtension}";

                            // Converte a imagem para um MemoryStream para envio
                            using (var outputStream = new MemoryStream())
                            {
                                bmp.Save(outputStream, ImageFormat.Png);
                                outputStream.Seek(0, SeekOrigin.Begin);

                                // Retorna o arquivo para download
                                return File(outputStream.ToArray(), "image/png", fileName);
                            }
                        }
                    }
                }

                return BadRequest(new { message = "Nenhum arquivo processado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
