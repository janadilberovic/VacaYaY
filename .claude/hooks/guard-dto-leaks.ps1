# PreToolUse hook (matcher: Edit|Write|MultiEdit).
# Blocks sensitive members from landing in a DTO: PasswordHash, TempPassword, or a
# User-entity-typed member. DTOs cross the Business boundary — they must carry data, never
# secrets or entities (see CLAUDE.md: "return DTOs only, never the User entity").
# Scoped to src/VacaYAY.Business/DTOs/** so services/controllers that legitimately handle the
# User entity internally are not affected.
# Reads the tool-call JSON from stdin; exit 2 blocks the call (stderr is shown to Claude).

$ErrorActionPreference = 'Stop'

$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }

try {
    $payload = $raw | ConvertFrom-Json
} catch {
    exit 0
}

$filePath = [string]$payload.tool_input.file_path
if ([string]::IsNullOrWhiteSpace($filePath)) { exit 0 }

$normalizedPath = $filePath -replace '\\', '/'

# Only DTO source files.
if ($normalizedPath -inotmatch '/src/VacaYAY\.Business/DTOs/.*\.cs$') { exit 0 }

# Gather all content the tool would write. Write => .content; Edit => .new_string;
# MultiEdit => .edits[].new_string.
$chunks = @()
if ($payload.tool_input.content)    { $chunks += [string]$payload.tool_input.content }
if ($payload.tool_input.new_string) { $chunks += [string]$payload.tool_input.new_string }
if ($payload.tool_input.edits) {
    foreach ($e in $payload.tool_input.edits) {
        if ($e.new_string) { $chunks += [string]$e.new_string }
    }
}
$content = ($chunks -join "`n")
if ([string]::IsNullOrWhiteSpace($content)) { exit 0 }

# Forbidden in a DTO: the secret members, or a User-entity-typed member/collection.
# `public\s+User[\s?<]` matches a User-typed property but NOT UserDto/UserId (no boundary char).
$forbidden = @(
    '\bPasswordHash\b',
    '\bTempPassword\b',
    'public\s+User[\s?<]',
    '<User>'
)

foreach ($p in $forbidden) {
    if ($content -imatch $p) {
        [Console]::Error.WriteLine(
            "Blocked: a DTO must not expose secrets or the User entity (matched '$p'). " +
            "Return a projected DTO instead (see CLAUDE.md: return DTOs only, never the User " +
            "entity, PasswordHash, or TempPassword).")
        exit 2
    }
}

exit 0
