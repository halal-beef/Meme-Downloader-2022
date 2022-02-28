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
    public FileInformation(string PostTitle, string DownloadURL, bool isGallery, bool isNSFW, string FileExtension, string FileName, FileTypes FileTypes)
    {
        this.PostTitle = PostTitle;
        this.DownloadURL = DownloadURL;
        this.IsGallery = isGallery;
        this.isNSFW = isNSFW;
        this.FileExtension = FileExtension;
        this.FileName = FileName;
        this.FileTypes = FileTypes;
    }

    public string DownloadURL { get; set; }
    public string FileExtension { get; set; }
    public string FileName { get; set; }
    public FileTypes FileTypes { get; set; }
    public bool IsGallery { get; set; }
    public bool isNSFW { get; set; }
    public string PostTitle { get; set; }
}