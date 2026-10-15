using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.WebApp.Services;

public class ProductImageStore(string directory) : IProductImageStore
{
    public const long MaxBytes = 5 * 1024 * 1024;

    public async Task<string> SaveAsync(Stream image, long size)
    {
        if (size is <= 0 or > MaxBytes)
            throw new PreOrderDomainException(
                "Ảnh tối đa 5 MB, định dạng PNG, JPEG hoặc WebP.");

        using var content = new MemoryStream();
        var buffer = new byte[81920];
        int read;

        while ((read = await image.ReadAsync(buffer)) > 0)
        {
            if (content.Length + read > MaxBytes)
                throw new PreOrderDomainException("Ảnh vượt quá 5 MB.");

            await content.WriteAsync(buffer.AsMemory(0, read));
        }

        var bytes = content.ToArray();

        string? extension =
            bytes.Length >= 8 &&
            bytes.AsSpan(0, 8).SequenceEqual(
                new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
                ? ".png"
                : bytes.Length >= 3 &&
                  bytes[0] == 255 &&
                  bytes[1] == 216 &&
                  bytes[2] == 255
                    ? ".jpg"
                    : bytes.Length >= 12 &&
                      System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" &&
                      System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP"
                        ? ".webp" : null;

        if (extension == null)
            throw new PreOrderDomainException(
                "Tệp không phải ảnh PNG, JPEG hoặc WebP hợp lệ.");

        Directory.CreateDirectory(directory);

        var filename = Guid.NewGuid().ToString("N") + extension;
        await File.WriteAllBytesAsync(
            Path.Combine(directory, filename), bytes);

        return "/uploads/products/" + filename;
    }

    public Task RemoveAsync(string url)
    {
        const string prefix = "/uploads/products/";

        if (!string.IsNullOrEmpty(url) && url.StartsWith(prefix, StringComparison.Ordinal))
        {
            var filename = url[prefix.Length..];

            if (filename == Path.GetFileName(filename) &&
                Guid.TryParseExact(
                    Path.GetFileNameWithoutExtension(filename),
                    "N", out _))
            {
                File.Delete(Path.Combine(directory, filename));
            }
        }

        return Task.CompletedTask;
    }
}
