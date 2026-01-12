using Supabase.Storage;

namespace SistemaGestionVentas.Services
{
    public class SupabaseStorageService
    {
        private readonly Supabase.Client _supabase;

        public SupabaseStorageService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folder = "productos")
        {
            ArgumentNullException.ThrowIfNull(file);
            if (file.Length == 0)
                throw new ArgumentException("Archivo no válido");

            var bucket = _supabase.Storage.From("sgv");
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            var folderPrefix = string.IsNullOrEmpty(folder) ? "" : $"{folder}/";
            var fileName = $"{folderPrefix}{Guid.NewGuid()}_{file.FileName ?? "unknown"}";

            var result = await bucket.Upload(
                bytes,
                fileName,
                new Supabase.Storage.FileOptions { CacheControl = "3600", Upsert = false }
            );

            if (!string.IsNullOrEmpty(result))
            {
                return bucket.GetPublicUrl(fileName);
            }
            return null;
        }

        /// Elimina un archivo del bucket "sgv" en la carpeta especificada.
        /// Retorna true si el archivo fue eliminado exitosamente, false en caso contrario.
        public async Task<bool> DeleteFileAsync(string fileName, string folder = "")
        {
            ArgumentNullException.ThrowIfNull(fileName);

            var bucket = _supabase.Storage.From("sgv");
            var folderPrefix = string.IsNullOrEmpty(folder) ? "" : $"{folder}/";
            var fullFileName = $"{folderPrefix}{fileName}";
            var result = await bucket.Remove(new List<string> { fullFileName });
            return result?.Count > 0;
        }
    }
}
