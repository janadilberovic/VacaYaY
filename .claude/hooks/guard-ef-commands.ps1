# PreToolUse hook (matcher: Bash).
# Blocks EF commands that mutate the database — applying/dropping is owned outside Claude.
# Note: `dotnet ef migrations add/remove` is intentionally NOT blocked (allowed).
# Reads the tool-call JSON from stdin; exit 2 blocks the call (stderr is shown to Claude).

$ErrorActionPreference = 'Stop'

$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }

try {
    $payload = $raw | ConvertFrom-Json
} catch {
    # Malformed payload: don't block, just allow.
    exit 0
}

$command = [string]$payload.tool_input.command
if ([string]::IsNullOrWhiteSpace($command)) { exit 0 }

# Normalize whitespace so "dotnet   ef  database   update" still matches.
$normalized = ($command -replace '\s+', ' ').Trim()

# Match `dotnet ef ...` and the `dotnet-ef ...` variant. Only database-mutating commands.
$patterns = @(
    'dotnet[- ]ef database update',
    'dotnet[- ]ef database drop'
)

foreach ($p in $patterns) {
    if ($normalized -imatch $p) {
        [Console]::Error.WriteLine(
            "Blocked: applying/dropping the database is owned outside Claude in this project " +
            "(see CLAUDE.md Guardrails). A human runs 'dotnet ef database update'.")
        exit 2
    }
}

exit 0
