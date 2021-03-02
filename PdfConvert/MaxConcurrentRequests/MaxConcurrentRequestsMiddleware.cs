﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PdfConvert.MaxConcurrentRequests
{
    public class MaxConcurrentRequestsMiddleware
    {
        private int _concurrentRequestsCount;

        private readonly RequestDelegate _next;
        private readonly MaxConcurrentRequestsOptions _options;
        private readonly MaxConcurrentRequestsEnqueuer _enqueuer;

        public MaxConcurrentRequestsMiddleware(RequestDelegate next, IOptions<MaxConcurrentRequestsOptions> options)
        {
            _concurrentRequestsCount = 0;

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (_options.LimitExceededPolicy != MaxConcurrentRequestsLimitExceededPolicy.Drop)
            {
                _enqueuer = new MaxConcurrentRequestsEnqueuer(_options.MaxQueueLength, (MaxConcurrentRequestsEnqueuer.DropMode)_options.LimitExceededPolicy, _options.MaxTimeInQueue);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != "/pdf/process")
            {
                await _next(context);
                return;
            }
            if (CheckLimitExceeded() && !(await TryWaitInQueueAsync(context.RequestAborted)))
            {
                if (!context.RequestAborted.IsCancellationRequested)
                {
                    IHttpResponseFeature responseFeature = context.Features.Get<IHttpResponseFeature>();

                    responseFeature.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    responseFeature.ReasonPhrase = "Concurrent request limit exceeded.";
                }
            }
            else
            {
                try
                {
                    await _next(context);
                }
                finally
                {
                    if (await ShouldDecrementConcurrentRequestsCountAsync())
                    {
                        Interlocked.Decrement(ref _concurrentRequestsCount);
                    }
                }
            }
        }

        private bool CheckLimitExceeded()
        {
            bool limitExceeded;

            if (_options.Limit == MaxConcurrentRequestsOptions.ConcurrentRequestsUnlimited)
            {
                limitExceeded = false;
            }
            else
            {
                int initialConcurrentRequestsCount, incrementedConcurrentRequestsCount;
                do
                {
                    limitExceeded = true;

                    initialConcurrentRequestsCount = _concurrentRequestsCount;
                    if (initialConcurrentRequestsCount >= _options.Limit)
                    {
                        break;
                    }

                    limitExceeded = false;
                    incrementedConcurrentRequestsCount = initialConcurrentRequestsCount + 1;
                }
                while (initialConcurrentRequestsCount != Interlocked.CompareExchange(ref _concurrentRequestsCount, incrementedConcurrentRequestsCount, initialConcurrentRequestsCount));
            }

            return limitExceeded;
        }

        private async Task<bool> TryWaitInQueueAsync(CancellationToken requestAbortedCancellationToken)
        {
            return (_enqueuer != null) && (await _enqueuer.EnqueueAsync(requestAbortedCancellationToken));
        }

        private async Task<bool> ShouldDecrementConcurrentRequestsCountAsync()
        {
            return (_options.Limit != MaxConcurrentRequestsOptions.ConcurrentRequestsUnlimited)
                && ((_enqueuer == null) || !(await _enqueuer.DequeueAsync()));
        }
    }
}
