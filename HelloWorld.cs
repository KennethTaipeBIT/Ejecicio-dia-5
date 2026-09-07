using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace HelloWorldApi;

public class HelloWorld
{
    // ESTO DEBEN CAMBIAR: reemplazar "HelloWorld" por el nombre propio (ej: "Kenneth").
    // Cambia la URL del endpoint (/api/<Nombre>) y la clase puede renombrarse igual.
    // Si se cambia aca, tambien hay que actualizar la URL en el paso de validacion
    // del workflow (.github/workflows/deploy.yaml).
    [Function("HelloWorld")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("Hola mi nombre");
        return response;
    }
}
