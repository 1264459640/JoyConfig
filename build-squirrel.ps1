# Squirrel.Windows 打包脚本
# 用于创建安装包和更新包

param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release",
    [string]$OutputPath = "Releases"
)

# 设置错误处理
$ErrorActionPreference = "Stop"

Write-Host "开始 Squirrel.Windows 打包流程..." -ForegroundColor Green
Write-Host "版本: $Version" -ForegroundColor Yellow
Write-Host "配置: $Configuration" -ForegroundColor Yellow

# 清理输出目录
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "已清理输出目录: $OutputPath" -ForegroundColor Yellow
}

# 创建输出目录
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# 发布应用程序
Write-Host "正在发布应用程序..." -ForegroundColor Cyan
dotnet publish JoyConfig.csproj `
    --configuration $Configuration `
    --framework net9.0 `
    --runtime win-x64 `
    --self-contained true `
    --output "publish" `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -p:Version=$Version

if ($LASTEXITCODE -ne 0) {
    Write-Error "发布失败"
    exit 1
}

Write-Host "应用程序发布完成" -ForegroundColor Green

# 创建 NuSpec 文件
$nuspecContent = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>JoyConfig</id>
    <version>$Version</version>
    <title>JoyConfig</title>
    <authors>JoyConfig Team</authors>
    <description>Attribute Database Editor for Game Development</description>
    <projectUrl>https://github.com/1264459640/JoyConfig</projectUrl>
    <iconUrl>https://raw.githubusercontent.com/1264459640/JoyConfig/main/public/Icon.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <copyright>Copyright © 2025</copyright>
  </metadata>
  <files>
    <file src="publish\**" target="lib\net9.0\" />
  </files>
</package>
"@

$nuspecPath = "JoyConfig.nuspec"
$nuspecContent | Out-File -FilePath $nuspecPath -Encoding UTF8
Write-Host "已创建 NuSpec 文件: $nuspecPath" -ForegroundColor Green

# 创建 NuGet 包
Write-Host "正在创建 NuGet 包..." -ForegroundColor Cyan
nuget pack $nuspecPath -OutputDirectory $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "NuGet 包创建失败"
    exit 1
}

$nupkgFile = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | Select-Object -First 1
Write-Host "NuGet 包创建完成: $($nupkgFile.Name)" -ForegroundColor Green

# 使用 Squirrel 创建安装包
Write-Host "正在使用 Squirrel 创建安装包..." -ForegroundColor Cyan

# 安装 Squirrel.Windows 工具（如果未安装）
if (!(Get-Command "squirrel" -ErrorAction SilentlyContinue)) {
    Write-Host "正在安装 Squirrel.Windows 工具..." -ForegroundColor Yellow
    dotnet tool install --global Squirrel.Windows
}

# 创建 Squirrel 发布
squirrel --releasify $nupkgFile.FullName --releaseDir $OutputPath --setupIcon "public\Icon.ico"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Squirrel 打包失败"
    exit 1
}

Write-Host "Squirrel 打包完成!" -ForegroundColor Green
Write-Host "输出目录: $OutputPath" -ForegroundColor Yellow

# 列出生成的文件
Write-Host "生成的文件:" -ForegroundColor Cyan
Get-ChildItem -Path $OutputPath | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor White
}

# 清理临时文件
Remove-Item "publish" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $nuspecPath -Force -ErrorAction SilentlyContinue

Write-Host "打包流程完成!" -ForegroundColor Green