namespace FetchInfo;

public class ImageSaver
{
    public static async Task SaveImageAsync(Stream imageStream, string fileName)
    {
        // Ensure the 'photos' directory exists
        var photosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "photos");
        Directory.CreateDirectory(photosDirectory);

        // Combine the directory path with the file name
        var filePath = Path.Combine(photosDirectory, fileName);

        if(File.Exists(filePath))
        {
            Console.WriteLine($"File {fileName} already exists");
            return;
        }
        
        // Save the image stream to the specified file
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await imageStream.CopyToAsync(fileStream);
    }
}