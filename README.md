# FTPow

 FTPow 是 FTP Open With 的缩写，也可以理解为 FTP Power 或 FTP On Windows。FTPow 目的在于为用户提供一个可以高度自定义 FTP 协议文件打开方式的工具。

### 背景

虽然 Windows 资源管理器为用户提供了通过添加网络位置的方式直接连接 FTP 并浏览其中的文件的功能，但是针对 FTP 上的文件访问操作，微软做了很多限制：

- 用户无法像访问本地文件一样，通过右键文件-选择打开方式来选定任意的程序作为默认应用，用户可选择的应用一般只有网页浏览器和微软商店中的一些 FTP 客户端程序；
- 微软为注册表中默认应用设定的 ProgId 键旁还设置了一个 hash 校验码键，该 hash 校验码与默认应用的 ProgId 相关联。每当系统指定一个新的默认应用时，系统会自动通过某种算法计算出一个 hash 校验码。如果想要直接改变注册表中的 ProgId，则同时需要提供正确的 hash 校验码。若校验码错误，则该 ProgId 无效。而这个 hash 算法是不公开的。

由于以上限制，用户在资源管理器内双击文件后，基本只能通过网页或客户端打开 FTP 中的文件链接。这对于一些文本文件或者压缩包来说，并没有很大的影响，这些文件本身也是需要下载到本地再读取访问的。但是对于可以流式传输并读取的大型文件来说，会浪费很多时间和硬盘空间用于下载文件。

