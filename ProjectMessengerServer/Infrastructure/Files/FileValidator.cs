namespace ProjectMessengerServer.Infrastructure.Files
{
    public class FileValidator
    {
        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new()
        {
            [".jpg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new() { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".png"] = new() { new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
            [".webp"] = new() { new byte[] { 0x52, 0x49, 0x46, 0x46 } },
            [".mp4"] = new() { new byte[] { 0x00, 0x00, 0x00 } },
            [".pdf"] = new() { new byte[] { 0x25, 0x50, 0x44, 0x46 } },
            [".zip"] = new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
            [".docx"] = new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
            [".pptx"] = new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
            [".mp3"] = new() { new byte[] { 0x49, 0x44, 0x33 }, new byte[] { 0xFF, 0xFB } },
            [".webm"] = new() { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } }
        };

        public static bool IsValidExtension(string extension)
        {
            return FileSignatures.ContainsKey(extension.ToLower());
        }

        public static async Task<bool> IsValidSignatureAsync(IFormFile file, string extension)
        {
            if (!FileSignatures.ContainsKey(extension.ToLower()))
                return false;

            using var stream = file.OpenReadStream();
            var headerBytes = new byte[8];
            await stream.ReadAsync(headerBytes, 0, headerBytes.Length);

            return FileSignatures[extension]
                .Any(sig => headerBytes.Take(sig.Length).SequenceEqual(sig));
        }
    }
}
