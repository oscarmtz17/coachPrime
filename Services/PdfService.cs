using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Globalization;
using System.Net;

namespace webapi.Services
{
    public class PdfService
    {
        private readonly S3Service _s3Service;

        public PdfService(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        // Método para generar PDF de una Dieta
        public byte[] GenerarDietaPdf(Dieta dieta, string userId)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Dieta de {dieta.Nombre}";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Variables de estilos y márgenes
            int marginLeft = 40;
            int marginRight = 40;
            int marginTop = 120; // Ajustar margen superior para respetar el header
            int currentY = marginTop; // Asegurarse de comenzar después del header
            int pageCount = 1;

            // Encabezado
            XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
            XFont dateFont = new XFont("Arial", 10, XFontStyle.Regular);

            // Cargar el logo del bucket
            string logoKey = $"private/{userId}/logo/"; // Ruta base del logo en el bucket
            try
            {
                var logos = _s3Service.ListImagesInFolderAsync(logoKey).Result; // Lista de imágenes en la carpeta del logo
                if (logos.Any())
                {
                    var logoUrl = logos.First(); // Usamos el primer logo encontrado
                    using (var client = new WebClient())
                    {
                        byte[] logoBytes = client.DownloadData(logoUrl);
                        using (MemoryStream ms = new MemoryStream(logoBytes))
                        {
                            XImage logo = XImage.FromStream(() => ms);
                            double originalWidth = logo.PixelWidth;
                            double originalHeight = logo.PixelHeight;

                            double maxWidth = 150;
                            double maxHeight = 50;

                            double widthScale = maxWidth / originalWidth;
                            double heightScale = maxHeight / originalHeight;
                            double scaleFactor = Math.Min(widthScale, heightScale); // Escalar proporcionalmente

                            double finalWidth = originalWidth * scaleFactor;
                            double finalHeight = originalHeight * scaleFactor;

                            gfx.DrawImage(logo, marginLeft, 20, finalWidth, finalHeight); // Dibujar el logo ajustado
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el logo: {ex.Message}");
            }

            // Fecha en la parte superior derecha
            gfx.DrawString($"{DateTime.Now:dd/MM/yyyy}", dateFont, XBrushes.Gray, new XRect(0, 30, page.Width - marginRight, 50), XStringFormats.TopRight);

            // Título centrado
            gfx.DrawString($"Plan Alimenticio - {dieta.Nombre}", headerFont, XBrushes.Black, new XRect(0, 80, page.Width, 30), XStringFormats.TopCenter);
            gfx.DrawLine(XPens.Gray, marginLeft, 110, page.Width - marginRight, 110);

            // Ajustar `currentY` para que el contenido no toque el header
            currentY = marginTop;

            // Cuerpo del PDF
            XFont mealTitleFont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont foodItemFont = new XFont("Arial", 12, XFontStyle.Regular);
            foreach (var comida in dieta.Comidas)
            {
                gfx.DrawString($"Comida {comida.Hora}", mealTitleFont, XBrushes.Black, new XRect(marginLeft, currentY, page.Width - marginRight, 20), XStringFormats.TopLeft);
                currentY += 20;

                foreach (var alimento in comida.Alimentos)
                {
                    gfx.DrawString($"- {alimento.Nombre}: {alimento.Cantidad} {alimento.Unidad}", foodItemFont, XBrushes.Black, new XRect(marginLeft + 20, currentY, page.Width - marginRight, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }

                currentY += 10;

                // Verificar si es necesario agregar una nueva página
                if (currentY > page.Height - 100)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    currentY = marginTop;

                    gfx.DrawString($"Plan Alimenticio - {dieta.Nombre}", headerFont, XBrushes.Black, new XRect(0, 80, page.Width, 30), XStringFormats.TopCenter);
                    gfx.DrawLine(XPens.Gray, marginLeft, 110, page.Width - marginRight, 110);
                }
            }

            // Dibujar "Notas adicionales:" en negritas
            gfx.DrawString("Notas adicionales:", mealTitleFont, XBrushes.Black, new XRect(marginLeft, currentY, page.Width - marginRight, 30), XStringFormats.TopLeft);

            // Calcular el ancho del texto "Notas adicionales:" para posicionar el contenido después de él
            double textWidth = gfx.MeasureString("Notas adicionales:", mealTitleFont).Width;

            // Dibujar el contenido de dieta.Notas con estilo normal
            gfx.DrawString(dieta.Notas, foodItemFont, XBrushes.Black, new XRect(marginLeft + textWidth + 5, currentY, page.Width - marginRight - textWidth, 30), XStringFormats.TopLeft);

            currentY += 30;

            // Pie de página con número de página
            gfx.DrawString($"Página {pageCount}", dateFont, XBrushes.Gray, new XRect(0, page.Height - 40, page.Width, 50), XStringFormats.BottomCenter);

            // Guardar el PDF en memoria
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream, false);
                return stream.ToArray();
            }
        }




        // Método para generar PDF de una Rutina
        public byte[] GenerarRutinaPdf(Rutina rutina)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Rutina de {rutina.Nombre}";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Variables de estilos y márgenes
            int marginLeft = 40;
            int marginRight = 40;
            int marginTop = 100;
            int currentY = marginTop;
            int pageCount = 1;

            // Encabezado
            XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
            gfx.DrawString($"Rutina de Entrenamiento - {rutina.Nombre}", headerFont, XBrushes.Black, new XRect(0, 40, page.Width, 50), XStringFormats.TopCenter);

            XFont dateFont = new XFont("Arial", 10, XFontStyle.Regular);
            gfx.DrawString($"Fecha de Creación: {DateTime.Now:dd/MM/yyyy}", dateFont, XBrushes.Gray, new XRect(0, 70, page.Width, 50), XStringFormats.TopCenter);

            gfx.DrawLine(XPens.Gray, marginLeft, 90, page.Width - marginRight, 90);

            // Recorrer días de entrenamiento
            XFont dayTitleFont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont exerciseFont = new XFont("Arial", 12, XFontStyle.Bold);
            XFont detailsFont = new XFont("Arial", 10, XFontStyle.Regular);

            foreach (var dia in rutina.DiasEntrenamiento)
            {
                // Título del día
                gfx.DrawString($"Día {dia.DiaSemana}", dayTitleFont, XBrushes.Black, new XRect(marginLeft, currentY, page.Width - marginRight, 30), XStringFormats.TopLeft);
                currentY += 30;

                foreach (var agrupacion in dia.Agrupaciones)
                {
                    // Tabla de ejercicios
                    gfx.DrawString("Ejercicio", detailsFont, XBrushes.Black, new XRect(marginLeft, currentY, 100, 20), XStringFormats.TopLeft);
                    gfx.DrawString("Series", detailsFont, XBrushes.Black, new XRect(marginLeft + 150, currentY, 100, 20), XStringFormats.TopLeft);
                    gfx.DrawString("Repeticiones", detailsFont, XBrushes.Black, new XRect(marginLeft + 250, currentY, 100, 20), XStringFormats.TopLeft);

                    currentY += 20;

                    foreach (var ejercicioAgrupado in agrupacion.EjerciciosAgrupados)
                    {
                        var ejercicio = ejercicioAgrupado.Ejercicio;
                        gfx.DrawString(ejercicio.Nombre, exerciseFont, XBrushes.Black, new XRect(marginLeft, currentY, 100, 20), XStringFormats.TopLeft);
                        gfx.DrawString(ejercicio.Series.ToString(), detailsFont, XBrushes.Black, new XRect(marginLeft + 150, currentY, 100, 20), XStringFormats.TopLeft);
                        gfx.DrawString(ejercicio.Repeticiones.ToString(), detailsFont, XBrushes.Black, new XRect(marginLeft + 250, currentY, 100, 20), XStringFormats.TopLeft);

                        // Cargar imagen del ejercicio si existe
                        if (!string.IsNullOrEmpty(ejercicio.ImagenUrl))
                        {
                            try
                            {
                                using (var client = new WebClient())
                                {
                                    byte[] imageBytes = client.DownloadData(ejercicio.ImagenUrl);
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        XImage image = XImage.FromStream(() => ms);  // Corregido para usar una función lambda que retorna el stream
                                        gfx.DrawImage(image, marginLeft + 350, currentY - 10, 150, 100); // Tamaño máximo 150x100
                                    }
                                }
                            }
                            catch
                            {
                                // Si no se puede cargar la imagen, omitimos
                            }
                        }

                        currentY += 120; // Espacio después de cada ejercicio
                    }

                    currentY += 20; // Espacio después de cada agrupación

                    // Verificar si es necesario agregar una nueva página
                    if (currentY > page.Height - 100)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        currentY = marginTop;
                        pageCount++;

                        // Encabezado en la nueva página
                        gfx.DrawString($"Rutina de Entrenamiento - {rutina.Nombre}", headerFont, XBrushes.Black, new XRect(0, 40, page.Width, 50), XStringFormats.TopCenter);
                        gfx.DrawString($"Fecha de Creación: {DateTime.Now:dd/MM/yyyy}", dateFont, XBrushes.Gray, new XRect(0, 70, page.Width, 50), XStringFormats.TopCenter);
                        gfx.DrawLine(XPens.Gray, marginLeft, 90, page.Width - marginRight, 90);
                    }
                }
            }

            // Pie de página con número de página
            gfx.DrawString($"Página {pageCount}", detailsFont, XBrushes.Gray, new XRect(0, page.Height - 40, page.Width, 50), XStringFormats.BottomCenter);

            // Guardar el PDF en memoria
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream, false);
                return stream.ToArray();
            }
        }


    }
}
