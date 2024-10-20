using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Net;

namespace webapi.Services
{
    public class PdfService
    {
        // Método para generar PDF de una Dieta
 // Método para generar PDF de la dieta
        public byte[] GenerarDietaPdf(Dieta dieta)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Dieta de {dieta.Nombre}";

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
            gfx.DrawString($"Plan Alimenticio - {dieta.Nombre}", headerFont, XBrushes.Black, new XRect(0, 40, page.Width, 50), XStringFormats.TopCenter);

            XFont dateFont = new XFont("Arial", 10, XFontStyle.Regular);
            gfx.DrawString($"Fecha de Creación: {DateTime.Now:dd/MM/yyyy}", dateFont, XBrushes.Gray, new XRect(0, 70, page.Width, 50), XStringFormats.TopCenter);

            gfx.DrawLine(XPens.Gray, marginLeft, 90, page.Width - marginRight, 90);

            // Variables para el contenido
            XFont mealTitleFont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont foodItemFont = new XFont("Arial", 12, XFontStyle.Regular);
            XFont notesFont = new XFont("Arial", 10, XFontStyle.Italic);

            // Recorrer comidas de la dieta
            foreach (var comida in dieta.Comidas)
            {
                // Título de la comida
                gfx.DrawString($"Comida - {comida.Hora}", mealTitleFont, XBrushes.Black, new XRect(marginLeft, currentY, page.Width - marginRight, 30), XStringFormats.TopLeft);
                currentY += 30;

                // Alimentos de cada comida
                foreach (var alimento in comida.Alimentos)
                {
                    gfx.DrawString($"{alimento.Nombre} - {alimento.Cantidad} {alimento.Unidad}", foodItemFont, XBrushes.Black, new XRect(marginLeft + 20, currentY, page.Width - marginRight, 20), XStringFormats.TopLeft);
                    currentY += 20;
                }

                currentY += 20; // Espacio entre comidas

                // Verificar si es necesario agregar una nueva página
                if (currentY > page.Height - 100)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    currentY = marginTop;
                    pageCount++;

                    // Encabezado en la nueva página
                    gfx.DrawString($"Plan Alimenticio - {dieta.Nombre}", headerFont, XBrushes.Black, new XRect(0, 40, page.Width, 50), XStringFormats.TopCenter);
                    gfx.DrawString($"Fecha de Creación: {DateTime.Now:dd/MM/yyyy}", dateFont, XBrushes.Gray, new XRect(0, 70, page.Width, 50), XStringFormats.TopCenter);
                    gfx.DrawLine(XPens.Gray, marginLeft, 90, page.Width - marginRight, 90);
                }
            }

            // Notas adicionales de la dieta
            gfx.DrawString("Notas adicionales:", mealTitleFont, XBrushes.Black, new XRect(marginLeft, currentY, page.Width - marginRight, 30), XStringFormats.TopLeft);
            currentY += 30;
            gfx.DrawString(dieta.Notas, notesFont, XBrushes.Black, new XRect(marginLeft + 20, currentY, page.Width - marginRight, 50), XStringFormats.TopLeft);

            // Pie de página con número de página
            gfx.DrawString($"Página {pageCount}", dateFont, XBrushes.Gray, new XRect(0, page.Height - 40, page.Width, 50), XStringFormats.BottomCenter);

            // Guardar el PDF en memoria
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream, false);
                return stream.ToArray();
            }
        }

        // Método para generar PDF de la rutina (mencionado previamente)

    

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
