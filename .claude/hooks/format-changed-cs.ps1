# PostToolUse hook (matcher: Edit|Write|MultiEdit).
# Runs `dotnet format` scoped to the single changed .cs file so style stays consistent.
# PostToolUse cannot block; this only tidies. Non-.cs edits exit immediately.

$ErrorActionPreference = 'SilentlyContinue'

$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }

try {
    $payload = $raw | ConvertFrom-Json
} catch {
    exit 0
}

$filePath = [string]$payload.tool_input.file_path
if ([string]::IsNullOrWhiteSpace($filePath) -or $filePath -notmatch '\.cs$') { exit 0 }
if (-not (Test-Path $filePath)) { exit 0 }

# Find the owning .csproj by walking up from the file's directory.
$dir = Split-Path -Parent $filePath
$project = $null
while ($dir -and (Test-Path $dir)) {
    $csproj = Get-ChildItem -Path $dir -Filter '*.csproj' -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($csproj) { $project = $csproj.FullName; break }
    $parent = Split-Path -Parent $dir
    if ($parent -eq $dir) { break }
    $dir = $parent
}

if (-not $project) { exit 0 }

# Scope to just the one file; --no-restore keeps it fast. Swallow output/errors — best-effort tidy.
& dotnet format "$project" --include "$filePath" --no-restore 2>&1 | Out-Null

exit 0
