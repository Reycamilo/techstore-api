using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TechStoreApi.Dtos.Common;
using HttpStatusCode = TechStoreApi.Constants.CodigosDeEstadoHttp;

namespace techstore_api.Filters
{
    public class ValidarEstadoDeModeloAtributo : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errores = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var respuesta = new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Uno o más errores de validación ocurrieron.",
                    Errors = errores
                };

                context.Result = new BadRequestObjectResult(respuesta);
            }
        }
    }
}