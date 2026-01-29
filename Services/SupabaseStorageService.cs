using System;
using System.Linq;
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

        public async Task<(bool ok, string? url, string? error)> UploadImageAsync(
            IFormFile? file,
            string folder = "productos"
        )
        {
            if (file == null)
                return (false, null, "Archivo nulo");

            if (file.Length == 0)
                return (false, null, "Archivo vacío");

            try
            {
                var bucket = _supabase.Storage.From("sgv");

                var folderPrefix = string.IsNullOrWhiteSpace(folder) ? "" : $"{folder}/";
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{folderPrefix}{Guid.NewGuid()}{extension}";

                using var memoryStream = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var result = await bucket.Upload(
                    bytes,
                    fileName,
                    new Supabase.Storage.FileOptions { CacheControl = "3600", Upsert = false }
                );

                if (string.IsNullOrEmpty(result))
                    return (false, null, "Supabase no devolvió resultado");

                var url = bucket.GetPublicUrl(fileName);
                return (true, url, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// Retorna true si el archivo fue eliminado exitosamente, false en caso contrario.
        public async Task<(bool ok, string? error)> DeleteFileAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (false, "URL vacía");

            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    return (false, "URL inválida");

                // Supabase public URL tipica:
                // /storage/v1/object/public/{bucket}/{path}
                var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                var publicIndex = Array.IndexOf(parts, "public");
                if (publicIndex < 0 || parts.Length <= publicIndex + 2)
                    return (false, "Formato de URL no compatible");

                var bucketName = parts[publicIndex + 1];
                var fileName = string.Join("/", parts.Skip(publicIndex + 2));

                var bucket = _supabase.Storage.From(bucketName);
                var result = await bucket.Remove(new List<string> { fileName });

                if (result == null || result.Count == 0)
                    return (false, "Archivo no eliminado o no existente");

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
