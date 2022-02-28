# Github workflows
# Made by SmallPP420
# the size of this program is gonna be huge xd
dotnet publish "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/Meme Downloader 2022.csproj" /p:DefineConstants=WINDOWS --output "build\\" --arch x64 --os win -c release --self-contained true # win build

dotnet publish "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/Meme Downloader 2022.csproj" /p:DefineConstants=WINDOWS --output "build-debug\\" --arch x64 --os win -c debug --self-contained true # win build

dotnet publish "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/Meme Downloader 2022.csproj" /p:DefineConstants=LINUX --output "build\\" --arch x64 --os linux -c release --self-contained true # linux build

dotnet publish "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/Meme Downloader 2022.csproj" /p:DefineConstants=LINUX --output "build-debug\\" --arch x64 --os linux -c debug --self-contained true # linux build

# Move debug build
mv "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/build-debug/Meme Downloader 2022.exe" "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/build/DEBUG-Meme Downloader 2022.exe"

# Move debug build
mv "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/build-debug/Meme Downloader 2022" "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/build/Linux-DEBUG-Meme Downloader 2022"

mv "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/build/Meme Downloader 2022" "/home/runner/work/Meme-Downloader-2022/Meme-Downloader-2022/build/Linux-Meme Downloader 2022"
