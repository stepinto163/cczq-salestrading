1、Installshield Limited Edition 2015下载地址（先注册）：
https://info.flexerasoftware.com/IS-EVAL-InstallShield-Limited-Edition-Visual-Studio


2、Installshield Limited Edition配置dotnet4.5.2本地化：
IDC.Setup.isl打包工程，自动匹配\Readme\Microsoft .NET Framework 4.5.2 Full.prq文件，实现离线安装dotnet

Microsoft .NET Framework 4.5.2 Full.prq文件修改信息说明：
1、file节点LocalFile属性配置dotnet安装包路径，
 <files>
    <file LocalFile="&lt;ISProjectFolder&gt;\.Net 4.5.2\NDP452-KB2901907-x86-x64-AllOS-ENU.exe" URL="http://download.microsoft.com/download/E/2/1/E21644B5-2DF2-47C2-91BD-63C560427900/NDP452-KB2901907-x86-x64-AllOS-ENU.exe" CheckSum="EE01FC4110C73A8E5EFC7CABDA0F5FF7" FileSize="0,69999448"></file>
  </files>
2、删除整个<operatingsystemconditions></operatingsystemconditions>节点

 
  
3、命令行打包：
C:\Windows\Microsoft.NET\Framework\v4.0.30319>MSBuild /p:Configuration=SingleImage d:\xyb\sumscopegit\idc-broker-client\IDC.Setup\IDC.Setup.sln 