using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using System.Web;

namespace Redimensionador_de_Imagens.Models
{
    public class ArquivosProporcaoModel
    {
        public List<IFormFile> Arquivos { get; set; } = new List<IFormFile>();
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
