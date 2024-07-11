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
            if (model == null)
                throw new Exception("Sem objeto");

            if (model.Arquivos.Count == 0 || model.Width <= 0 || model.Height <= 0)
                throw new Exception("Todos os campos são essenciais");

            // Cria o diretório "Importacoes" se não existir
            string importacoesPath = "Importacoes";
            if (!Directory.Exists(importacoesPath))
            {
                Directory.CreateDirectory(importacoesPath);
            }

            foreach (var arquivo in model.Arquivos)
            {
                if (arquivo.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        arquivo.CopyTo(ms);

                        float width = model.Width;
                        float height = model.Height;
                        var brush = new SolidBrush(Color.Black);
                        var image = new Bitmap(Image.FromStream(ms));
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)(image.Width * scale), (int)(image.Height * scale));
                        using (var graph = Graphics.FromImage(bmp))
                        {
                            graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graph.CompositingQuality = CompositingQuality.HighQuality;
                            graph.SmoothingMode = SmoothingMode.AntiAlias;
                            graph.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            var scaleWidth = (int)(image.Width * scale);
                            var scaleHeight = (int)(image.Height * scale);
                            graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                            graph.DrawImage(image, 0, 0, scaleWidth, scaleHeight);
                        }

                        // Gera um nome de arquivo único
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(arquivo.FileName);
                        string fileExtension = ".png";
                        string fileName = $"{fileNameWithoutExtension}-{width}{fileExtension}";
                        string filePath = Path.Combine(importacoesPath, fileName);

                        int fileCount = 1;
                        while (System.IO.File.Exists(filePath))
                        {
                            fileName = $"{fileNameWithoutExtension}-{width}({fileCount}){fileExtension}";
                            filePath = Path.Combine(importacoesPath, fileName);
                            fileCount++;
                        }

                        bmp.Save(filePath, ImageFormat.Png);
                    }
                }
            }

            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
