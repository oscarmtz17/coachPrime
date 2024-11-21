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



        public byte[] GenerarRutinaPdf(Rutina rutina, string userId)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Rutina de {rutina.Nombre}";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Variables de estilos y márgenes
            int marginLeft = 40;
            int marginRight = 40;
            int marginTop = 120; // Ajustar margen superior para respetar el header
            int marginBottom = 60; // Margen para el pie de página
            int currentY = marginTop;
            int pageCount = 1;

            // Fuentes
            XFont headerFont = new XFont("Arial", 16, XFontStyle.Bold);
            XFont dateFont = new XFont("Arial", 10, XFontStyle.Regular);
            XFont dayTitleFont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont groupFont = new XFont("Arial", 12, XFontStyle.Bold);
            XFont tableFont = new XFont("Arial", 10, XFontStyle.Regular);

            // Encabezado: Logo, fecha y título
            string logoKey = $"private/{userId}/logo/";
            try
            {
                var logos = _s3Service.ListImagesInFolderAsync(logoKey).Result;
                if (logos.Any())
                {
                    var logoUrl = logos.First();
                    using (var client = new WebClient())
                    {
                        byte[] logoBytes = client.DownloadData(logoUrl);
                        using (MemoryStream ms = new MemoryStream(logoBytes))
                        {
                            XImage logo = XImage.FromStream(() => ms);
                            double originalWidth = logo.PixelWidth;
                            double originalHeight = logo.PixelHeight;
                            double maxWidth = 150, maxHeight = 50;
                            double scaleFactor = Math.Min(maxWidth / originalWidth, maxHeight / originalHeight);
                            double finalWidth = originalWidth * scaleFactor;
                            double finalHeight = originalHeight * scaleFactor;
                            gfx.DrawImage(logo, marginLeft, 20, finalWidth, finalHeight);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el logo: {ex.Message}");
            }

            gfx.DrawString($"{DateTime.Now:dd/MM/yyyy}", dateFont, XBrushes.Gray, new XRect(0, 30, page.Width - marginRight, 50), XStringFormats.TopRight);
            gfx.DrawString($"Rutina de Entrenamiento - {rutina.Nombre}", headerFont, XBrushes.Black, new XRect(0, 80, page.Width, 30), XStringFormats.TopCenter);
            gfx.DrawLine(XPens.Gray, marginLeft, 110, page.Width - marginRight, 110);

            currentY = marginTop;

            // Cuerpo del PDF
            foreach (var dia in rutina.DiasEntrenamiento)
            {
                gfx.DrawString($"Día {dia.DiaSemana}", dayTitleFont, XBrushes.Black, new XRect(marginLeft, currentY, page.Width - marginRight, 30), XStringFormats.TopLeft);
                currentY += 30;

                foreach (var agrupacion in dia.Agrupaciones)
                {
                    gfx.DrawString(agrupacion.Tipo, groupFont, XBrushes.Black, new XRect(marginLeft, currentY, page.Width - marginRight, 20), XStringFormats.TopLeft);
                    currentY += 20;

                    // Dibujar tabla de ejercicios
                    gfx.DrawString("Ejercicio", tableFont, XBrushes.Black, new XRect(marginLeft, currentY, 100, 20), XStringFormats.TopLeft);
                    gfx.DrawString("Series", tableFont, XBrushes.Black, new XRect(marginLeft + 150, currentY, 100, 20), XStringFormats.TopLeft);
                    gfx.DrawString("Repeticiones", tableFont, XBrushes.Black, new XRect(marginLeft + 250, currentY, 100, 20), XStringFormats.TopLeft);
                    gfx.DrawString("Ejemplo", tableFont, XBrushes.Black, new XRect(marginLeft + 350, currentY, 100, 20), XStringFormats.TopLeft);

                    currentY += 20;

                    foreach (var ejercicioAgrupado in agrupacion.EjerciciosAgrupados)
                    {
                        var ejercicio = ejercicioAgrupado.Ejercicio;

                        // Calcular el espacio necesario para el ejercicio
                        double maxWidth = 150, maxHeight = 100;
                        double requiredSpace = 20 + maxHeight + 10; // Espacio para texto + imagen + margen

                        // Verificar si hay espacio suficiente en la página
                        if (currentY + requiredSpace > page.Height - marginBottom)
                        {
                            // Agregar pie de página antes de crear una nueva página
                            gfx.DrawString($"Página {pageCount}", dateFont, XBrushes.Gray, new XRect(page.Width - marginRight - 50, page.Height - marginBottom + 10, 50, 20), XStringFormats.BottomRight);
                            pageCount++;

                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            currentY = marginTop;

                            gfx.DrawString($"Rutina de Entrenamiento - {rutina.Nombre}", headerFont, XBrushes.Black, new XRect(0, 80, page.Width, 30), XStringFormats.TopCenter);
                            gfx.DrawLine(XPens.Gray, marginLeft, 110, page.Width - marginRight, 110);
                        }

                        // Dibujar ejercicio
                        gfx.DrawString(ejercicio.Nombre, tableFont, XBrushes.Black, new XRect(marginLeft, currentY, 100, 20), XStringFormats.TopLeft);
                        gfx.DrawString(ejercicio.Series.ToString(), tableFont, XBrushes.Black, new XRect(marginLeft + 150, currentY, 100, 20), XStringFormats.TopLeft);
                        gfx.DrawString(ejercicio.Repeticiones.ToString(), tableFont, XBrushes.Black, new XRect(marginLeft + 250, currentY, 100, 20), XStringFormats.TopLeft);

                        // Imagen del ejercicio o espacio reservado
                        try
                        {
                            if (!string.IsNullOrEmpty(ejercicio.ImagenUrl))
                            {
                                using (var client = new WebClient())
                                {
                                    byte[] imageBytes = client.DownloadData(ejercicio.ImagenUrl);
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        XImage image = XImage.FromStream(() => ms);
                                        double originalWidth = image.PixelWidth;
                                        double originalHeight = image.PixelHeight;
                                        double scaleFactor = Math.Min(maxWidth / originalWidth, maxHeight / originalHeight);
                                        double finalWidth = originalWidth * scaleFactor;
                                        double finalHeight = originalHeight * scaleFactor;

                                        gfx.DrawImage(image, marginLeft + 350, currentY, finalWidth, finalHeight);
                                    }
                                }
                            }
                            else
                            {
                                gfx.DrawRectangle(XPens.Gray, marginLeft + 350, currentY, maxWidth, maxHeight);
                            }
                        }
                        catch
                        {
                            gfx.DrawRectangle(XPens.Gray, marginLeft + 350, currentY, maxWidth, maxHeight);
                        }

                        currentY += (int)maxHeight + 30; // Espacio después de cada ejercicio
                    }

                    currentY += 20; // Espacio entre agrupaciones
                }
            }

            // Pie de página de la última página
            gfx.DrawString($"Página {pageCount}", dateFont, XBrushes.Gray, new XRect(page.Width - marginRight - 50, page.Height - marginBottom + 10, 50, 20), XStringFormats.BottomRight);

            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream, false);
                return stream.ToArray();
            }
        }





    }
}
