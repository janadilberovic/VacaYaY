# PreToolUse hook (matcher: Bash).
# Blocks EF migration commands — migrations are owned outside Claude in this project.
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

# Normalize whitespace so "dotnet   ef  migrations   add" still matches.
$normalized = ($command -replace '\s+', ' ').Trim()

# Match `dotnet ef ...` and the `dotnet-ef ...` variant.
$patterns = @(
    'dotnet[- ]ef migrations add',
    'dotnet[- ]ef migrations remove',
    'dotnet[- ]ef database update',
    'dotnet[- ]ef database drop'
)

foreach ($p in $patterns) {
    if ($normalized -imatch $p) {
        [Console]::Error.WriteLine(
            "Blocked: EF migrations are owned outside Claude in this project (see CLAUDE.md Guardrails). " +
            "Edit the EF model only; a human will generate/apply the migration.")
        exit 2
    }
}

exit 0