以很典型的视频文件为例，目前多数播放器均支持网络串流播放，如 [PotPlayer](https://potplayer.daum.net/) 可以通过[下面这个命令播放网络媒体资源](https://stackoverflow.com/questions/31196481/open-youtube-links-in-external-videoplayer-from-browser)（该调用方法同播放本地文件的方法一致）：

```powershell
PotPlayerMini64.exe "URL_of_file"
```

FTPow 开发的最初目的，就是要实现在资源管理器的 FTP 网络位置中，让用户通过双击视频文件的方式，就能够自动调用 PotPlayer 播放，以优化用户体验。

### 实现思路

好消息是，目前主要有两种办法绕过上面的限制，从而实现让用户自定义文件类型或协议类型所关联的默认应用：1. 通过逆向微软的 hash 算法（如 [SetUserFTA](https://kolbi.cz/blog/2017/10/25/setuserfta-userchoice-hash-defeated-set-file-type-associations-per-user/)，但作者闭源了）；2. 劫持系统内部计算 hash 的接口（由 [Danysys](https://danysys.com/) 开发的 [SFTA](https://github.com/DanysysTeam/SFTA)，但劫持接口的办法是由[看雪论坛里面的国人实现的](https://bbs.pediy.com/thread-213954.htm)）。本软件借助了 SFTA，该项目相关代码已在 [GitHub](https://github.com/DanysysTeam/SFTA) 开源，关于这个工具的具体使用方法可移步[这个文章](https://danysys.com/set-file-type-association-default-application-command-line-windows-10-userchoice-hash-internal-method/)查看。

那么，如果我们通过 SFTA ，将 FTP 协议的默认应用关联到 PotPlayer，问题是否就解决了呢？

并没有，主要原因有以下几点：

1. 首先，并非所有 FTP 上面的文件都需要用 PotPlayer 打开，对于小体积的文本文档、压缩包等，我们还是希望能够下载到本地再访问的。如果将所有文件都和 PotPlayer 进行关联，则为访问这类文件增加了不便性；

2. 对于使用有密码的账户建立的 FTP 连接，当用户双击其中的文件时，传给默认应用的文件地址链接中是不包含 FTP 账户密码的，如：

   ```
   ftp://{username}@{ip address or domain}/{some folder path}/file.mp4
   ```

   但如果想在 PotPlayer 中播放这类视频文件，我们需要在地址链接中提供密码，如：

   ```
   ftp://{username}:{password}@{ip address or domain}/{some folder path}/file.mp4
   ```

3. PotPlayer 能够识别的网络地址链接是不能够被 [URL 转义编码](http://www.ruanyifeng.com/blog/2010/02/url_encoding.html)的，即需要原始字符串。但通过双击文件传入应用中的文件地址链接是经过 URL 转义编码的，因此需要先被解码才能播放；

4. URL 转义编解码没有规定字符与二进制之间的编解码关系。对于中文，有多种字符编解码方式如 UTF-8、GB2312 等，这个在不同系统环境中可能会有区别。如果编解码出错，则同样无法正常播放；

5. 进一步的，我希望并不局限于 PotPlayer 这一个应用。如果可以做到对于 FTP 上不同的文件类型使用不同的应用读取访问，那么灵活性就可以大大提高；

计算机领域类似的问题往往都能通过一个中间层来解决，FTPow 就是这样一个中间层，它会根据配置文件合理处理传入链接，然后再交给相应的应用程序读取访问。

### 安装

下载 SFTA 和 FTPow 压缩包并在任意合适位置解压即可。

SFTA下载地址：https://danysys.com/download/stfa/

FTPow下载地址：见 [releases 页面](https://github.com/Sec-ant/FTPow/releases)。

### 运行环境要求

[.NET Framework 4.6+ Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net46)

### 用法

1. 切换到 SFTA.exe 文件所在目录，在文件夹内 `shift` + `右键`，选择“在此处打开Powershell窗口”；

2. 用 SFTA 将 FTPow.exe 注册为 FTP 协议关联的默认应用：

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

 FTPow.exe 同目录下的 config.json 是用户可自定义的配置文件，为 JSON 解构，其根对象结构如下：

- `servers` **数组**

  FTPow 支持配置多个 FTP 服务器信息。该数组中每一个元素都是一个对象，具有以下结构：

  - `address` **字符串**

    FTP 服务器地址，可使用 IP 地址或域名。

  - `username` **字符串**

    FTP 账户名称，无需转义。若无账户名称该项可设为空字符串。

  - `password` **字符串**

    FTP 账户密码，无需转义。若无账户密码该项可设为空字符串。

- `apps` **数组**

  FTPow 支持配置根据不同的文件类型后缀名使用不同的应用读取。该数组中每一个元素都是一个对象，具有以下结构：

  - `programPath`  **字符串**

    应用程序的路径。注意转义路径分隔符 `\\` 。

  - `command` **数组**

    应用程序的命令行参数。数组中每一个元素都是字符串。在 FTPow 中，应用程序所获得的文件地址链接会被**插入到这个数组中的每个逗号分隔符的位置**，然后再被合并为一个字符串作为应用程序的参数被调用。

    注意，如果想要实现类似如下调用方式：

    ```
    myApp.exe %1
    ```

    则需要将该项设置为 `["", ""]` 或 `["\"", "\""]`。

  - `extList` **数组**

    默认使用该应用程序打开的文件类型后缀名。数组中每一个元素都是字符串，表示文件的后缀名。注意后缀名中不包含前缀点号“.”。

  - `queryString` **字符串**

    FTP 文件地址链接可选的 URL 参数，格式如下：

    ```
    A&B=c&D=e...
    ```

    若无参数该项可设为空字符串。本项配置亦可通过 `command` 实现。

  - `decodePlus` **布尔**

    由于空格 ` ` 在 URL 中可被编码为 `%20` 或 `+`，此项为 `true` 表示将 URL 中的 `+` 解码为空格 ` `，为 `false` 表示对 `+` 不进行解码。该项只有在 `deocde` 项不为 `NONE` 时才有效。

  - `decode` **字符串**

    设定 URL 解码时字符的编解码方式。支持：`UTF8`、`UTF32`、`ASCII`、`GB2312`、`AUTO`、`DEFAULT` 和 `NONE`。其中 `NONE` 表示不对 URL 链接进行解码，`AUTO` 和 `DEFAULT` 表示使用系统默认字符编码方式对链接进行解码，但并非意味着与 FTP 连接的编码方式一致。中文 Windows 操作系统建议优先尝试 `GB2312`。
  
- `fallback` **对象**

  当 FTPow 在配置文件中没有找到匹配传入链接的文件类型时，会使用一个统一的应用处理这类文件，该应用在本项进行设置。本项与 `apps` 数组中的元素格式一致，但没有 `extList` 和 `queryString` 这两个键。一般这个应用可以设置为初始时系统 FTP 协议所关联的默认应用，如网页浏览器。

配置文件的一个示例如下：

```json
{
  "servers": [
    {
      "address": "192.168.0.100",
      "username": "admin",
      "password": "adminadmin"
    }
  ],
  "apps": [
    {
      "programPath": "C:\\Program Files\\DAUM\\PotPlayer\\PotPlayerMini64.exe",
      "command": ["", ""],
      "extList": [ "mkv", "mp4", "avi", "ts", "mp3", "m2ts", "mpg", "mpeg", "wmv", "flac" ],
      "queryString": "WithCaption&passive",
      "decodePlus": false,
      "decode": "GB2312"
    }
  ],
  "fallback": {
    "programPath": "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
    "command": ["", ""],
    "decodePlus": false,
    "decode": "GB2312"
  }
}
```

### 局限性

- 目前本工具仅支持 FTP 协议，不支持 FTPS 或其他协议；
- 若将命令行应用与某些文件类型关联，则当用户打开这类文件时，无法自动弹出命令行窗口与用户交互。

### 许可

[GNU General Public License v3.0 or later](https://spdx.org/licenses/GPL-3.0-or-later.html)

### 捐赠

<table><tbody><tr><td>支付宝 Alipay</td><td>微信 Wechat</td></tr>
<tr><td><img width="200" src="https://i.loli.net/2020/02/28/JPGgHc3UMwXedhv.jpg"></td><td><img width="200" src="https://i.loli.net/2020/03/02/qDQ9Xk8uCHwcaLZ.png"></td></tr></tbody></table>