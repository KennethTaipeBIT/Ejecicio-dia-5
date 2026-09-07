# Día 5 - CD (Continuous Deployment)

Ejercicio de pipeline CI/CD con GitHub Actions: compila una Azure Function
(.NET 8 isolated) y la despliega automáticamente a Azure en cada push a `main`.

**Para resolver el ejercicio:** seguir la guía paso a paso en
[`ejercicio-ci-cd.yaml`](ejercicio-ci-cd.yaml) y construir el propio
`.github/workflows/deploy.yaml` completando cada TODO. Recién al final,
comparar contra la solución completa y comentada en
[`.github/workflows/deploy.yaml`](.github/workflows/deploy.yaml) de este repo.

> **Este pipeline usa un runner self-hosted** (`runs-on: self-hosted` en el
> workflow), no los runners de GitHub. Esto significa que el job se ejecuta en
> una máquina propia (una laptop o una VM) en la que se instalan previamente
> las herramientas necesarias (.NET SDK, Azure CLI) — GitHub no provee esa
> máquina ni el software. Las secciones siguientes cubren esa preparación:
> instalar dependencias, descargar el runner, registrarlo en el repositorio y
> dejarlo en ejecución para que pueda tomar los jobs.

## 0. Personalizar el ejercicio

Cada alumno debe adaptar dos nombres antes de desplegar:

1. **Nombre de la Function App**: variable `AZURE_FUNCTIONAPP_NAME` en
   [`.github/workflows/deploy.yaml`](.github/workflows/deploy.yaml) — debe
   coincidir con el recurso creado en Azure (`func-<nombre>-<apellido>`).
2. **Nombre de la función**: atributo `[Function("HelloWorld")]` en
   [`HelloWorld.cs`](HelloWorld.cs) — define la ruta pública del endpoint
   (`/api/<Nombre>`). Si se cambia, hay que actualizar también la URL en el
   paso *Validar que la función responde* del workflow para que apunte al
   nuevo nombre.

## Requisitos previos

- Windows con [winget](https://learn.microsoft.com/windows/package-manager/winget/) instalado.
- Acceso de administrador en la máquina que va a ejecutar el runner.
- Una suscripción de Azure con permisos sobre el resource group del ejercicio.
- El secret de repositorio `AZURE_CREDENTIALS` configurado (JSON de un service principal).

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

> Verificar siempre la versión y el checksum SHA256 contra la [página de releases](https://github.com/actions/runner/releases) del runner antes de ejecutar el zip.

## 3. Registrar el runner en el repositorio

Generar un token nuevo desde el repositorio en GitHub:
**Settings → Actions → Runners → New self-hosted runner**. El token es de un
solo uso y expira en aproximadamente 1 hora — no debe compartirse ni quedar
commiteado en el repositorio.

```powershell
./config.cmd --url https://github.com/<usuario>/<repositorio> --token <TOKEN-GENERADO-EN-GITHUB>
```

## 4. Iniciar el runner

```powershell
./run.cmd
```

Con el runner en ejecución, cualquier push a `main` (o un disparo manual desde
la pestaña *Actions* del repositorio) ejecuta el workflow y despliega la
función a Azure.

## Notas

- El runner debe permanecer en ejecución (`./run.cmd`) mientras se espera que
  el workflow corra; se cierra con `Ctrl+C` al finalizar.
- Para que el runner inicie automáticamente con Windows, se puede instalar
  como servicio con `./svc.sh install` (Linux/macOS) o el equivalente
  `./config.cmd` con opciones de servicio en Windows — ver la
  [documentación oficial](https://docs.github.com/actions/hosting-your-own-runners).
