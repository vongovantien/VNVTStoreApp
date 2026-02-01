$content = Get-Content all_tests_results.txt
$failures = @()
for ($i = 0; $i -lt $content.Length; $i++) {
    if ($content[$i] -match "Failed\s+(VNVTStore\.\S+)") {
        $testName = $matches[1]
        $errorMsg = ""
        for ($j = $i + 1; $j -lt $i + 10 -and $j -lt $content.Length; $j++) {
            if ($content[$j] -match "Error Message:") {
                $errorMsg = $content[$j+1].Trim()
                break
            }
        }
        $failures += [PSCustomObject]@{ Test = $testName; Message = $errorMsg }
    }
}
$failures | Format-Table -AutoSize
