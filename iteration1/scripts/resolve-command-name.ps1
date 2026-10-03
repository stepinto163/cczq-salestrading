function Resolve-CommandName {
    param(
        [string]$Value,
        [string[]]$Choices,
        [hashtable]$Aliases = @{},
        [string]$Label = 'name'
    )

    $name = ([string]$Value).Trim()
    if (!$name) { throw "Missing $Label. Choices: $($Choices -join ', ')" }
    foreach ($choice in $Choices) {
        if ([string]::Equals($choice, $name, [StringComparison]::OrdinalIgnoreCase)) {
            return $choice
        }
    }
    foreach ($alias in $Aliases.Keys) {
        if ([string]::Equals($alias, $name, [StringComparison]::OrdinalIgnoreCase)) {
            return $Aliases[$alias]
        }
    }
    $matches = @(
        foreach ($choice in $Choices) {
            if ($choice.StartsWith($name, [StringComparison]::OrdinalIgnoreCase)) { $choice }
        }
        foreach ($alias in $Aliases.Keys) {
            if ($alias.StartsWith($name, [StringComparison]::OrdinalIgnoreCase)) { $Aliases[$alias] }
        }
    ) | Sort-Object -Unique
    $matches = @($matches)
    if ($matches.Count -eq 1) { return $matches[0] }
    if ($matches.Count -gt 1) {
        throw "Ambiguous $Label '$Value'. Matches: $($matches -join ', '). Use a longer prefix."
    }
    throw "Unknown $Label '$Value'. Choices: $($Choices -join ', ')"
}
