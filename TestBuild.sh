# Github workflows
# Made by SmallPP420
# the size of this program is gonna be huge xd
dotnet publish "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/Meme Downloader 2022.csproj" --output "build\\" --arch x64 --os linux -c release --self-contained true # linux build
cd /home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/build/
chmod +x "Meme Downloader 2022"
./"Meme Downloader 2022" -ci
