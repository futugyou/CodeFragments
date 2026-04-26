<#
.SYNOPSIS
    Convert a value using a multiplication factor.

.DESCRIPTION
    Equivalent to scripts/convert.py. Calculates: result = value * factor
    Outputs a JSON string containing the original values and the result.

.EXAMPLE
    .\scripts\convert.ps1 -Value 26.2 -Factor 1.60934
    .\scripts\convert.ps1 -Value 75 -Factor 2.20462
#>

param (
    [Parameter(Mandatory = $true, HelpMessage = "The numeric value to convert.")]
    [double]$Value,

    [Parameter(Mandatory = $true, HelpMessage = "The conversion factor from the table.")]
    [double]$Factor
)
 
$Result = [Math]::Round($Value * $Factor, 4)

$Output = @{
    value  = $Value
    factor = $Factor
    result = $Result
}

$Output | ConvertTo-Json -Compress