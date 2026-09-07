# Día 5 - CD (Continuous Deployment)

Ejercicio de pipeline CI/CD con GitHub Actions: compila una Azure Function
(.NET 8 isolated) y la despliega automáticamente a Azure en cada push a `main`.

Ver el workflow completo y comentado en [`.github/workflows/deploy.yaml`](.github/workflows/deploy.yaml).

> **Este pipeline usa un runner self-hosted** (`runs-on: self-hosted` en el
> workflow), no los runners de GitHub. Eso significa que el job corre en una
> máquina propia (tu laptop o una VM) que vos preparás e instalás con las
> herramientas necesarias (.NET SDK, Azure CLI) — GitHub no provee esa
> máquina ni el software. Las secciones de abajo son justamente esa
> preparación: instalar dependencias, descargar el runner, registrarlo en tu
> repo y dejarlo corriendo para que pueda tomar los jobs.

## Requisitos previos

- Windows con [winget](https://learn.microsoft.com/windows/package-manager/winget/) instalado.
- Acceso de administrador en la máquina que va a correr el runner.
- Una suscripción de Azure con permisos sobre el resource group del ejercicio.
- El secret de repo `AZURE_CREDENTIALS` configurado (JSON de un service principal).

## 1. Instalar el SDK de .NET y Azure CLI

Ejecutar en PowerShell **como administrador**:

```powershell
winget install --id Microsoft.DotNet.SDK.8 --accept-package-agreements --accept-source-agreements
winget install --id Microsoft.AzureCLI --accept-package-agreements --accept-source-agreements
```

## 2. Descargar el runner de GitHub Actions

También **como administrador**:

```powershell
mkdir actions-runner; cd actions-runner
Invoke-WebRequest -Uri https://github.com/actions/runner/releases/download/v2.337.0/actions-runner-win-x64-2.337.0.zip -OutFile actions-runner-win-x64-2.337.0.zip
if((Get-FileHash -Path actions-runner-win-x64-2.337.0.zip -Algorithm SHA256).Hash.ToUpper() -ne '1150692AFA94E71F872017E254EA55B6EECE1EECE3FE7E3A6D4C93D0A1B85CFC'.ToUpper()){
    throw 'Computed checksum did not match'
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory("$PWD/actions-runner-win-x64-2.337.0.zip", "$PWD")
```

> Verifica siempre la versión y el checksum SHA256 contra la [página de releases](https://github.com/actions/runner/releases) del runner antes de ejecutar el zip.

## 3. Registrar el runner en tu repositorio

Genera un token nuevo desde tu repo en GitHub:
**Settings → Actions → Runners → New self-hosted runner**. El token es de un
solo uso y expira en aproximadamente 1 hora — no lo compartas ni lo dejes
commiteado en el repo.

```powershell
./config.cmd --url https://github.com/<tu-usuario>/<tu-repo> --token <TOKEN-GENERADO-EN-GITHUB>
```

## 4. Iniciar el runner

```powershell
./run.cmd
```

Con el runner corriendo, cualquier push a `main` (o un disparo manual desde la
pestaña *Actions* del repo) ejecuta el workflow y despliega la función a Azure.

## Notas

- El runner debe quedar corriendo (`./run.cmd`) mientras se espera que el
  workflow se ejecute; ciérralo con `Ctrl+C` cuando termines.
- Para que el runner arranque solo con Windows, instálalo como servicio con
  `./svc.sh install` (Linux/macOS) o el equivalente `./config.cmd` con
  opciones de servicio en Windows — ver la
  [documentación oficial](https://docs.github.com/actions/hosting-your-own-runners).
