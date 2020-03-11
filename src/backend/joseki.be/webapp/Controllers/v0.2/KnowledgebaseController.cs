﻿using System;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using webapp.Handlers;
using webapp.Models;

namespace webapp.Controllers.v0._2
{
    /// <summary>
    /// Audit data endpoints.
    /// </summary>
    [ApiController]
    [ApiVersion("0.2")]
    [Route("api/knowledgebase")]
    public class KnowledgebaseController : Controller
    {
        private static readonly ILogger Logger = Log.ForContext<KnowledgebaseController>();
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgebaseController"/> class.
        /// </summary>
        /// <param name="services">DI container.</param>
        public KnowledgebaseController(IServiceProvider services)
        {
            this.services = services;
        }

        /// <summary>
        /// Returns the knowledgebase item content.
        /// </summary>
        /// <returns>The knowledgebase item content.</returns>
        [HttpGet]
        [Route("items", Name = "get-knowledgebase-item-by-id")]
        [ProducesResponseType(200, Type = typeof(KnowledgebaseItem))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetKnowledgebaseItemById([FromQuery]string id)
        {
            var unescapedId = HttpUtility.UrlDecode(id);
            try
            {
                var handler = this.services.GetService<GetKnowledgebaseItemsHandler>();

                var details = await handler.GetItemById(unescapedId);

                return details == KnowledgebaseItem.NotFound
                    ? this.NotFound($"Item with id {id} was not found")
                    : this.StatusCode(200, details);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get knowledgebase item {ItemId}", unescapedId);
                return this.StatusCode(500, $"Failed to get knowledgebase item {unescapedId}");
            }
        }

        /// <summary>
        /// Returns all knowledgebase items.
        /// </summary>
        /// <returns>Knowledgebase items.</returns>
        [HttpGet]
        [Route("items", Name = "get-all-knowledgebase-items")]
        [ProducesResponseType(200, Type = typeof(KnowledgebaseItem[]))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<ObjectResult> GetComponentCategoriesDescriptions()
        {
            try
            {
                var handler = this.services.GetService<GetKnowledgebaseItemsHandler>();

                var items = await handler.GetAll();

                return this.StatusCode(200, items);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get knowledgebase items");
                return this.StatusCode(500, $"Failed to get knowledgebase items");
            }
        }
    }
}