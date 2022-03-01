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

public struct RedditVideoInformation
{
#nullable enable

    /// <summary>
    /// The download link of the video
    /// </summary>
    public string VideoLink { get; private set; }

    /// <summary>
    /// The download link of the video's audio.
    /// </summary>
    public string? AudioLink { get; private set; }

    /// <summary>
    /// Is the audio valid?
    /// </summary>
    public bool isAudioValid { get; private set; } = false;

    /// <summary>
    /// Is the video a gif?
    /// </summary>
    public bool isGif { get; private set; } = false;

    /// <summary>
    /// Initialize <see cref="RedditVideoInformation"/>
    /// </summary>
    /// <param name="audioLink">The download link of the video's audio.</param>
    /// <param name="videoLink">The download link of the video</param>
    /// <param name="isAudioOfVideoValid">Is the audio valid?</param>
    public RedditVideoInformation(string? audioLink, string videoLink, bool isAudioOfVideoValid, bool isVideoAGif)
    {
        VideoLink = videoLink;
        AudioLink = audioLink;
        isAudioValid = isAudioOfVideoValid;
        isGif = isVideoAGif;
    }

#nullable restore
}