using System.IO.Enumeration;

namespace Dottik.MemeDownloader.Downloader;
public enum FileTypes
{
    /// <summary>
    /// Image Media Type.
    /// </summary>
    Image,
    /// <summary>
    /// MultiMedia Media Type.
    /// </summary>
    Video,
    /// <summary>
    /// Audio Media Type.
    /// </summary>
    Audio,
    /// <summary>
    /// Unknown Media Type.
    /// </summary>
    Unknown
}
public struct FileInformation
{
    public bool isGallery { get; set; }
    public string FileExtension { get; set; }
    public string FileName { get; set; }
    public FileTypes FileTypes { get; set; }
    public FileInformation(bool isGallery, string FileExtension, string FileName, FileTypes FileTypes) {
        this.isGallery = isGallery;
        this.FileExtension = FileExtension;
        this.FileName = FileName;
        this.FileTypes = FileTypes;
    }
}
