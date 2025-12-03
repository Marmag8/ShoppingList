using CommunityToolkit.Maui.Storage;
using ShoppingList.Models;

namespace ShoppingList.Services
{
    internal static class ExportOptions
    {
        public static async Task ExportAsync()
        {
            string appPath = Utils.GetDataFilePath();
            if (!File.Exists(appPath))
                throw new FileNotFoundException("Brak pliku do eksportu.", appPath);

            using FileStream readStream = File.OpenRead(appPath);

            FileSaverResult result = await FileSaver.Default.SaveAsync(
                $"ShoppingList{DateTime.Now.ToFileTime()}.xml",
                readStream,
                CancellationToken.None);

            if (!result.IsSuccessful)
                throw result.Exception ?? new OperationCanceledException("Anulowano zapis.");
        }

        public static async Task ImportAsync(Action<ListItemModel> appendItem)
        {
            FilePickerFileType xmlTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".xml" } },
                { DevicePlatform.Android, new[] { "application/xml", "text/xml" } },
                { DevicePlatform.iOS, new[] { "public.xml" } },
                { DevicePlatform.MacCatalyst, new[] { "public.xml" } }
            });

            FileResult picked = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Wybierz plik ShoppingList.xml do importu",
                FileTypes = xmlTypes
            });

            if (picked is null)
                throw new OperationCanceledException("Anulowano import.");

            string filePath = Utils.GetDataFilePath();

            using (Stream src = await picked.OpenReadAsync())
            using (FileStream dst = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                await src.CopyToAsync(dst);
            }

            List<ListItemModel> itemsFromFile = Utils.FromXML().Items;
            foreach (ListItemModel item in itemsFromFile)
                appendItem(item);
        }
    }
}
