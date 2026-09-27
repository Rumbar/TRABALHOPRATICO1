using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Middleware
{
    /// <summary>
    /// Captura as excecoes nao tratadas do pipeline e devolve uma resposta JSON padronizada,
    /// evitando que detalhes internos da aplicacao cheguem ao cliente.
    /// </summary>
    public class TratamentoErrosMiddleware
    {
        private readonly RequestDelegate _proximo;
        private readonly ILogger<TratamentoErrosMiddleware> _logger;

        public TratamentoErrosMiddleware(RequestDelegate proximo, ILogger<TratamentoErrosMiddleware> logger)
        {
            _proximo = proximo;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            try
            {
                await _proximo(contexto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Conflito de concorrencia ao gravar no banco de dados.");
                await EscreverResposta(contexto, StatusCodes.Status409Conflict,
                    "O registro foi alterado por outra operacao. Recarregue os dados e tente novamente.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Falha ao gravar no banco de dados.");
                await EscreverResposta(contexto, StatusCodes.Status409Conflict,
                    "Nao foi possivel gravar os dados. Verifique as restricoes do banco (chaves duplicadas ou registros vinculados).");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operacao invalida.");
                await EscreverResposta(contexto, StatusCodes.Status400BadRequest,
                    "Operacao invalida para os dados informados.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro nao tratado na requisicao {Metodo} {Rota}.",
                    contexto.Request.Method, contexto.Request.Path);
                await EscreverResposta(contexto, StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro inesperado ao processar a requisicao.");
            }
        }

        private static async Task EscreverResposta(HttpContext contexto, int statusCode, string mensagem)
        {
            if (contexto.Response.HasStarted)
                return;

            contexto.Response.Clear();
            contexto.Response.StatusCode = statusCode;
            contexto.Response.ContentType = "application/json; charset=utf-8";

            var corpo = JsonSerializer.Serialize(new
            {
                status = statusCode,
                mensagem,
                rota = contexto.Request.Path.Value,
                momento = DateTime.Now
            });

            await contexto.Response.WriteAsync(corpo);
        }
    }
}
