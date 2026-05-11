Add-Type -AssemblyName System.Drawing

$outPath = Join-Path $PSScriptRoot "hsiv.ico"
$sizes = @(16, 32, 48, 256)
$blue = [System.Drawing.Color]::FromArgb(0, 103, 192)
$white = [System.Drawing.Color]::White

function Draw-HSIVIcon {
    param([int]$size)

    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

    # Background rounded rectangle
    $radius = [int]($size * 0.156)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc(0, 0, $radius * 2, $radius * 2, 180, 90)
    $path.AddArc($size - $radius * 2, 0, $radius * 2, $radius * 2, 270, 90)
    $path.AddArc($size - $radius * 2, $size - $radius * 2, $radius * 2, $radius * 2, 0, 90)
    $path.AddArc(0, $size - $radius * 2, $radius * 2, $radius * 2, 90, 90)
    $path.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush($blue)), $path)

    # Font
    $fontSize = [int]($size * 0.42)
    $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)

    # Calculate cell dimensions
    $hm = [int]($size * 0.12)
    $vm = [int]($size * 0.08)
    $cellW = [int](($size - $hm * 3) / 2)
    $cellH = [int](($size - $vm * 3) / 2)

    $brush = New-Object System.Drawing.SolidBrush($white)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center

    # Positions
    $x1 = $hm
    $x2 = $hm * 2 + $cellW
    $y1 = $vm
    $y2 = $vm * 2 + $cellH

    # Draw H
    $rect1 = New-Object System.Drawing.RectangleF($x1, $y1, $cellW, $cellH)
    $g.DrawString("H", $font, $brush, $rect1, $format)

    # Draw S
    $rect2 = New-Object System.Drawing.RectangleF($x2, $y1, $cellW, $cellH)
    $g.DrawString("S", $font, $brush, $rect2, $format)

    # Draw I
    $rect3 = New-Object System.Drawing.RectangleF($x1, $y2, $cellW, $cellH)
    $g.DrawString("I", $font, $brush, $rect3, $format)

    # Draw V
    $rect4 = New-Object System.Drawing.RectangleF($x2, $y2, $cellW, $cellH)
    $g.DrawString("V", $font, $brush, $rect4, $format)

    $g.Dispose()
    return $bmp
}

# Create ICO file
$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($ms)

# ICO header
$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$sizes.Count)

$imageData = @()
$offset = 6 + (16 * $sizes.Count)

foreach ($size in $sizes) {
    $bmp = Draw-HSIVIcon $size
    $pngMs = New-Object System.IO.MemoryStream
    $bmp.Save($pngMs, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $pngMs.ToArray()
    $pngMs.Dispose()
    $bmp.Dispose()
    $imageData += @{Size=$size; Data=$pngBytes; Offset=$offset}
    $offset += $pngBytes.Length
}

foreach ($img in $imageData) {
    $s = $img.Size
    $w = if ($s -ge 256) { 0 } else { $s }
    $h = if ($s -ge 256) { 0 } else { $s }
    $bw.Write([byte]$w)
    $bw.Write([byte]$h)
    $bw.Write([byte]0)
    $bw.Write([byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$img.Data.Length)
    $bw.Write([UInt32]$img.Offset)
}

foreach ($img in $imageData) {
    $bw.Write($img.Data)
}

$bw.Flush()
[System.IO.File]::WriteAllBytes($outPath, $ms.ToArray())
$bw.Dispose()
$ms.Dispose()

Write-Host "Created: $outPath"
