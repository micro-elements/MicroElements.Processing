// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Processing.SignalR
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSignalRProcessNotifier(
            this IServiceCollection services)
        {
            // SignalR.
            services.AddSignalR();

            // IProcessNotifier implemented with SignalR ProcessNotifier.
            services.AddSingleton<IProcessNotifier, ProcessNotifierSignalR>();

            return services;
        }

        public static void UseSignalRProcessNotifier(
            this IApplicationBuilder app,
            string pattern = "/ProcessNotifier",
            Action<HttpConnectionDispatcherOptions> action = null)
        {
            app.UseEndpoints(endpoints =>
            {
                // Maps incoming requests with the specified path to the specified ProcessNotifierHub type.
                endpoints.MapHub<ProcessNotifierHub>(pattern, action);
            });
        }
    }
}
