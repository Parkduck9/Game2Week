# 프로젝트 루트를 http://localhost:<Port>/ 로 서비스하는 최소 정적 서버 (미리보기용)
param([int]$Port = 8790)
if ($env:PORT) { $Port = [int]$env:PORT }  # 미리보기 도구가 autoPort로 포트를 넘겨줄 때

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$types = @{
    '.html' = 'text/html; charset=utf-8'; '.js' = 'text/javascript'; '.css' = 'text/css'
    '.png' = 'image/png'; '.jpg' = 'image/jpeg'; '.json' = 'application/json'
    '.glb' = 'model/gltf-binary'; '.gltf' = 'model/gltf+json'; '.md' = 'text/plain; charset=utf-8'
}

$listener = [System.Net.HttpListener]::new()
$listener.Prefixes.Add("http://localhost:$Port/")
$listener.Start()
Write-Host "Serving $root at http://localhost:$Port/"

while ($listener.IsListening) {
    $ctx = $listener.GetContext()
    $res = $ctx.Response
    try {
        $rel = [Uri]::UnescapeDataString($ctx.Request.Url.AbsolutePath.TrimStart('/'))

        # POST /save?name=<file>.glb → Exports/<file>.glb 로 저장 (미리보기의 GLB 내보내기용)
        if ($ctx.Request.HttpMethod -eq 'POST' -and $rel -eq 'save') {
            $name = $ctx.Request.QueryString['name']
            if ($name -notmatch '^[A-Za-z0-9_\-]+\.glb$') { $res.StatusCode = 400; continue }
            $dir = Join-Path $root 'Exports'
            New-Item -ItemType Directory -Force $dir | Out-Null
            $ms = [IO.MemoryStream]::new()
            $ctx.Request.InputStream.CopyTo($ms)
            [IO.File]::WriteAllBytes((Join-Path $dir $name), $ms.ToArray())
            $res.StatusCode = 200
            continue
        }

        if ($rel -eq '') { $rel = 'TodoList.html' }
        $file = [IO.Path]::GetFullPath((Join-Path $root $rel))
        if ($file.StartsWith($root) -and (Test-Path $file -PathType Leaf)) {
            $bytes = [IO.File]::ReadAllBytes($file)
            $ext = [IO.Path]::GetExtension($file).ToLower()
            $res.ContentType = if ($types.ContainsKey($ext)) { $types[$ext] } else { 'application/octet-stream' }
            $res.Headers.Add('Cache-Control', 'no-store')
            $res.OutputStream.Write($bytes, 0, $bytes.Length)
        } else {
            $res.StatusCode = 404
        }
    } catch {
        $res.StatusCode = 500
    } finally {
        $res.Close()
    }
}
