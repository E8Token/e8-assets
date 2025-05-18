# JSPluginTools Test Runner
# Скрипт для запуска тестов JSPluginTools из командной строки
param(
    [switch]$EditMode,
    [switch]$PlayMode,
    [switch]$WebGL,
    [switch]$All,
    [string]$ResultsPath = "TestResults",
    [switch]$Coverage,
    [string]$Category = "JSPluginTools"
)

$unityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.16f1\Editor\Unity.exe"
$projectPath = "E:\Projects\Energy8\e8-assets"

# Создаем директорию для результатов если она не существует
if (-not (Test-Path $ResultsPath)) {
    New-Item -ItemType Directory -Path $ResultsPath | Out-Null
}

function Run-Tests {
    param (
        [string]$platform,
        [string]$resultFileName
    )
    
    $args = @(        "-batchmode",
        "-projectPath", $projectPath,
        "-runTests",
        "-testPlatform", $platform,
        "-testCategory", $Category,
        "-logFile", "$ResultsPath\$platform-log.txt"
    )
    
    if ($Coverage) {
        $args += @(
            "-enableCodeCoverage",
            "-coverageResultsPath", "$ResultsPath\Coverage",
            "-coverageOptions", "generateAdditionalMetrics;generateHtmlReport;generateBadgeReport"
        )
    }
    
    Write-Host "Запуск $platform тестов..."
    & $unityPath $args
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Тесты $platform выполнены успешно!" -ForegroundColor Green
    }
    else {
        Write-Host "Тесты $platform завершились с ошибками. Смотрите лог: $ResultsPath\$platform-log.txt" -ForegroundColor Red
    }
}

# Запуск тестов в зависимости от параметров
if ($EditMode -or $All) {
    Run-Tests -platform "EditMode" -resultFileName "editmode-results.xml"
}

if ($PlayMode -or $All) {
    Run-Tests -platform "PlayMode" -resultFileName "playmode-results.xml"
}

if ($WebGL) {
    Write-Host "Для запуска WebGL тестов необходимо создать WebGL сборку и запустить ее в браузере."
    Write-Host "Команда для создания WebGL сборки:"
    Write-Host "$unityPath -batchmode -projectPath $projectPath -executeMethod WebGLBuilder.Build -logFile $ResultsPath\webgl-build-log.txt"
}

if (-not ($EditMode -or $PlayMode -or $WebGL -or $All)) {
    Write-Host "Пожалуйста, укажите тип тестов для запуска:"
    Write-Host "   -EditMode : Запустить тесты в режиме редактора"
    Write-Host "   -PlayMode : Запустить тесты в режиме воспроизведения"
    Write-Host "   -WebGL    : Показать инструкции для тестов WebGL"
    Write-Host "   -All      : Запустить все тесты (кроме WebGL)"
    Write-Host ""
    Write-Host "Дополнительные параметры:"
    Write-Host "   -Coverage         : Включить анализ покрытия кода"
    Write-Host "   -ResultsPath path : Путь для сохранения результатов тестов (по умолчанию: TestResults)"
}
