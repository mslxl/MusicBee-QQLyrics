# QQ Lyric

This is another lyrics retrieval plugin for music player MusicBee. It can get **synchronized** lyrics from QQ Music(QQ音乐, another cloud music service in China).

It can combine the translation to the lyrics if available, like:  
Blalalala(Original)  
Alalalala(Translation)  
is wrapped to: Blalalala/Alalalalala  
etc..

But if you don't want this function you can disable it in plugin settings.

## Usage
To use this plugin, download it from [github release](https://github.com/mslxl/MusicBee-QQLyrics/releases) page or [BaiduYun(Password:vede)](https://pan.baidu.com/s/155sPKyRGEKPyn5SfNJSjVg?pwd=vede):  

Install(via "Add plugin" button) and enable it in the `Plugin` tab in the preferences.  
And adjust the retrieving provider priority of "QQ Music" in the Tags(2) Tab.  
<!-- If you want a specific song's lyric from NetEase Cloud Music to be matched, you may set the `custom10` tag to the song URL like `https://music.163.com/#/song?id=29126914` (You can directly copy URL like this from the Netease website or client) or `netease=123123` (where "123123" should be your song id) in the music and re-search lyrics. -->

## For non-Chinese users 
And this plugin is also useful for people who aren't from China as QQ Music also has bunch of songs in other languages. You can disable the translation in the plugin settings.

## Thanks
Thanks to [cqjjjzr](https://github.com/cqjjjzr) who developed [MusicBee-NeteaseLyrics](https://github.com/cqjjjzr/MusicBee-NeteaseLyrics).
This plug-in was modified from his [MusicBee-NeteaseLyrics](https://github.com/cqjjjzr/MusicBee-NeteaseLyrics).

# QQ 音乐歌词

这是 MusicBee 另一个获取歌词的插件。可以从 QQ 音乐获取**同步歌词**。  

该插件可以合并歌词翻译（如果有的话）到歌词。例如：  
巴拉巴拉巴拉巴（原句）  
阿啦啦啦啦啦（翻译）  
会被合并为：巴拉巴拉巴拉巴/阿啦啦啦啦啦  

如果不想要这个功能可以在设置里面的插件设置中关闭。
<!-- 如果想设置网易云音乐中特定的歌曲，可以在 `标签 (2)` 标签页给歌曲设定 `custom10` 标签，内容为曲目的 URL，比如 `https://music.163.com/#/song?id=29126914`（可以直接从网易云网站或客户端复制下来），或者 `netease=123123`，其中 123123 是曲目的 ID，并重新搜索歌词。 -->

## 使用
从[度盘（提取码 `vede`）](https://pan.baidu.com/s/155sPKyRGEKPyn5SfNJSjVg?pwd=vede)下载，并在首选项的 `插件` 中安装（“添加插件”按钮）、启用插件，在 `标签 (2)` 标签页调整歌词的提供者优先级。

## 鸣谢
本插件修改自[MusicBee-NeteaseLyrics](https://github.com/cqjjjzr/MusicBee-NeteaseLyrics), 感谢它的开发者 [cqjjjzr](https://github.com/cqjjjzr)。