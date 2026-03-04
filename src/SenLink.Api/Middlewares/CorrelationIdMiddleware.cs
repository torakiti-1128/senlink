using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace SenLink.Api.Middlewares
{
    /// <summary>
    /// リクエストごとに一意の識別子（Correlation ID）を生成し、ログに付与するミドルウェア。
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// リクエスト処理の際に Correlation ID を生成し、レスポンスヘッダーとログコンテキストに追加する
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // ヘッダーから取得、なければ新規生成 (GUID)
            var correlationId = GetCorrelationId(context);

            // クライアント（フロントエンド）に返すレスポンスヘッダーにも付与する
            context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);

            // Serilogの LogContext にプッシュ（このリクエスト中の全ログに自動付与される）
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        /// <summary>
        /// リクエストヘッダーから Correlation ID を取得。存在しない場合は新規生成
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static string GetCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId))
            {
                return correlationId.ToString();
            }
            return Guid.NewGuid().ToString();
        }
    }
}