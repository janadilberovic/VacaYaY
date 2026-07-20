# PostToolUse hook (matcher: Edit|Write|MultiEdit).
# Tidies the single changed file so style stays consistent:
#   .cs                         -> dotnet format (scoped to the owning .csproj)
#   .ts/.tsx/.js/.jsx/.css/...  -> prettier --write (uses web/.prettierrc.json + .editorconfig)
# PostToolUse cannot block; this only tidies. Other file types exit immediately.

$ErrorActionPreference = 'SilentlyContinue'

$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }

try {
    $payload = $raw | ConvertFrom-Json
} catch {
    exit 0
}

$filePath = [string]$payload.tool_input.file_path
if ([string]::IsNullOrWhiteSpace($filePath) -or -not (Test-Path $filePath)) { exit 0 }

# Walk up from $startDir looking for the first directory that contains $relative.
function Find-Up([string]$startDir, [string]$relative) {
    $dir = $startDir
    while ($dir -and (Test-Path $dir)) {
        $candidate = Join-Path $dir $relative
        if (Test-Path $candidate) { return $candidate }
        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) { break }
        $dir = $parent
    }
    return $null
}

$startDir = Split-Path -Parent $filePath

if ($filePath -match '\.cs$') {
    # Find the owning .csproj by walking up, then format just this file.
    $dir = $startDir
    $project = $null
    while ($dir -and (Test-Path $dir)) {
        $csproj = Get-ChildItem -Path $dir -Filter '*.csproj' -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($csproj) { $project = $csproj.FullName; break }
        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) { break }
        $dir = $parent
    }
    if (-not $project) { exit 0 }

    # Scope to just the one file; --no-restore keeps it fast. Best-effort — swallow output/errors.
    & dotnet format "$project" --include "$filePath" --no-restore 2>&1 | Out-Null
    exit 0
}

if ($filePath -match '\.(ts|tsx|js|jsx|mjs|cjs|css)$') {
    # Use the project-local prettier if one is installed (scopes this to web/, where it lives).
    $prettier = Find-Up $startDir 'node_modules\.bin\prettier.cmd'
    if (-not $prettier) { exit 0 }

    # Prettier auto-resolves web/.prettierrc.json + .editorconfig from the file's location.
    & $prettier --write "$filePath" 2>&1 | Out-Null
    exit 0
}

exit 0
