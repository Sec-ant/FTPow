# FTPow

 FTPow 是 FTP Open With 的缩写，也可以理解为 FTP Power 或 FTP On Windows。FTPow 目的在于为用户提供一个可以高度自定义 FTP 协议文件打开方式的工具。

### 背景

虽然 Windows 资源管理器为用户提供了通过添加网络位置的方式直接连接 FTP 并浏览其中的文件的功能，但是针对 FTP 上的文件访问操作，微软做了很多限制：

- 用户无法像访问本地文件一样，通过右键文件-选择打开方式来选定任意的程序作为默认应用，用户可选择的应用一般只有网页浏览器和微软商店中的一些 FTP 客户端程序；
- 微软为注册表中默认应用设定的键旁还设置了一个 hash 校验码键，该 hash 校验码与默认应用的 ProgId 相关联。每当系统指定一个新的默认应用时，系统会自动通过某种算法计算出一个 hash 校验码。如果想要直接改变注册表中的 ProgId，则同时需要提供正确的 hash 校验码。若校验码错误，则该 ProgId 无效。而这个 hash 算法是不公开的。

由于以上限制，用户在资源管理器内双击文件后，基本只能通过网页或客户端打开 FTP 中的文件链接。这对于一些文本文件或者压缩包来说，并没有很大的影响，这些文件本身也是需要下载到本地再读取访问的。但是对于可以流式传输并读取的大型文件来说，会浪费很多时间和硬盘空间用于下载文件。

以很典型的视频文件为例，目前多数播放器均支持网络串流播放，如 PotPlayer 可以通过下面这个命令播放网络媒体资源（该调用方法同播放本地文件的方法一致）：

```powershell
PotPlayerMini64.exe "URL_of_file"
```

FTPow 的初衷，就是要实现在资源管理器的 FTP 网络位置中，用户通过双击视频文件的方式，就能够自动调用 PotPlayer 播放，以优化用户体验。

### 实现思路

好消息是，微软的 hash 算法已被成功逆向。目前网络上至少有两个工具可以实现让用户自定义文件类型或协议类型所关联的默认应用。而由 Danysys 开发的 SFTA 就是其中之一，该项目相关代码已在 GitHub 开源，关于这个工具的具体使用方式可移步这个文章查看。

那么，如果我们将 FTP 协议的默认应用关联到 PotPlayer，问题是否就解决了呢？

并没有，主要原因有以下几点：

1. 并非所有文件都要用 P 打开

2. FTP 密码问题

   ```
   ftp://{username}@{ip address or domain}/{some folder path}/file.mp4
   ```

   ```
   ftp://{username}:{password}@{ip address or domain}/{some folder path}/file.mp4
   ```

3. URL 编码问题

4. 字符编码问题

5. 不同的文件类型用不同的应用打开

中间层 FTPow

### 安装

下载 SFTA 和 FTPow 压缩包并在任意合适位置解压即可。

SFTA下载地址：

FTPow下载地址：见 [Release 页面](https://github.com/Sec-ant/FTPow/releases)。

### 运行环境要求

[.NET Framework 4.6+ Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net46)

### 用法

1. 切换到 SFTA.exe 文件所在目录，在文件夹内 `shift` + `右键`，选择“在此处打开Powershell窗口”；

2. 将 FTPow.exe 注册为 FTP 协议关联的默认应用：

   ```powershell
   ./SFTA.exe --reg "C:\\Program Files\\FTPow\\FTPow.exe" "FTP" "FTPOpenWith"
   ```

   注意：

   - `"C:\\Program Files\\FTPow\\FTPow.exe"` 应替换为自己计算机上 FTPow.exe 的文件路径；
   - `"FTP"` 表示协议类型，不区分大小写；
   - `"FTPOpenWith"` 表示该应用的一个自定义 ProgId。不建议使用很短的字符串，避免与其它 ID 发生碰撞，此项参数可选。

3. 配置 FTPow.exe 同目录下的 config.json 配置文件；

4. 双击一个 FTP 上的文件打开试试吧！

### 配置文件说明



### 注意

- 不需要管理员权限，单用户生效

- windows 查封



### 局限性

- 



### 证书

GPLv3.0

### 捐赠

支付宝和微信